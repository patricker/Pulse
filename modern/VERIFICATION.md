# Pulse .NET 8 Migration Verification Checklist

Use this after converting to SDK-style and building.

## Prerequisites

- Windows 10 1809+ or Windows 11 (for WinRT LockScreen APIs)
- .NET 8 SDK 8.0.400+ (`dotnet --version`)
- Windows 11 SDK 10.0.22621 installed via VS Installer
- `git clone https://github.com/patricker/Pulse.git`
- `cd Pulse`

## Phase 1: Legacy net4.8 Build (should still work)

```cmd
# From repo root, Windows with MSBuild
msbuild Pulse/Pulse.sln /p:Configuration=Release /p:Platform="Any CPU"

# Check outputs
dir Pulse/bin/Release/*.exe
dir Pulse/bin/Release/Providers/*.dll

# Expected: Pulse.exe, PulseForm: Pulse.exe (conflict? Actually PulseForm outputs Pulse.exe too), wallhaven.dll, NASAAPOD.dll, Bing? etc

# Run legacy
Pulse/bin/Release/Pulse.exe
# Tray icon should appear, Options -> Wallhaven provider q=nature -> Preview should show thumbnails (uses new API v1)
```

Check logs: `%LOCALAPPDATA%\Pulse\Logs\Pulse_*.txt` or `Pulse/bin/Release/Logs/`

- [ ] Wallhaven API returns 24 images (log: "Wallhaven API: Returning 24 pictures")
- [ ] NASA APOD returns via api.nasa.gov (log: "Got X pictures via api.nasa.gov")
- [ ] MediaRSS loads DeviantArt RSS (log no errors)
- [ ] Wallpaper sets (check registry `HKCU\Control Panel\Desktop\Wallpaper` changes)
- [ ] Piler collage creates (check cache folder `%LOCALAPPDATA%\Pulse\` for guid.jpg)
- [ ] AeroGlass -> Accent changes taskbar color (if ColorPrevalence enabled)
- [ ] LogonBackground -> Lock screen registry set (requires admin, or logs "needs admin")

## Phase 2: Modern net8 Build

```cmd
# From repo root
dotnet restore modern/Pulse.Modern.sln
dotnet build modern/Pulse.Modern.sln -c Release

# Single-file publish
dotnet publish modern/src/Pulse/Pulse.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64-single/

# Framework-dependent
dotnet publish modern/src/Pulse/Pulse.csproj -c Release -r win-x64 --self-contained false -o publish/win-x64-fd/
```

### Check modern artifacts

- [ ] `publish/win-x64-single/Pulse.exe` exists ~50-80MB
- [ ] `publish/win-x64-fd/Pulse.dll` + `Pulse.exe` (small shim)

### Run modern

```cmd
publish/win-x64-fd/Pulse.exe
```

- [ ] Tray icon appears (WPF + WinForms `UseWindowsForms=true`)
- [ ] Options window opens, Wallhaven provider shows TopRange dropdown (visible only when OrderBy=toplist)
- [ ] Test Wallhaven search: q=nature, categories=111, purity=100, OrderBy=relevance, Desc -> Preview returns 24 thumbs
- [ ] Test Wallhaven toplist: OrderBy=toplist, TopRange=1W -> should return 24 toplist images from last week
- [ ] Test Wallhaven random: OrderBy=random -> should return random with seed logging
- [ ] Test Bing provider (if built): Should return 8 images from Bing HPImageArchive, no API key needed
- [ ] Test NASA APOD: Returns 20 images via DEMO_KEY (some videos filtered)
- [ ] Set wallpaper: Check IDesktopWallpaper path via PowerShell:
```powershell
Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
[ComImport, Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IDesktopWallpaper {
  void SetWallpaper(string id, string wp);
  string GetWallpaper(string id);
}
[ComImport, Guid("C2CF3110-460E-4FC1-B9D0-8A110D49287D")] public class DesktopWallpaperClass {}
"@
$dw = New-Object DesktopWallpaperClass -as IDesktopWallpaper
$dw.GetWallpaper($null)
```

- [ ] Span wallpaper: Set style Span in registry or via API, spans across monitors (needs 2+ monitors to visually confirm)

### Lock Screen (Win10+)

Modern LockScreen requires `net8.0-windows10.0.22621.0` TFM and WinRT.

Test with PowerShell if Pulse failed (simulates what Pulse should do):

```powershell
# Requires admin for machine policy, or user context for personalization
# This is what Desktop.SetLockScreenImage does via registry fallback
Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Personalization" -Name LockScreenImage -Value "C:\Path\To\Image.jpg" -Force
```

For real WinRT (needs packaged app or .NET 8 Windows):

```csharp
// In C# interactive or Pulse modern build
var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(@"C:\Path\To\Image.jpg");
bool ok = await Windows.System.UserProfile.UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync(file);
```

- [ ] Lock screen sets (check Settings > Personalization > Lock screen shows image)

### Accent Color

- [ ] Enable AeroGlass/Accent provider as output
- [ ] Wallpaper change triggers accent change (log: "Set modern accent color via registry: ...")
- [ ] Check registry `HKCU\Software\Microsoft\Windows\DWM\AccentColor` changed (DWORD ABGR)
- [ ] Taskbar/Start accent changes if `ColorPrevalence` enabled in Settings > Personalization > Colors

## Provider Matrix After Migration

| Provider | Old Status | New net8 Status | Notes |
|----------|-----------|-----------------|-------|
| Wallhaven | FIXED API v1 | WORKS | Needs API key for NSFW, 24/page, seed for random |
| BingWallpaper | NEW | WORKS | No key, 8 images, UHD via replace |
| NASA APOD | FIXED | WORKS | DEMO_KEY rate limited 30/hr, 50/hour, use personal key for production |
| MediaRSS | WORKS | WORKS | DeviantArt RSS still alive, generic RSS works |
| LocalDirectory | WORKS | WORKS | FS |
| WallpaperSetter | WORKS | WORKS | SPI + IDesktopWallpaper |
| Piler | FIXED | WORKS | SPI fallback |
| AeroGlassChanger | BROKEN on Win10, now FIXED via registry | WORKS | Accent provider |
| LogonBackground | BROKEN Win8+, now LockScreen | PARTIAL | Registry needs admin, WinRT needs net8 + packaged |
| GoogleImages | RETIRED | RETIRED | Replace with Bing |
| NatGeo | RETIRED | RETIRED | Replace with Bing/NASA |

## CI

- GitHub Actions `modern/.github/workflows/build.yml` should pass both legacy and modern builds on `windows-latest`

## Known Issues to Watch

- Single-file publish: `AppContext.BaseDirectory` points to extraction dir, not exe dir. `ProviderManager` uses `AppContext.BaseDirectory` + fallback to `LocalAppData\Pulse\Providers` - verify provider DLLs found
- `System.Drawing.Common` trimming: `PublishTrimmed` may trim GDI+ - keep false for now
- COM STA: `IDesktopWallpaper` must be called from STA thread - main WPF thread is STA, background timer thread may need `Dispatcher.Invoke`
- Settings.conf: Old location `Pulse/bin/Release/settings.conf` vs new `%LOCALAPPDATA%\Pulse\settings.conf` - migration should copy old to new if new not exists

## Done Criteria

- [ ] Legacy sln builds on VS2022
- [ ] Modern sln builds with `dotnet build`
- [ ] Single-file publishes
- [ ] Wallhaven + Bing + NASA + LocalDirectory providers return images
- [ ] Wallpaper sets via IDesktopWallpaper on Win11
- [ ] Accent changes via registry
- [ ] Lock screen sets (admin or WinRT)
- [ ] No WebClient obsolete warnings
- [ ] No CsQuery dependency
- [ ] Logs clean, no 401/429 unless missing API key
