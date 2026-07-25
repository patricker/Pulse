# Pulse (Revived) — True Cross-Platform

Pulse is an application that automatically changes your wallpaper (and so much more!) by downloading images from the Internet. You choose the search and filtering options, Pulse does all the heavy lifting to get you the wallpapers you want!

**Original:** Windows 7/8 and OSX with Mono (tested on 3.2.3, beta support).
**Revived (2026):** **True cross-platform .NET 8** — Windows 10/11, macOS, Linux from single codebase. **Single image library ImageSharp** (no System.Drawing.Common switching). Avalonia UI for Win/Mac/Linux, CLI for headless.

## Quick Start — Build from Just Linux!

```bash
# Cross-platform core (works on Linux, macOS, Windows) - no Windows SDK needed
dotnet restore modern/Pulse.Modern.CrossPlatform.sln
dotnet build modern/Pulse.Modern.CrossPlatform.sln -c Release

# Run CLI that tests Wallhaven + Bing providers without setting wallpaper (works on Linux)
dotnet run --project modern/src/Pulse.CLI/Pulse.CLI.csproj -c Release
# OS: Unix ... Got 5 pictures from Wallhaven ...

# Publish Linux self-contained CLI
dotnet publish modern/src/Pulse.CLI/Pulse.CLI.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/linux-x64-cli/
./publish/linux-x64-cli/Pulse.CLI

# Cross-platform Avalonia UI (Windows, macOS, Linux) - single UI, no WPF/WinForms
dotnet run --project modern/src/Pulse.Avalonia/Pulse.Avalonia.csproj -c Release

# Full Windows build (Win10 1809+ / Win11) with wallpaper setting via IDesktopWallpaper
dotnet restore modern/Pulse.Modern.sln
dotnet build modern/Pulse.Modern.sln -c Release
dotnet publish modern/src/Pulse/Pulse.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64-single/
# Publish for macOS/Linux with platform-specific wallpaper setter
dotnet publish modern/src/Pulse.Avalonia/Pulse.Avalonia.csproj -c Release -r osx-x64 --self-contained true -o publish/osx-x64/
dotnet publish modern/src/Pulse.Avalonia/Pulse.Avalonia.csproj -c Release -r linux-x64 --self-contained true -o publish/linux-x64/
```

No legacy .NET Framework 4.8 build needed anymore — `Pulse/Pulse.sln` is retired. True cross-platform is `net8.0` Abstractions + `net8.0-windows/macos/linux` platform-specific setters via `IWallpaperSetter`.

## Documentation Index

- **[REVIVAL_PLAN.md](REVIVAL_PLAN.md)** — Live endpoint validation (wallhaven API v1 works, Google dead), what was fixed, provider audit table
- **[WIN10-11-PATH.md](WIN10-11-PATH.md)** — Why Win7 breaks on Win10/11: OSVersion lie, PerMonitorV2, IDesktopWallpaper vs SPI, DWM #131 → accent registry
- **[DOTNET-CORE-MIGRATION.md](DOTNET-CORE-MIGRATION.md)** — Audit of .NET 4.0 blockers, TFM strategy, SDK conversion, AssemblyLoadContext, MSIX
- **[INTEGRATION-TESTING.md](INTEGRATION-TESTING.md)** — Full integration testing plan: what needs testing, environments (Win11 3-mon, low-end HDD, macOS, Linux, NAT), test cases, infrastructure, tools
- **[modern/LINUX-BUILD.md](modern/LINUX-BUILD.md)** — **Build from just Linux**: what builds on Linux vs needs Windows, EnableWindowsTargeting, cross-platform CLI, cross-platform solution, what doesn't work yet (wallpaper setting macOS/Linux impl)
- **[modern/VERIFICATION.md](modern/VERIFICATION.md)** — Checklist to verify builds, wallpaper setting, accent, lock screen
- **[.github/workflows/build.yml](.github/workflows/build.yml)** — CI now cross-platform: windows-latest for full modern + ubuntu-latest for Linux API providers
- **Cross-Platform UI:** `modern/src/Pulse.Avalonia/` — Avalonia 11.1 single UI for Win/Mac/Linux replacing WPF/WinForms, `modern/src/Pulse.CLI/` — headless CLI for Linux testing
- **Abstractions:** `modern/src/Pulse.Base.Abstractions/` — net8.0 pure, no OS deps, ImageSharp only, IWallpaperSetter with Windows/macOS/Linux implementations

## Providers

| Provider | Type | Status | Notes |
|----------|------|--------|-------|
| Wallhaven | Input | **FIXED** | Now hits `https://wallhaven.cc/api/v1/search` API v1, needs API key for NSFW (get from https://wallhaven.cc/settings, free 45 req/min) |
| Bing Wallpaper | Input | NEW | No key needed, 8 images daily from HPImageArchive |
| NASA APOD | Input | FIXED | Tries `api.nasa.gov/planetary/apod?api_key=DEMO_KEY` then scraping fallback, DEMO_KEY 30/hr limit - get personal key at api.nasa.gov |
| MediaRSS | Input | WORKS | Generic RSS, DeviantArt backend still alive, now with UA + DtdProcessing.Prohibit |
| LocalDirectory | Input | WORKS | FS |
| Google Images | Input | **RETIRED** | Scraping dead 2018, `gbv=1` + `imgurl=` regex gone |
| National Geographic | Input | **RETIRED** | XML endpoint died 2015 |
| WallpaperSetter | Output | WORKS | SPI + IDesktopWallpaper for multi-monitor |
| Piler | Output | FIXED | Collage, now SPI not ActiveDesktop |
| Aero Glass Color Sync | Output | FIXED | Win7 DWM #131 removed, now registry accent for Win10/11 |
| Lock Screen / Logon Background | Output | FIXED | Win7 OEMBackground dead, now Win10 lock screen via registry (admin) or WinRT (net8 build) |

## Wallhaven API Key

1. Create account at https://wallhaven.cc/join
2. Verify email
3. Go to https://wallhaven.cc/settings, copy API Key
4. In Pulse Options > Wallhaven provider > Authentication tab: User = your username, API Key = key (visible, not password)
5. For NSFW (purity 110/111), key required because guest account setting disallows

## Known Breaking Changes from Win7 → Win11 build

- `settings.conf` location: old `Pulse/bin/Release/settings.conf` vs new `%LOCALAPPDATA%\Pulse\settings.conf` (no auto-migration yet, copy manually if upgrading)
- Banned images: old BanImageKey = filename `wallhaven-zxqkdy.jpg`, new = id `zxqkdy` - old bans won't match, will reappear. See KNOWN-ISSUES if exists
- Collections: old hardcoded user `Aheres` fallback removed, now throws `Username required for collection browsing` - set ApiUsername
- TopRange UI: visible only when OrderBy=Toplist (was Area=Top List), new dropdown at 75,29

## Branches & Migration

- `master` - legacy net4.8 Win7-11 (retired, kept for archaeology)
- `v3-net8-win1011` - revival: Win10/11 compat + Wallhaven API v1 + security hardening + ImageSharp single library (you are here) — **last branch with legacy .NET Framework**
- **`main` / `v3-net8` going forward** — **true cross-platform .NET 8 only**: 
  - `net8.0` Abstractions (no OS deps, ImageSharp only)
  - Providers cross-platform (`net8.0` referencing Abstractions only, not Windows Base)
  - `IWallpaperSetter` abstraction: Windows (IDesktopWallpaper), macOS (osascript/NSWorkspace), Linux (gsettings/feh)
  - Avalonia UI `net8.0` for Win/Mac/Linux single UI, CLI `net8.0` for headless
  - No legacy build, no System.Drawing.Common switching, single image library

Legacy `Pulse/Pulse.sln` net4.8 is **no longer needed** — `modern/Pulse.Modern.CrossPlatform.sln` builds from just Linux for providers + ImageSharp + CLI. Full wallpaper setting needs Windows for IDesktopWallpaper, macOS for osascript, Linux for gsettings, but core builds everywhere.

## Links

- Original repo: https://github.com/patricker/Pulse
- Wallhaven API: https://wallhaven.cc/help/api
- NASA APOD API: https://api.nasa.gov/
- Bing HPImageArchive: https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=8&mkt=en-US


