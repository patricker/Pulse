# Pulse — Windows 10 / 11 Migration Path

> This doc is the "right path for Windows 10 + 11" continuation of REVIVAL_PLAN.md.
> Target: make Pulse, built for Windows 7, fully native on 10 and 11 while keeping Win7 compat where possible.

## TL;DR

- **Phase 1 (done, net4.8, minimal risk)**: Add app.manifest with Win10/11 GUID + PerMonitorV2 DPI, fix `Environment.OSVersion` lie, make `ProviderManager` allow newer OS, rewrite `WinAPI.Desktop` to prefer `IDesktopWallpaper` (Win8+ multi-monitor), add Span style, replace `DwmSetColorizationParameters` (#131 removed) with registry accent, replace LogonBackground OEM with LockScreen registry/WinRT stub.
- **Phase 2 (next, net8.0-windows)**: SDK-style projects, `HttpClient`, Newtonsoft.Json, `UserProfilePersonalizationSettings.TrySetLockScreenImageAsync` for lock screen, single-file self-contained exe, optional MSIX, PerMonitorV2 already default.

---

## 1. Why Win7 Code Breaks on Win10/11

### OS version detection lie
.NET Framework without manifest reports Windows 10 as `6.2` (Win8) for compat. With manifest containing `<supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />`, it reports `10.0`.

Pulse's `ProviderPlatformAttribute` filtered providers by exact major.minor:
```csharp
ppaI.MajorVersion == Environment.OSVersion.Version.Major
&& ppaI.MinorVersion == Environment.OSVersion.Version.Minor
```
So a provider marked `[ProviderPlatform(Win32NT,6,1)]` (Win7) would be hidden on Win10 even though it could work. Old `LogonBackground` and `AeroGlassChanger` were intentionally restricted to 6.1/6.2.

**Fix applied in `ProviderManager.cs`**: Allow `current.Major > required.Major` or same major with `minor >= required`. Plus log skip. Now 6.1 providers load on 10.0.

### DPI awareness
Win7 tray apps were system-DPI aware. Win10/11 with 4K multi-monitor needs PerMonitorV2, otherwise blurry thumbnails, wrong `PrimaryScreenResolution` etc. `PictureManager.PrimaryScreenResolution` uses `Screen.PrimaryScreen.Bounds` which requires DPI awareness to be correct.

**Fix**: `app.manifest` with:
```xml
<dpiAware>true</dpiAware>
<dpiAwareness>PerMonitorV2, PerMonitor</dpiAwareness>
```
WPF in .NET 8 defaults to PerMonitorV2 anyway, but for net4.8 you need manifest.

### Wallpaper APIs

| Era | API | Status on Win10/11 |
|-----|-----|-------------------|
| XP/Vista | `IActiveDesktop` COM `{75048700-...}` | Removed after Vista, `SetWallpaperUsingActiveDesktop` silently fails |
| Win7-11 | `SystemParametersInfo(SPI_SETDESKWALLPAPER)` | **Still works**, sets same wallpaper for all monitors |
| Win8+ | `IDesktopWallpaper` COM `{C2CF3110-460E-...}` / `B92B56A9-...` | **Preferred** on Win10/11, supports per-monitor, Span, etc |
| Win8+ | `WPSTYLE_SPAN = 5` (registry WallpaperStyle=22) | Used for panoramic across monitors |

**Fix in `WinAPI/Desktop.cs`**: 
- Added `IDesktopWallpaper` interface + `DesktopWallpaperClass`.
- `SetWallpaperUsingDesktopWallpaper(path, style)` tries modern API first, falls back to SPI.
- `SetWallpaperUsingSystemParameterInfo` now tries modern then fallback.
- Added `WallpaperStyle.Span`.
- `SetWallpaperType` now handles Span (22 + Tile 0) and also calls IDesktopWallpaper to apply immediately.
- `SetDesktopBackgroundColor` also sets `IDesktopWallpaper.SetBackgroundColor`.

### Accent / Aero Glass

Win7: `dwmapi.dll #127 DwmGetColorizationParameters`, `#131 DwmSetColorizationParameters` (undocumented ordinals). Transitioned color with timer for Aura effect.

Win8: DWM still has #127/#131 but behavior changed.

Win10 1507+: #131 removed / no-op. Changing glass color does nothing or throws `EntryPointNotFoundException`. Accent color moved to registry:
- `HKCU\Software\Microsoft\Windows\DWM\ColorizationColor` (DWORD ARGB)
- `HKCU\Software\Microsoft\Windows\DWM\AccentColor` (DWORD ABGR: 0xAABBGGRR)
- `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Accent\AccentColorMenu`

Plus broadcast:
```csharp
SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, 0, "ImmersiveColorSet", SMTO_ABORTIFHUNG, 1000, out _);
SendMessageTimeout(HWND_BROADCAST, 0x0320 /*WM_DWMCOLORIZATIONCOLORCHANGED*/, 0, 0, SMTO_ABORTIFHUNG, 1000, out _);
```

**Fix**: `Desktop.SetAeroColor` tries old DWM first, catches `EntryPointNotFound`, then calls `SetModernAccentColor` which writes registry + broadcasts. `AeroGlassChangerProvider` now lists 6.1,6.2,6.3,10.0 to load everywhere.

Win11 adds Mica/Acrylic, but accent registry still respected for start/taskbar. Could add DWMWA_USE_IMMERSIVE_DARK_MODE in future.

### Logon Background / Lock Screen

Win7: `%WINDIR%\System32\oobe\info\backgrounds\backgroundDefault.jpg` + `HKLM\...\LogonUI\Background OEMBackground=1`. Required admin + `Wow64DisableWow64FsRedirection`. Pulse's `OEMBackgroundManager` handled <256KB limit via `ReduceQuality`.

Win8: Removed OEMBackground.

Win10: Lock screen image is separate from logon. New APIs:
- **Unpackaged .NET 4.8**: Registry `HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization\LockScreenImage` (machine, needs admin) or `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Lock Screen\Creative`
- **Packaged / WinRT**: `Windows.System.UserProfile.LockScreen.SetImageFileAsync(IStorageFile)` or `UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync()` (requires `net8.0-windows10.0.17763.0`+ and Windows SDK, works from desktop via WinRT projection).

**Fix**:
- `OEMBackgroundProvider` now marked for 6.1,6.2,6.3,10.0 and tries `Desktop.SetLockScreenImage(path)` first on Win10+.
- `OEMBackgroundManager.SetNewPicture` checks OS: if >=10, attempts modern lock screen, returns early (avoids writing to dead oobe folder). If modern fails, still attempts OEM as fallback (some LTSC with policy).
- `Desktop.SetLockScreenImage` implements registry fallback for net4.8, logs that full WinRT needs net8 build.

### Other Win7-isms

- `TargetFrameworkVersion v3.5 Client` / `v4.0`: No TLS 1.2 by default, so https calls to wallhaven/api fail. Fixed via `ServicePointManager.SecurityProtocol |= Tls12 (3072) | Tls11 (768)`.
- `WebClient` obsolete in .NET 8, but still works in 4.8. For net8, replace with `HttpClient`.
- `System.Drawing.Common` only supported on Windows in net8 (needs `<EnableWindowsTargeting>true</EnableWindowsTargeting>` or move to ImageSharp).
- `PictureManager` uses `Screen.AllScreens` + GDI+ - works but should be DPI aware (now via manifest).

---

## 2. What We Fixed In This Pass (Phase 1)

Files changed:
- `Pulse/Pulse/Properties/app.manifest` (new) - Win10/11 GUID + PerMonitorV2 + UTF8 + longPath
- `Pulse/PulseForm/Properties/app.manifest` (new)
- `Pulse/Pulse.Base/Properties/app.manifest` (new)
- `Pulse/Pulse/Pulse.csproj` - Target 4.8 + manifest + AutoGenerateBindingRedirects
- `Pulse/PulseForm/PulseForm.csproj` - Target 4.8 + manifest
- `Pulse/Pulse.Base/Pulse.Base.csproj` - Target 4.8 + manifest
- `Pulse/Pulse.Base/Providers/ProviderManager.cs` - allow newer OS (`>=` logic)
- `Pulse/Providers/WinAPI/Desktop.cs` - complete rewrite with IDesktopWallpaper, Span, modern accent, lock screen stub
- `Pulse/Providers/AeroGlassChanger/AeroGlassChangerProvider.cs` - add 6.3 + 10.0 platform, description updated
- `Pulse/Providers/LogonBackground/OEMBackgroundProvider.cs` + `OEMBackgroundManager.cs` - add Win10+ path, 10.0 platform

Results:
- Environment.OSVersion now correctly reports 10.0 on Win10/11 (with manifest)
- Providers previously hidden on Win10 now load
- Wallpaper setting uses modern COM when available (multi-monitor friendly)
- Accent sync works on Win10/11 via registry
- Lock screen attempt on Win10/11 instead of dead oobe folder
- Span wallpaper style supported (WallpaperStyle=22)

---

## 3. Remaining Gaps for Perfect Win10/11

### Short-term (still net4.8)

- [ ] `TopRange` UI for Wallhaven toplist - ComboBox not exposed (property exists, Designer missing)
- [ ] `GoogleImages` + `NationalGeographic` - mark obsolete, return empty with log "Provider retired, use Bing/Wallhaven"
- [ ] `WinAPI.cs` - remove `IActiveDesktop` completely or keep inactive; already fixed fallback in `Piler`
- [ ] `PictureManager` - add retry + `HttpClient` UA for thumbnail fetch (currently `WebClient.OpenRead` without UA can 403 on some hosts)
- [ ] Add `app.manifest` to remaining provider csproj? Not needed (dlls), but exe's need it (done)
- [ ] Test on Windows 11 ARM? `Wow64DisableWow64FsRedirection` only needed for x86 on x64, but ARM64 has CHPE - should still work.

### Long-term (net8.0-windows)

We created templates in `/modern/`:

#### `modern/Pulse.Base.modern.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <TargetPlatformVersion>10.0.22621.0</TargetPlatformVersion>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
</Project>
```

#### `modern/Pulse.Pulse.modern.csproj`
Full WPF app targeting `net8.0-windows10.0.22621.0` (Win11 22H2 SDK) to get LockScreen API.

Key code changes for net8:

**HttpClient instead of WebClient**:
```csharp
private static readonly HttpClient _http = new(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All })
{
    DefaultRequestHeaders = { { "User-Agent", "Pulse/2.0" } }
};
var json = await _http.GetStringAsync(url);
```

**Lock Screen WinRT (real implementation)**:
```csharp
[SupportedOSPlatform("windows10.0.17763.0")]
public static async Task<bool> SetLockScreenAsync(string path)
{
    var file = await StorageFile.GetFileFromPathAsync(path);
    var settings = UserProfilePersonalizationSettings.Current;
    return await settings.TrySetLockScreenImageAsync(file);
}
```

**Wallpaper per-monitor**:
```csharp
var dw = (IDesktopWallpaper)new DesktopWallpaperClass();
uint count = dw.GetMonitorDevicePathCount();
for (uint i=0; i<count; i++)
{
    string id = dw.GetMonitorDevicePathAt(i);
    dw.SetWallpaper(id, pathForMonitor[i]);
}
```

**Accent with Win11 Mica**:
- Could set `DWMWA_USE_IMMERSIVE_DARK_MODE` or `DWMWA_SYSTEMBACKDROP_TYPE = DWMSBT_MAINWINDOW` (Mica) via `DwmSetWindowAttribute`.

** packaging**:
- MSIX with `windows.build` + `StartupTask` for autostart (instead of registry Run)
- Single-file self-contained: `<PublishSingleFile>true</PublishSingleFile>`

---

## 4. How to Build and Test on Win10/11

### Phase 1 (current, net4.8) - Works without dotnet SDK, needs MSBuild + .NET 4.8 dev pack

On Windows 10/11 with VS2022 or Build Tools:
```cmd
msbuild Pulse.sln /p:Configuration=Release /p:Platform="Any CPU"
# Outputs to Pulse\bin\Release\Pulse.exe + Providers\*.dll
```

Test manifest:
```cmd
# Check OS version now reports 10.0
Pulse\bin\Release\Pulse.exe
# In Debug output or log file: should show OS 10.0 not 6.2
```

Test wallpaper:
- Run Pulse, set provider Wallhaven `q=nature`, click Next. Check `HKCU\Control Panel\Desktop\Wallpaper` changes + `IDesktopWallpaper.GetWallpaper(null)` returns path.

Test accent (Aero Glass provider):
- Enable Aero provider as output. Should change taskbar/start accent (if ColorPrevalence enabled in Settings > Personalization > Colors > Show accent on Start/taskbar).

Test lock screen (Logon Background provider):
- On Win10/11 non-admin, `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Lock Screen\Creative` or fails gracefully with log guiding to net8 build.
- With admin, `HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization\LockScreenImage` sets machine lock.

### Phase 2 (net8)

Install .NET 8 SDK + Windows 11 SDK (10.0.22621):
```cmd
dotnet build modern/Pulse.Pulse.modern.csproj -c Release
dotnet publish modern/Pulse.Pulse.modern.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

---

## 5. Recommendations - The "Right Path"

**For quick Win10/11 ship (you can do today):**
1. Keep net4.8, use Phase 1 fixes already applied.
2. Ship with manifest, ProviderManager fix, new Desktop.cs.
3. Retire GoogleImages/NatGeo providers (log warning).
4. Document that full lock screen needs admin or net8 build.

**For proper modern app (next major version 3.0):**
1. Migrate to `net8.0-windows10.0.22621.0` SDK-style.
2. Replace all `WebClient` with `HttpClient` + `System.Text.Json` / Newtonsoft.
3. Replace `System.Drawing` with `ImageSharp` for resizing (keep Drawing for wallpaper API interop).
4. Use `IDesktopWallpaper` exclusively (drop SPI fallback except for Win7 compat shim).
5. Implement real lock screen via WinRT + accent via registry + toast notifications via `Microsoft.Toolkit.Uwp.Notifications`.
6. MSIX package for Store sideload, auto-update via `Windows.Services.Store`.
7. Add Settings UI in WinUI 3 or modern WPF (maybe keep existing WPF but style with ModernWpf or WpfUI).
8. Add per-monitor wallpaper selection.

**Why not jump straight to net8?** 
- Net4.8 still runs on Win7 (if you care, market share <3% but some users). 
- Net8 drops Win7 support (Win10+ only). So you could maintain two branches: `master` stays net4.8 with Win10 fixes, `v3` is net8 Win10+ only.

Pulse 2.x = net4.8 + Win7-11 with manifests (current work)
Pulse 3.x = net8.0-windows + Win10 1809+ only, full WinRT

---

## 6. Links

- IDesktopWallpaper docs: https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-idesktopwallpaper
- App manifest compat: https://learn.microsoft.com/en-us/windows/win32/sbscs/application-manifests
- PerMonitorV2: https://learn.microsoft.com/en-us/windows/win32/hidpi/high-dpi-desktop-application-development-on-windows
- LockScreen.SetImageFileAsync: https://learn.microsoft.com/en-us/uwp/api/windows.system.userprofile.lockscreen.setimagefileasync
- DWM Accent registry: https://www.tenforums.com/customization/40692-how-change-taskbar-color-windows-10-a.html (community reverse-engineered)
- Pulse repo: https://github.com/patricker/Pulse

---

Enjoy Windows 11!
