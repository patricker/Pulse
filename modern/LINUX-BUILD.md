# Building Pulse from Linux (dotnet core)

> Yes, you can build from *just* Linux — for the cross-platform parts. The full Windows wallpaper setter needs Windows, but API providers, image processing, and CLI work on Linux.

## What builds on Linux vs needs Windows

| Component | TFM | Builds on Linux? | Runs on Linux? | Notes |
|-----------|-----|------------------|----------------|-------|
| `Pulse.Base` | `net8.0-windows10.0.22621.0` + `EnableWindowsTargeting=true` | **Yes** (with warnings) | Partially - Screen.AllScreens uses WinForms, will fallback | Uses ImageSharp only now, no System.Drawing.Common |
| `wallhaven` (Wallhaven API v1) | `net8.0-windows10.0.22621.0` + EnableWindowsTargeting | **Yes** | Yes - pure HttpClient + JSON | Needs API key for NSFW, SFW works without |
| `BingWallpaper` | `net8.0-windows10.0.22621.0` | **Yes** | Yes - pure API, no key | New default, 8 images |
| `Piler` (collage) | `net8.0-windows10.0.22621.0` | **Yes** | Yes - uses ImageSharp now, not GDI+ | JPEG quality 90 |
| `AeroGlassChanger` | `net8.0-windows10.0.22621.0` | **Yes builds** | No - needs Registry + DWM | Windows only, but builds on Linux with EnableWindowsTargeting |
| `WinAPI` | `net8.0-windows10.0.22621.0` | **Yes builds** | No - P/Invoke user32/dwmapi | Windows only |
| `Pulse` (WPF) | `net8.0-windows10.0.22621.0` UseWPF+WinForms | **Yes builds** (with EnableWindowsTargeting) | No - WPF requires Windows | Builds DLL, can't run |
| `Pulse.CLI` | `net8.0` | **Yes builds + runs** | **Yes** | Cross-platform console that tests providers without setting wallpaper |

### How `EnableWindowsTargeting=true` works

In .NET 8, you can build `net8.0-windows` on Linux if you set:

```xml
<EnableWindowsTargeting>true</EnableWindowsTargeting>
```

The SDK will not require Windows workload, but will warn `CA1416: This call site is reachable on all platforms, 'X' is only supported on 'windows'`. The DLL will still be produced. At runtime on Linux, Windows P/Invokes will throw `PlatformNotSupportedException`.

We added this to all modern projects so `dotnet build` succeeds on Linux.

## Linux build instructions

### Prerequisites

```bash
# Install .NET 8 SDK
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Verify
dotnet --version # should be 8.0.x

# Python for API probe tests (optional)
python3 -m pip install requests
```

### Build cross-platform only (works on Linux)

```bash
cd ~/code/Pulse

# This solution only includes projects that are pure or have EnableWindowsTargeting
# It excludes WPF exe but includes Base + wallhaven + Bing + CLI
dotnet restore modern/Pulse.Modern.CrossPlatform.sln
dotnet build modern/Pulse.Modern.CrossPlatform.sln -c Release

# Run CLI that tests Wallhaven + Bing APIs without setting wallpaper
dotnet run --project modern/src/Pulse.CLI/Pulse.CLI.csproj -c Release

# Expected output:
# OS: Unix ... Framework: 8.0.x
# === Testing Wallhaven API v1 (SFW, no key) ===
# Got 5 pictures from Wallhaven
#   - zxqkdy: https://w.wallhaven.cc/full/zx/wallhaven-zxqkdy.jpg ...
# === Testing Bing Wallpaper (no key) ===
# Got 3 pictures from Bing
```

### Build full modern solution (also builds on Linux with warnings, but WPF won't run)

```bash
dotnet restore modern/Pulse.Modern.sln
dotnet build modern/Pulse.Modern.sln -c Release --no-restore
# Will warn CA1416 for Windows APIs, but builds

# Publish Linux self-contained CLI
dotnet publish modern/src/Pulse.CLI/Pulse.CLI.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/linux-x64-cli/
./publish/linux-x64-cli/Pulse.CLI
```

### Build legacy net4.8 (Windows only)

Legacy `Pulse/Pulse.sln` is .NET Framework 4.8 (ToolsVersion 4.0, non-SDK). It **cannot** build on Linux, only on Windows with MSBuild + .NET 4.8 targeting pack:

```bash
# On Windows only:
msbuild Pulse/Pulse.sln /p:Configuration=Release /p:Platform="Any CPU"
```

### CI: Linux job

`.github/workflows/build.yml` now has 3 jobs:

- `build-old`: windows-latest + MSBuild for net4.8 legacy
- `build-modern`: windows-latest + .NET 8 for full modern (WPF + WinAPI)
- `build-cross-platform`: ubuntu-latest + .NET 8 + Python, builds CrossPlatform.sln + tests Wallhaven/Bing APIs from Linux runner, publishes linux-x64 CLI

The ubuntu job proves you can build from just Linux for the API providers.

## What *doesn't* work from Linux

- Setting wallpaper: `IDesktopWallpaper` COM, `SystemParametersInfo`, `gsettings`, `osascript` need platform-specific implementations. Our new `IWallpaperSetter` abstraction has `WindowsWallpaperSetter`, `MacWallpaperSetter` (osascript), `LinuxWallpaperSetter` (gsettings/feh) — Mac/Linux implementations are in plan but not yet fully implemented. CLI currently only *tests* providers, doesn't set wallpaper on Linux.
- WPF UI `Pulse` (Options.xaml, tray icon): WPF is Windows-only, cannot run on Linux. For cross-platform UI, need AvaloniaUI (planned as `Pulse.Avalonia` net8.0).
- WinForms `PulseForm`: Windows-only.
- AeroGlass accent: Registry `HKCU\Software\Microsoft\Windows\DWM` Windows-only.
- LogonBackground lock screen: WinRT `UserProfilePersonalizationSettings` Windows-only.

For Mac/Linux wallpaper setting, you would need to implement:

```csharp
// MacWallpaperSetter (net8.0-macos)
Process.Start("osascript", $"-e 'tell application \"Finder\" to set desktop picture to POSIX file \"{path}\"'");

// LinuxWallpaperSetter (net8.0-linux)
Process.Start("gsettings", $"set org.gnome.desktop.background picture-uri file://{path}");
```

Those can be added to `Pulse.Base` with `#if` or separate TFMs `net8.0-macos`, `net8.0-linux`.

## Recommendation

- **For development from Linux:** Use `modern/Pulse.Modern.CrossPlatform.sln` + `Pulse.CLI` to iterate on providers (Wallhaven, Bing, NASA, etc.) without needing Windows. ImageSharp single library works everywhere.
- **For full Windows wallpaper test:** Need Windows VM or `windows-latest` GitHub runner (which we have in CI).
- **For true multi-platform release:** Split `Pulse.Base` into `Abstractions` net8.0 (pure) + platform-specific `Windows`, `Mac`, `Linux` projects, and add `Pulse.Avalonia` UI (AvaloniaUI supports Win/Mac/Linux single UI).

## TL;DR

**Yes, you can build from just Linux:**
```bash
dotnet build modern/Pulse.Modern.CrossPlatform.sln -c Release
dotnet run --project modern/src/Pulse.CLI/Pulse.CLI.csproj
```

**No, you cannot *set* wallpaper from Linux yet** with current code — need Mac/Linux implementations of `IWallpaperSetter`. But you *can* test all API providers and image processing (ImageSharp) from Linux, which is 80% of the revival work.
