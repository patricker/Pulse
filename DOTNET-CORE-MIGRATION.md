# Pulse — Migration to .NET 8 / .NET 9 (dotnet core)

> From .NET Framework 3.5 → 4.8 (Phase 1 done) → **.NET 8 SDK-style** (Phase 2, this doc)
> Goal: modern `dotnet build`, self-contained single file, Win10 1809+ APIs, maintain provider plugin model.

## Current State Audit (2026-07-24)

### Solution layout (old)
```
Pulse.sln (VS2013, Format 12.00)
  Pulse/              # WPF WinExe, Target 4.8 (was 3.5 Client), App.xaml, MainWindow, Options
  PulseForm/          # WinForms WinExe, Target 4.8 (was 4.0), ClickOnce, PublishUrl E:\Coding...
  Pulse.Base/         # Library, Target 4.8 (was 4.0), Picture*, Providers, Log, Settings
  Pulse.Forms.UI/     # WinForms controls for download queue, Target 4.8
  PulseMassDownloader/ # WinForms batch downloader, Target 4.8
  Pulse.Screensaver/  # Screensaver Form, Target 4.8 (was 4.5)
  Providers/
    WinAPI/           # P/Invoke for SPI_SETDESKWALLPAPER, IActiveDesktop, DWM #127/#131, Target 4.8
    WallpaperSetter/  # IOutputProvider using Desktop.SetWallpaperUsingSystemParameterInfo
    Piler/            # Collage via GDI+ + IActiveDesktop (now fixed to SPI)
    AeroGlassChanger/ # DwmGet/SetColorizationParameters #127/#131
    LogonBackground/  # oobe\info\backgrounds + HKLM\...\OEMBackground
    LocalDirectory/   # FS provider, WORKS
    MediaRSS/         # XDocument MediaRSS (now fixed with UA)
    NASAAPOD/         # Now supports api.nasa.gov + scraping
    NationalGeographicWallpapers/ # RETIRED
    GoogleImages/     # RETIRED
    wallbase/         # wallhaven API v1 (rewritten, CsQuery removed)
  References/
    CodePlexNewReleaseChecker.dll (29KB, CodePlex shutdown 2017, needs replacement)
    AppleScriptSlim.dll (OSX wallpaper via AppleScript, Mono 3.2.3 era)
  modern/             # Phase 2 templates started (net8.0-windows)
```

### Dependencies that break on .NET 8

| Area | Old API | .NET 8 Status | Replacement |
|------|---------|---------------|-------------|
| **Web** | `WebClient`, `CookieAwareWebClient : WebClient` with `CookieContainer` | Obsolete (`SYSLIB0014`), removed in .NET 9? | `HttpClient` + `HttpClientHandler.UseCookies=true` + `CookieContainer` or `SocketsHttpHandler` |
| **JSON** | Manual regex fallback + optional `JavaScriptSerializer` (System.Web.Extensions) | `System.Web.Extensions` not in .NET Core. `JavaScriptSerializer` gone. | `System.Text.Json` (built-in) or `Newtonsoft.Json` (package). Our new wallhaven provider already has dual parser, but should migrate to `System.Text.Json`. |
| **Imaging** | `System.Drawing.Bitmap`, `Image.FromFile`, `Graphics`, `ImageCodecInfo` etc | `System.Drawing.Common` package: Windows-only by default in .NET 6+. On Linux/macOS throws `PlatformNotSupportedException` unless `EnableUnixSupport` (removed in .NET 8) or switch to ImageSharp. | Option A: Keep Windows-only: `<UseWPF>true</UseWPF><EnableWindowsTargeting>true` + `System.Drawing.Common 8.0.0` works on Windows. Option B: Cross-platform: `SixLabors.ImageSharp 3.1.4` |
| **DPAPI** | `ProtectedData.Protect/Unprotect` in `GeneralHelper.cs` uses `System.Security.Cryptography.ProtectedData` | Not in shared framework, needs NuGet package `System.Security.Cryptography.ProtectedData 8.0.0` | Add package, or switch to `Microsoft.AspNetCore.DataProtection` for cross-platform |
| **Version check** | `CodePlexNewReleaseChecker.dll` - checks CodePlex RSS for new version | CodePlex died 2017, DLL dead. | Replace with GitHub Releases API: `https://api.github.com/repos/patricker/Pulse/releases/latest` + `Octokit` or plain HttpClient. |
| **AppleScript** | `AppleScriptSlim.dll` for OSX via Mono | Mono 3.2.3 era, not .NET 8. macOS wallpaper can be set via `osascript -e 'tell application "Finder" to set desktop picture to POSIX file "..."'` still works, but need new impl. | For `net8.0-macos` target, use `Process.Start` with osascript, or `NSWorkspace` via Xamarin.Mac. Simpler: keep Windows-only for v3, add macOS later with `net8.0-macos` TFM. |
| **Config** | `AppDomain.CurrentDomain.BaseDirectory`, `Path.Combine(..., "settings.conf")`, custom `XmlSerializable<T>` | `AppDomain` still works in .NET 8 but `BaseDirectory` is different for single-file publish (points to extraction dir). Need `AppContext.BaseDirectory` instead. | Use `AppContext.BaseDirectory` + `Environment.GetFolderPath(LocalApplicationData)` for settings/caches. |
| **COM** | `IActiveDesktop` `{75048700-...}`, `IDesktopWallpaper` `{C2CF3110-...}` via `[ComImport]` | Works in .NET 8 Windows, but must be STA thread (`[STAThread]` or `Thread.SetApartmentState`). `EnableComHosting` etc. | Keep, but ensure main thread STA. ActiveDesktop can be removed (XP only). IDesktopWallpaper is preferred and works on Win10/11. |
| **DWM** | `dwmapi.dll #127/#131 DwmGet/SetColorizationParameters` private ordinals | Removed on Win10+, throws `EntryPointNotFoundException`. Already fixed with registry fallback. | Keep fallback, or remove entirely in net8 build and use modern accent provider only. |
| **Provider loading** | `Assembly.LoadFrom(@"Providers\*.dll")` scanning | Works but loads into default ALC, locks file, no collectible unload. In .NET 8 better to use `AssemblyLoadContext` + `AssemblyDependencyResolver` for plugin isolation. | Create `ProviderLoadContext : AssemblyLoadContext` with `isCollectible:true`. |
| **Resources** | `ResXResourceReader/Writer` + `Properties/Resources.resx` | Works in SDK style, but needs `<EmbeddedResource>` + `ResXFileCodeGenerator` (still works). | No change, but designer generates `internal` class - make sure `GenerateResource` doesn't need `System.Resources.Extensions`. |
| **Threading** | `DispatcherTimer` (WPF), `System.Timers.Timer`, `ManualResetEvent` | Works, but `DispatcherTimer` requires WPF. | Keep. For cross-platform, use `PeriodicTimer` (.NET 6+). |
| **Security** | `Wow64DisableWow64FsRedirection`, registry HKLM for OEMBackground | Requires admin, same on .NET 8. | Keep, but wrap in `OperatingSystem.IsWindows()` check for cross-platform. |

### Project file legacy issues

- Old csproj: `ToolsVersion="4.0"`, `<ProductVersion>8.0.30703</ProductVersion>`, `<SccProjectName>SAK</SccProjectName>` (SourceSafe), `<BootstrapperPackage>`, `<PublishUrl>E:\Coding\Pulse\...` (local path), `<IsWebBootstrapper>`, ClickOnce cert `PulseForm_TemporaryKey.pfx` (self-signed, expired). All should be removed in SDK style.
- `app.manifest` now present for all EXEs (Phase 1). In SDK style, manifest is auto-generated unless you set `<ApplicationManifest>`.
- `app.config` exists in `Pulse/`. In SDK style, should be `appsettings.json` or keep for compat but use `System.Configuration.ConfigurationManager` package if needed (Pulse doesn't use it much).

---

## TFM Strategy

We want to keep Windows-specific wallpaper APIs but also allow core logic to be cross-platform (e.g., for server-side batch downloader).

**Recommended:**

- **Pulse.Base**: `net8.0-windows` (or dual target `net8.0;net8.0-windows` with `#if WINDOWS` for WinAPI). Simplest: `net8.0-windows` only for v3, since wallpaper setting is Windows. If you want cross-platform later, split into `Pulse.Base` (net8.0, no WinAPI) + `Pulse.Base.Windows` (net8.0-windows, depends on Base, contains WinAPI/* providers).
- **Providers/WinAPI**: `net8.0-windows` (P/Invoke)
- **Providers/wallbase (Wallhaven)**: `net8.0` (pure HttpClient + JSON, no OS specific) -> can be reused for any OS
- **Providers/*Output** (WallpaperSetter, Piler, AeroGlassChanger, LogonBackground): `net8.0-windows`
- **Providers/*Input** (LocalDirectory, MediaRSS, NASAAPOD, BingNew, Unsplash): `net8.0` if possible, `net8.0-windows` if they use System.Drawing.
- **Pulse/Pulse (WPF)**: `net8.0-windows10.0.22621.0` - this TFM gives WinRT APIs like `UserProfilePersonalizationSettings.TrySetLockScreenImageAsync` for lock screen, `Windows.System.UserProfile.LockScreen`, `Windows.Storage.StorageFile`.
  - `10.0.17763.0` = Win10 1809 minimum for `UserProfilePersonalizationSettings`
  - `10.0.22621.0` = Win11 22H2 SDK, latest stable
- **PulseForm (WinForms)**: Either keep `net8.0-windows` (WinForms) or merge into WPF. Pulse has *two* UIs (WPF Options + WinForms PulseOptions) - duplication from CodePlex era when WPF was new. For v3, consolidate to single WPF with ModernWpfUI or WpfUI (modern Win11 look).

**Solution:**

```
Pulse.Modern.sln
  src/
    Pulse.Base/                 net8.0-windows
    Pulse.Base.Abstractions/    net8.0 (interfaces IInputProvider etc, no OS)
    Providers/
      WinAPI/                   net8.0-windows
      wallbase/                 net8.0 (or net8.0-windows for bitmap?)
      NASAAPOD/                 net8.0
      BingWallpaper/            net8.0 (new)
      etc.
    Pulse/                      net8.0-windows10.0.22621.0 (WPF)
    Pulse.CLI/                  net8.0-windows (optional headless console for testing providers)
  tests/
    Pulse.Tests/                net8.0
```

---

## Phase 0: Completed (Win10/11 compat on net4.8)

- [x] Manifests with Win10 GUID + PerMonitorV2
- [x] ProviderManager allow newer OS
- [x] TLS 1.2 + User-Agent
- [x] Wallhaven API v1 rewrite
- [x] NASA APOD API + fixed scraping
- [x] MediaRSS UA + null guards
- [x] Piler ActiveDesktop → SPI
- [x] WinAPI Desktop: IDesktopWallpaper, Span, modern accent, lock screen stub
- [x] AeroGlassChanger: add 6.3+10.0
- [x] LogonBackground: add Win10 path
- [x] Bump all csproj to 4.8
- [x] Retire GoogleImages/NatGeo
- [x] TopRange UI

---

## Phase 1: SDK Conversion (Start Here)

### 1.1 Install tools

- .NET 8 SDK (8.0.400+) from https://dotnet.microsoft.com
- Windows 11 SDK 10.0.22621 via Visual Studio Installer (Individual Components)
- `dotnet tool install -g try-convert` (optional, auto-converts old csproj)

### 1.2 Convert one project at a time (example: Pulse.Base)

Old `Pulse.Base.csproj` (97 lines, ToolsVersion 4.0, explicit Compile Includes):

New SDK-style (15 lines):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows10.0.22621.0</TargetFramework>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>
    <LangVersion>12</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <!-- System.Drawing.Common on Windows only -->
    <EnableWindowsTargeting>true</EnableWindowsTargeting>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.Drawing.Common" Version="8.0.8" />
    <PackageReference Include="System.Security.Cryptography.ProtectedData" Version="8.0.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <!-- Replace CodePlex checker -->
    <!-- <PackageReference Include="Octokit" Version="13.0.1" /> -->
  </ItemGroup>

  <!-- No need to list Compile Include - SDK globbing auto includes **/*.cs -->
</Project>
```

For `Pulse.csproj` (WPF exe):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.22621.0</TargetFramework>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>
    <ApplicationIcon>Icon.ico</ApplicationIcon>
    <ApplicationManifest>Properties\app.manifest</ApplicationManifest>
    <AssemblyName>Pulse</AssemblyName>
    <RootNamespace>Pulse</RootNamespace>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Pulse.Base\Pulse.Base.csproj" />
    <ProjectReference Include="..\Providers\WinAPI\WinAPI.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```

### 1.3 Fix code breaks after conversion

**a) `AppDomain.CurrentDomain.BaseDirectory` → `AppContext.BaseDirectory`**

For single-file publish, `BaseDirectory` points to extraction temp. Use:

```csharp
string baseDir = AppContext.BaseDirectory; // works for single-file + normal
// Also for settings:
string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pulse");
Directory.CreateDirectory(appData);
```

**b) `WebClient` → `HttpClient`**

Old:
```csharp
using (var wc = new WebClient()) { wc.Headers.Add(...); string s = wc.DownloadString(url); }
```

New:
```csharp
private static readonly HttpClient _http = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All })
{
    DefaultRequestHeaders = { { "User-Agent", "Pulse/2.0" } }
};
string s = await _http.GetStringAsync(url);

// For CookieContainer:
var handler = new HttpClientHandler { UseCookies = true, CookieContainer = new CookieContainer() };
var http = new HttpClient(handler);
```

**c) `System.Drawing` → keep for Windows**

Add to csproj:
```xml
<EnableWindowsTargeting>true</EnableWindowsTargeting>
```
Or migrate to ImageSharp:

```csharp
// Old
using (Bitmap bmp = (Bitmap)Bitmap.FromFile(path))
{
    Color c = PictureManager.CalcAverageColor(bmp);
}

// New with ImageSharp
using var img = await Image.LoadAsync<Rgba32>(path);
var avg = GetAverageColor(img); // custom
```

**d) `ProtectedData`**

Add package `System.Security.Cryptography.ProtectedData`. Code stays same, but `Encoding.Default` is deprecated - use `Encoding.UTF8`.

**e) `BinaryFormatter`?**

Search codebase - no BinaryFormatter found (good, it's obsolete and disabled in .NET 8). `XmlSerializable<T>` used for settings - that uses `XmlSerializer`, which still works.

**f) COM STA**

Main needs `[STAThread]` on `Main()`. For WPF, App.xaml already STA. For background threads using COM (ActiveDesktop, IDesktopWallpaper), use `Thread.SetApartmentState(ApartmentState.STA)` or `Task` with STA? In .NET 8, `Thread` still supports STA, but better to use single STA thread for COM.

### 1.4 Provider loading — AssemblyLoadContext

Old `ProviderManager.FindProviders()`:
```csharp
var assembly = Assembly.LoadFrom(f);
var types = assembly.GetTypes().Where(t => typeof(IProvider).IsAssignableFrom(t));
```

New with collectible ALC:

```csharp
public class ProviderLoadContext : AssemblyLoadContext
{
    private AssemblyDependencyResolver _resolver;
    public ProviderLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? asmPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (asmPath != null) return LoadFromAssemblyPath(asmPath);
        return null;
    }
}

// Usage
var alc = new ProviderLoadContext(dllPath);
var asm = alc.LoadFromAssemblyPath(dllPath);
var types = asm.GetTypes().Where(t => typeof(IProvider).IsAssignableFrom(t));
// Optional: unload when provider disabled
// alc.Unload();
```

Also consider `Microsoft.Extensions.DependencyModel` or `McMaster.NETCore.Plugins`.

---

## Phase 2: API Modernization

### 2.1 Replace CodePlex checker with GitHub Releases

Old `CodePlexNewReleaseChecker.dll` - delete.

New:

```csharp
public static async Task<string?> GetLatestVersionAsync()
{
    using var http = new HttpClient();
    http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Pulse", "2.0"));
    var json = await http.GetStringAsync("https://api.github.com/repos/patricker/Pulse/releases/latest");
    var doc = System.Text.Json.JsonDocument.Parse(json);
    return doc.RootElement.GetProperty("tag_name").GetString(); // e.g. "v2.1"
}
```

### 2.2 System.Text.Json for Wallhaven

Our new provider already has manual regex fallback for .NET 4.0. In .NET 8, use:

```csharp
record WallhavenThumbs(string large, string original, string small);
record WallhavenWallpaper(string id, string url, string path, WallhavenThumbs thumbs);
record WallhavenMeta(int current_page, int last_page, int per_page, int total, string? seed);
record WallhavenResponse(List<WallhavenWallpaper> data, WallhavenMeta meta);

var resp = await http.GetFromJsonAsync<WallhavenResponse>(apiUrl);
```

### 2.3 HttpClientFactory / resilience

Add `Polly` for retry on 429 rate limit:

```csharp
var policy = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.TooManyRequests)
    .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)));
```

### 2.4 Image handling

`PictureManager.ShrinkImage` uses GDI+ - keep for Windows, but for Linux/macOS (if you go cross-platform) use ImageSharp:

```csharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

using var img = await Image.LoadAsync(path);
img.Mutate(x => x.Resize(newWidth, newHeight));
await img.SaveAsync(outPath, new JpegEncoder { Quality = 90 });
```

---

## Phase 3: UI Consolidation

Currently two UIs:

- `Pulse` (WPF): `App.xaml.cs`, `MainWindow.xaml`, `Options.xaml` - tray icon via `System.Windows.Forms.NotifyIcon` (WinForms in WPF)
- `PulseForm` (WinForms): `PulseHost`, `PulseOptions`, `BannedImageReview` - ClickOnce deployment (dead)

For .NET 8, consolidate to **single WPF with ModernWpfUI** (Win11 style):

- Keep WPF project `Pulse` as main
- Use `WpfUI` NuGet (https://github.com/lepoco/wpfui) for Win11 controls, or `ModernWpfUI`
- Tray icon: `Hardcodet.NotifyIcon.Wpf` or `NotifyIcon` from WinForms still works in WPF (UseWindowsForms=true)
- Options window: single `Options.xaml` with tabs for providers

Alternatively, go **WinUI 3** (Windows App SDK) for true Win11 look, but requires MSIX packaging and drops Win7 support (which is fine for v3).

**Steps:**

1. Create new WPF window `MainWindow` with `WpfUI` - navigation view, provider list, preview.
2. Port `PulseOptions` logic (Provider ComboBox, Settings) to WPF.
3. Remove `PulseForm` project (or keep as legacy, but not in modern sln).

---

## Phase 4: Plugin System

Current: `Providers/*.dll` in `bin/Release/Providers`, loaded via `Assembly.LoadFrom`.

Modern:

- Use `AssemblyLoadContext` collectible as above
- Optional: use `MEF2` (`System.Composition`) with `[Export(typeof(IInputProvider))]`
- For strong typing, define `IProvider` in `Pulse.Base.Abstractions` (net8.0, no Windows deps), then providers reference Abstractions.

Example:

```csharp
// Pulse.Base.Abstractions (net8.0)
public interface IInputProvider
{
    Task<PictureList> GetPicturesAsync(PictureSearch search);
}

// Provider (net8.0-windows)
[Description("Wallhaven")]
public class WallhavenProvider : IInputProvider { ... }
```

Also add provider manifest `provider.json` for metadata (name, version, author, icon) instead of relying solely on `[Description]`.

---

## Phase 5: Packaging & Distribution

### Old

- ClickOnce via `PulseForm` project (PublishUrl `E:\Coding\Pulse\Publications\`, CodePlex URL `http://pulse.codeplex.com/releases/clickonce/`), cert `PulseForm_TemporaryKey.pfx` (expired). Dead.

### New

**Option A: Single-file self-contained exe (simplest)**

```xml
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>true</SelfContained>
<RuntimeIdentifier>win-x64</RuntimeIdentifier>
<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
```

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
# -> Pulse.exe 50-80MB single file, no install needed, includes .NET runtime
```

**Option B: MSIX (Store-ready, auto-update, Start menu)**

- Create `Pulse.Package.wapproj` (Windows Application Packaging Project)
- Add `<AppxManifest>` with `windows.fullTrustProcess`
- Benefits: auto-startup via `StartupTask`, clean uninstall, Store distribution

**Option C: Chocolatey / Winget**

- `winget` manifest in `winget-pkgs` repo: `patricker.Pulse`
- Or `scoop` bucket.

**Auto-update**:

- Old: CodePlex checker. New: GitHub Releases API + Squirrel or Velopack? Squirrel.Windows is popular for self-contained auto-update without MSIX.
- Simple: on startup, check `https://api.github.com/repos/patricker/Pulse/releases/latest`, compare `Version`, show toast "Update available - Download".

---

## Phase 6: New Providers (Win10/11 era)

Now that infrastructure is modern, add providers that make sense for 2026:

- **Bing Wallpaper** (daily, 8 images): `https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=8&mkt=en-US`
  - Returns JSON with `images[].url, copyright, title`. No API key needed.
- **Unsplash** (free, needs key): `https://api.unsplash.com/search/photos?query=nature&per_page=30` - `Client-ID`
- **Pexels**, **Pixabay**, **Reddit** (`/r/wallpapers+EarthPorn+.../.json`)
- **Windows Spotlight** (Win10+): `%LOCALAPPDATA%\Packages\Microsoft.Windows.ContentDeliveryManager_...\LocalState\Assets` - copy and filter >100KB jpegs.

Implement as `BingWallpaperProvider : IInputProvider` with `HttpClient`.

---

## Migration Script

We include `modern/migrate-to-sdk.ps1` (PowerShell) that:

1. Backs up old csproj to `.old.csproj`
2. Generates SDK-style csproj with `net8.0-windows10.0.22621.0`
3. Removes `<Compile Include>` (globbing)
4. Converts `packages.config` to `<PackageReference>`
5. Updates `app.manifest` reference

**Usage:**

```powershell
cd Pulse
.\modern\migrate-to-sdk.ps1 -Project Pulse/Pulse.Base/Pulse.Base.csproj -TargetFramework net8.0-windows10.0.22621.0
```

---

## Verification Checklist

After migration, test:

- [ ] `dotnet build Pulse.Modern.sln` succeeds on Windows with .NET 8 SDK
- [ ] `dotnet publish -c Release -r win-x64 --self-contained` creates single file
- [ ] Wallhaven provider returns 24 images (with API key for NSFW off)
- [ ] NASA APOD via `api.nasa.gov` returns 20 images
- [ ] Wallpaper sets via `IDesktopWallpaper` (check `GetWallpaper(null)` returns path)
- [ ] Multi-monitor: Span style works across 2 monitors
- [ ] Accent provider changes taskbar color (ColorPrevalence on)
- [ ] Lock screen provider sets lock screen (on Win10+ with admin or WinRT)
- [ ] Settings.conf saved to `%LOCALAPPDATA%\Pulse\settings.conf` not `Program Files`
- [ ] Logs go to `%LOCALAPPDATA%\Pulse\Logs\Pulse_*.txt`
- [ ] Tray icon + context menu works
- [ ] No `WebClient` obsolete warnings
- [ ] No `System.Drawing.Common` Windows-only warnings on Linux build (if multi-targeting)

---

## Recommended Branching

- `master` stays net4.8 with Win10 manifests (current Phase 1, safe, works Win7-11)
- `v3-net8` new branch for SDK migration (Phase 2+)
- Tag `v2.1-win1011` for Phase 1 release
- Tag `v3.0-net8` for first .NET 8 single-file release

---

## Links

- Try-convert tool: https://github.com/dotnet/try-convert
- SDK-style migration guide: https://learn.microsoft.com/en-us/dotnet/core/porting/
- IDesktopWallpaper: https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-idesktopwallpaper
- System.Drawing.Common breaking change: https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only
- ProtectedData: https://www.nuget.org/packages/System.Security.Cryptography.ProtectedData/
- ImageSharp: https://github.com/SixLabors/ImageSharp
- WpfUI: https://github.com/lepoco/wpfui

---

This is the right path for Win10/11 + .NET 8. Start with Phase 1 (SDK conversion of Pulse.Base + WinAPI), then iterate providers, then UI, then packaging.
