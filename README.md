# Pulse (Revived)

Pulse is an application that automatically changes your wallpaper (and so much more!) by downloading images from the Internet. You choose the search and filtering options, Pulse does all the heavy lifting to get you the wallpapers you want!

**Original:** Windows 7/8 and OSX with Mono (tested on 3.2.3, beta support).
**Revived (2026):** Windows 7-11 with .NET Framework 4.8, plus modern .NET 8 path in `modern/` folder.

## Quick Start

```bash
# Legacy .NET 4.8 build (Win7-11, works today)
msbuild Pulse/Pulse.sln /p:Configuration=Release /p:Platform="Any CPU"
# Output: Pulse/bin/Release/Pulse.exe + Providers/*.dll

# Modern .NET 8 build (Win10 1809+ / Win11)
dotnet restore modern/Pulse.Modern.sln
dotnet build modern/Pulse.Modern.sln -c Release
dotnet publish modern/src/Pulse/Pulse.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64-single/
```

## Documentation Index

- **[REVIVAL_PLAN.md](REVIVAL_PLAN.md)** — Live endpoint validation (wallhaven API v1 works, Google dead), what was fixed, provider audit table, next PRs
- **[WIN10-11-PATH.md](WIN10-11-PATH.md)** — Why Win7 code breaks on Win10/11: OSVersion lie, PerMonitorV2 DPI, IDesktopWallpaper vs SPI_SETDESKWALLPAPER, DWM #131 removed → accent registry, lock screen vs oobe, manifest with Win10 GUID
- **[DOTNET-CORE-MIGRATION.md](DOTNET-CORE-MIGRATION.md)** — Full audit of .NET 4.0 blockers (WebClient obsolete, System.Drawing.Common Windows-only, ProtectedData package, CodePlex checker dead), TFM strategy `net8.0-windows10.0.22621.0`, SDK conversion, AssemblyLoadContext, MSIX packaging
- **[modern/VERIFICATION.md](modern/VERIFICATION.md)** — Checklist to verify legacy + modern builds, wallpaper setting via IDesktopWallpaper, accent, lock screen
- **[modern/.github/workflows/build.yml](modern/.github/workflows/build.yml)** — CI for both legacy (windows-latest + MSBuild) and modern (.NET 8)

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

## Branches

- `master` - net4.8 with Win10 manifests (Phase 1, safe)
- `v3-net8-win1011` - this revival: net4.8 + modern net8 templates, security hardening, Win10/11 fixes (you are here)
- Planned `v3-net8` - full SDK migration, single-file self-contained, MSIX

## Links

- Original repo: https://github.com/patricker/Pulse
- Wallhaven API: https://wallhaven.cc/help/api
- NASA APOD API: https://api.nasa.gov/
- Bing HPImageArchive: https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=8&mkt=en-US


