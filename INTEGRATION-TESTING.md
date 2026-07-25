# Pulse Integration Testing Plan

> Branch: `v3-net8-win1011` after second adversarial review and ImageSharp single-library migration.
> Purpose: Verify all fixes from 10-persona review work on real OS, not just static analysis.

## 1. What Needs Testing (from persona reviews)

### 1.1 Core Wallpaper Pipeline (Maya casual, Alex triple-monitor, Riley designer, Terry low-end)

**Wallhaven API v1 (new provider)**
- Search `q=nature`, categories 111, purity 100, sorting relevance, page 1 → expect 24 results, path `https://w.wallhaven.cc/full/...`, thumbs `https://th.wallhaven.cc/lg/...`
- Toplist with TopRange 1w, 1M, 3M → 24 results, meta last_page, topRange param validated
- Random with seed → returns seed in meta, second page with seed returns different images not duplicate
- Purity 110/111 without API key → should 401 and fallback to SFW only (log warning, not crash)
- Collections: public collection with username + collectionID → https://wallhaven.cc/api/v1/collections/{username}/{id}, needs username, throws InvalidOperationException fixed to return empty + log
- Rate limit 429 → sleep 5s + retry 3x, then break
- MaxPictureCount capped 200 default not int.MaxValue
- Query injection: sorting/topRange/ratios/colors escaped via Uri.EscapeDataString

**Bing Wallpaper (new provider, cross-platform, no key)**
- `https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=8&mkt=en-US` → 8 images, UHD replacement `1920x1080->UHD`, fallback to original if UHD 404
- Should be default provider for first-run (Maya) - Bing active true, Wallhaven inactive false by default now
- No API key needed, works behind corporate NAT

**NASA APOD**
- api.nasa.gov with DEMO_KEY count 50 → returns 5-50 images, media_type image vs video filtered, hdurl preferred
- Scraping fallback archivepix.html 4222 matches but case-insensitive, video skip youtube/vimeo

**MediaRSS**
- DeviantArt backend RSS still alive, generic RSS, DtdProcessing.Prohibit XmlResolver null, http/https validation, private IP block, Descendants for thumbnail, medium missing handling

**Wallpaper Setting**
- `IDesktopWallpaper` full vtable 16 methods, correct out params, STA thread wrapper with Join 5000 + ReleaseComObject
- `SetWallpaperType` uncommented, style Span=22 + DWPOS_SPAN=5 across 3 monitors
- `TotalScreenResolution` union for Span, not Primary
- PerMonitorV2 manifest + Win10 GUID

**Security (Jordan)**
- Picture.Id sanitization `[^a-zA-Z0-9_-]` + containment GetFullPath StartsWith + extension whitelist .jpg/.jpeg/.png/.webp only (blocks .dll)
- PictureDownload scheme validation http/https only, private IP full CIDR (10/8, 172.16/12, 192.168/16, 0.0.0.0, 169.254/16, ::1, fc00::/7, fe80::/10) + decimal/octal IP regex, AllowAutoRedirect=false + manual redirect validation loop 3 max
- Cache symlink final path via IsReparsePoint check in GetSafeCachePath + GetFinalPath + before File.Copy
- OEM ACL ReadAndExecute|Write not FullControl, Wow64 revert finally
- Settings GetSafeCachePath blocks Windows dir, Startup folder, allows LocalAppData/AppPath/Temp only, blocks reparse parent
- ApiKey DPAPI xmlApiKey protected, redacted logs apikey=REDACTED, query newline sanitized
- TLS only 1.2 assignment = not |=
- MediaRSS XXE fixed

**Old User Migration (no migration per user note)**
- Fresh install defaults Bing, not Wallbase dead, no settings copy, banned list fresh empty

**Enterprise (Sam) + Performance (Terry)**
- Dual exe fixed PulseForm AssemblyName, workflow moved to root .github/workflows, DataPath LocalAppData Pulse, CachePath DataPath/Cache, Logs DataPath/Logs, Max 100->25, ClearOldPics false->true, Refresh 20->30, DownloadOnAppStartup true for Bing immediate, Bing default, Wallhaven inactive, Rate limit jitter still missing, MSIX packaging missing

**A11y (Kim)**
- pnlColor Visible=true Size 44x24 BorderStyle FixedSingle AccessibleName, Show checkbox 60x24, Get Key LinkLabel 70x24, TabOrder sequential, AccessibleName from label, ProviderComboBox GDI leak SolidBrush using, API key masked default + eye toggle, high contrast still hardcoded Blue/Gray, BanImage no balloon live region, TopRange overlap fixed label12 moved

**Maintainer Lee**
- SAK bindings removed, vspscc/vssscc deleted, BuildProcessTemplates deleted, References blobs deleted, packages/CsQuery deleted, legacy.cs deleted, MediaRSS dead block deleted, modern/*.modern.csproj duplicates deleted, workflow moved to root, ProductVersion/SchemaVersion removed, PulseForm duplicate TFM fixed, ClickOnce PublishUrl/Bootstrapper removed, ProviderManager partial fixed, Bing/Piler TFM fixed to net8.0-windows10.0.22621.0

## 2. Test Environments Needed

### 2.1 Windows (Primary)

- **Win11 22H2+ x64** with .NET Framework 4.8 Dev Pack + .NET 8 SDK 8.0.400 + Windows 11 SDK 10.0.22621
- **Win10 1809+** minimum for UserProfilePersonalizationSettings (17763)
- **Multi-monitor rig:** 3 monitors mixed DPI as Alex: 4K 150%, 1440p 100%, portrait 1080x1920 left of primary (negative X). Test Span 48x9 ratio 32x9/48x9.
- **Low-end rig:** 4GB RAM, HDD 5400rpm, i3, battery, metered connection simulation (Windows Settings > Network > Metered)
- **Enterprise:** Intune enrolled device behind NAT, standard user non-admin + admin user, Program Files install protected, Group Policy long paths enabled/disabled
- **High DPI:** 200% scaling, 400% scaling for reflow test

### 2.2 macOS (Multi-platform new)

- **macOS 13+** with .NET 8 SDK, Xcode command line tools
- Test MacWallpaperSetter: `osascript -e 'tell application "Finder" to set desktop picture to POSIX file "/tmp/test.jpg"'` + NSWorkspace via Xamarin.Mac
- Cache path: `~/Library/Application Support/Pulse` or `~/.local/share/Pulse`

### 2.3 Linux (Multi-platform)

- **Ubuntu 22.04+ GNOME**, **Fedora KDE**, **Mint Cinnamon**, **i3wm** with feh
- Test gsettings: `gsettings set org.gnome.desktop.background picture-uri file:///tmp/test.jpg`
- .NET 8 SDK, ImageSharp should work headless

### 2.4 Network

- **Direct internet** with Wallhaven API key (free account) + NASA DEMO_KEY + personal NASA key
- **Behind NAT** simulating 200 machines: 2 VMs behind same egress IP, run Pulse simultaneously at 9am to test 429 storm + jitter
- **Metered:** Windows metered connection enabled, check NetworkInformation skip
- **Offline:** disconnect, should return cached list from CACHE_{hash}.xml, no crash

## 3. Test Cases

### 3.1 Automated (CI)

**.github/workflows/build.yml** (now at root after fix)

- **Legacy net4.8:** `msbuild Pulse/Pulse.sln /p:Configuration=Release /p:Platform="Any CPU" /m` must succeed (no SAK, no vspscc, no CodePlex dll, no packages)
- **Modern net8:** `dotnet restore modern/Pulse.Modern.sln; dotnet build -c Release` must succeed without NU1201 (Bing TFM fixed) and without CS0260 (ProviderManager partial)
- **Provider probe:** Python `requests.get('https://wallhaven.cc/api/v1/search?q=nature', headers={'User-Agent':'Pulse/2.0'})` asserts 200 and 24 data, plus Bing `HPImageArchive.aspx` returns 8, NASA `api.nasa.gov` returns 5
- **Security static scan:** grep for `System.Drawing.Common` in modern/src should be 0, grep for `Ssl3` should be 0, grep for `AllowAutoRedirect=true` should be 0, grep for `FileWebRequest` file:// should be 0

### 3.2 Manual - Core Loop (Maya)

1. Clean `%LOCALAPPDATA%\Pulse` delete
2. Run `Pulse/bin/Release/Pulse.exe` (net4.8) or `publish/win-x64-single/Pulse.exe` (net8)
3. Check tray icon appears, not hidden? On Win11, check overflow ^. Should show balloon "Pulse is in system tray" if first run (currently missing, to be added)
4. Right-click tray → Next Picture → should download Bing UHD (8MB) and set wallpaper via IDesktopWallpaper, check `HKCU\Control Panel\Desktop\Wallpaper` changes, check `IDesktopWallpaper.GetWallpaper(null)` returns path via PowerShell helper
5. Open Options → Providers → dropdown should NOT show Google/NatGeo (RETIRED filtered), should show Bing, Wallhaven, NASA, MediaRSS, LocalDirectory
6. Select Wallhaven, Query nature, OrderBy relevance, Preview → 24 thumbs 150px, no crash, thumbnails Bilinear (modern) vs HighQualityBicubic (legacy) check CPU
7. Select Wallhaven OrderBy toplist → TopRange dropdown appears at 75,29, no overlap with Search Query label (fixed), select 1w → Preview 24 toplist
8. Color filter: Panel should be Visible true Size 44x24, click Pick Color → ColorDialog, hex label shows #AABBCC, AccessibleName, keyboard Tab to Pick Color link Enter works
9. Ban Image: Right-click tray Ban Image → should delete local file and skip to next, show balloon "Banned ... and skipped" (currently missing, should be added)
10. Check Logs in `%LOCALAPPDATA%\Pulse\Logs\Pulse_yyyyMMdd.txt` not Program Files, apikey redacted, no world-readable ACL (icacls check)

### 3.3 Multi-Monitor (Alex)

1. With 3 monitors, set wallpaper style Span in WallpaperSetter settings (Options > Output > Desktop Wallpaper > Settings > Position Span)
2. Search Wallhaven with ratio 32x9 or 48x9 (newly added) → should return triple-wide wallpapers
3. Set wallpaper, check registry WallpaperStyle=22 Tile=0, and via PowerShell `IDesktopWallpaper.GetWallpaper(null)` returns same path, Span should stretch across all 3, TotalScreenResolution union check: `Screen.AllScreens` bounds union width ~7480
4. Test per-monitor distinct: not implemented, but document same image for now
5. Piler: Set backdrop 4K + 4 pile images 2560x1440, check collage JPEG quality 90 not 75 (file size ~500KB vs 200KB), no infinite placement loop with small backdrop 800x600, check rotated bbox doesn't overlap visually
6. DPI change: Drag Options window from 150% to 100% monitor, check PrimaryScreenResolution still physical pixels 3840 not virtual 2560, thumbnails not blurry

### 3.4 Security (Jordan) - Penetration

1. **Path traversal:** Try to craft Wallhaven API mock returning `id=../../Providers/evil` + `path=https://evil.com/evil.dll` → CalculateLocalPath should sanitize to `evil.jpg` inside Cache, not Providers
2. **Extension whitelist:** URL `https://evil.com/payload.dll?.jpg` → should be forced to .jpg, not .dll
3. **Symlink:** Create junction `LocalAppData\Pulse\Cache -> C:\Windows\System32` via `mklink /J`, then try set CachePath to that junction → GetSafeCachePath should detect reparse point and block, return default
4. **File:// SSRF:** Set MediaRSS URL to `file:///C:/Windows/win.ini` → should be blocked "Only http/https allowed"
5. **Private IP SSRF:** Set MediaRSS URL to `http://127.0.0.1:2375/v1.24/containers/json` → blocked, also `http://2130706433/` (decimal 127.0.0.1) blocked, `http://0x7f.0.0.1` blocked
6. **Redirect SSRF:** Set up local http server returning 302 to `http://169.254.169.254/latest/meta-data/` → CookieAwareWebClient AllowAutoRedirect=false should not follow, or follow with validation and block private IP redirect
7. **DLL plant:** Try to write `evil.dll` to Providers via CachePath set to `.../Providers` → GetSafeCachePath should block if path is inside Startup or Windows, but allows LocalAppData\Providers? Need hash allowlist, currently allows LocalAppData\Providers, so RCE possible if attacker can write DLL there. Test: place `evil.dll` with IProvider implementation in LocalAppData\Pulse\Providers, restart Pulse, should it load? Currently yes, no signature check. Should document as known risk.
8. **API key UI:** Check eye-toggle masked default, Show checkbox 60x24, Get Key link 70x24, accessible, no shoulder surf
9. **Logs ACL:** `icacls %LOCALAPPDATA%\Pulse\Logs\Pulse_*.txt` should show only current user + SYSTEM, not Everyone
10. **TLS:** Wireshark capture wallhaven API call → should be TLS 1.2 only, no Ssl3 ClientHello

### 3.5 Enterprise (Sam)

1. **Dual exe:** `bin/Release/` should contain `Pulse.exe` (WPF) + `PulseForm.exe` (WinForms) distinct, not race overwrite
2. **CI:** Push to branch should trigger `.github/workflows/build.yml` at root, not modern/.github, both legacy and modern jobs green
3. **DataPath:** Install to `C:\Program Files\Pulse\Pulse.exe`, run as standard user, check settings saved to `%LOCALAPPDATA%\Pulse\settings.conf` not Program Files, no UAC prompt, cache to LocalAppData\Pulse\Cache
4. **NAT storm:** Simulate 2 VMs behind same IP, both start Pulse with Wallhaven default at same time → expect 429 handling sleep 5s + retry 3x, no crash, logs show rate limited
5. **Bing default:** Fresh install default provider should be Bing, not Wallhaven (to avoid NAT + API key requirement)
6. **MSIX:** Not yet implemented, but test manual xcopy deploy: `xcopy Pulse.exe "C:\Program Files\Pulse\"` + Providers, detection file exists, uninstall script cleans LocalAppData
7. **Run registry:** Check `HKCU\...\Run\Pulse` vs StartupTask, GPO User Logon Script sets Run value

### 3.6 Designer (Riley)

1. Color search reachable: Panel Visible true 44x24, click opens ColorDialog, hex label shows #AABBCC
2. Piler quality: Collage file size ~500KB at quality 90 vs old 200KB at 75, visual check no JPEG artifacts banding
3. Rotate quality: Rotated pile images edges not aliased, InterpolationMode HighQualityBicubic or Lanczos3
4. Bing UHD fallback: If UHD 404, should fallback to 1920x1080, not blank
5. Thumbnail: 150px Bilinear vs Lanczos3 - check crispness, memory usage

### 3.7 Old Upgrader (Pat) - Fresh install (no migration per your note)

1. Clean LocalAppData, run Pulse.exe, should create fresh settings with Bing active, Wallhaven inactive, no Wallbase alias needed
2. Collections without username: set Wallhaven SA=user/collection CollectionID=123 without username → should log warning + return empty list, not throw InvalidOperationException
3. AeroGlass: On Win10+, set accent, should single registry write + single broadcast, not 26 broadcasts flicker, check via Spy++ or taskbar redraw
4. LogonBackground: On Win10 non-admin, should log "needs admin for HKLM policy or net8 WinRT build" and return, no write to oobe

### 3.8 Contributor

1. Copy BingWallpaper folder to MyProvider, rename namespace, `dotnet sln add`, `dotnet build modern/Pulse.Modern.sln` should succeed without NU1201 (Bing TFM fixed) and without CS0260 (ProviderManager partial)
2. Check modern/src/Pulse.Base/Pulse.Base.csproj has EnableDefaultCompileItems=false, no PublishSingleFile in library, no duplicate EmbeddedResource+None
3. Check migrate-to-sdk.ps1 handles XML namespace, Escape-Xml, UTF8NoBOM, PackageReference migration excluding CsQuery

### 3.9 A11y (Kim)

1. TabOrder sequential: Tab through Wallbase prefs, order should be Area → Categories → Purity → Resolution → Search tab OrderBy → Direction → TopRange (when visible) → Query → Color → Pick/Clear → User → API Key → Show checkbox → Get Key link → Collections → Favorites
2. AccessibleName: All combos should have AccessibleName from label, screen reader NVDA should announce "Image Area Search ComboBox"
3. ProviderComboBox: OwnerDraw with using SolidBrush, guard e.Index<0, high contrast border, CreateAccessibilityInstance override
4. API key eye-toggle: 44x24 min target size, masked default, Show checkbox accessible, Get Key link 44x24
5. High contrast: Blue Gray hardcoded should be SystemColors, SearchIcon.png vector, BanImage balloon live region
6. TopRange overlap: label12 28,55 txtSearch 58,71 pnlColor 58,95 no overlap at 150% DPI, test at 200% scaling
7. Color panel: Button role not Pane, 44x44, hex label

### 3.10 Performance (Terry)

1. Defaults: Max 25 not 100, ClearOldPics true, ClearInterval 2, Refresh 30, concurrent 5 still? Should be 2 on HDD detection via WMI Rotational
2. Thumbnail Bilinear vs Bicubic: Modern uses Bilinear for 150px (faster), legacy still HighQualityBicubic heavy, should be Bilinear for preview
3. Cache quota: No LRU yet, should implement 500MB/200 files eviction sorted by LastAccessTime, EnumerateFiles not GetFiles
4. Metered: No NetworkInformation check, should skip on metered + battery <20%
5. Log rotation: No max 5MB/7 days, should delete old logs
6. Bing UHD forced: Always UHD 8-15MB vs FHD 2-3MB, should be optional UseUHD false or resolution match PrimaryScreenResolution
7. LocalDirectory: int.MaxValue capped to 200 now, but still OrderBy Guid.NewGuid() O(N log N) sort entire dir, should reservoir sample
8. Single-file 80MB extract: PublishSingleFile true extracts to Temp each launch, slow on HDD, should be false for HDD SKU

## 4. What We Need for Integration Testing

### 4.1 Infrastructure

- **Windows 11 VM** with 3 monitors (or Display Settings > Simulate), 4K 150% + 1440p 100% + portrait, with .NET 4.8 Dev Pack + .NET 8 SDK 8.0.400 + Windows 11 SDK 10.0.22621
- **Windows 10 1809 VM** minimum for WinRT LockScreen API 17763
- **Low-end VM** 4GB RAM, HDD (not SSD), i3, metered network toggle
- **macOS** 13+ VM or machine with .NET 8 SDK, test osascript wallpaper, ImageSharp
- **Linux** Ubuntu 22.04 GNOME + Fedora KDE + Mint Cinnamon, .NET 8 SDK, gsettings/feh
- **Network:** Direct internet + behind NAT (2 VMs same egress), Wireshark, Fiddler for TLS check, local http server for redirect SSRF test
- **Accounts:** Wallhaven free account + API key, NASA api.nasa.gov personal key + DEMO_KEY, Bing no key

### 4.2 Tools

- **Build:** VS2022 17.8+ with Desktop development .NET + Windows 11 SDK, `msbuild`, `dotnet`, `nuget`, `pip install requests`
- **Test:** `dotnet test` (once test project created), Python 3.11 for API probes, PowerShell for IDesktopWallpaper check, Registry Editor, Spy++ for HWND_BROADCAST, Process Explorer for GDI leak, Wireshark for TLS, `icacls` for ACL, `mklink /J` for junction test, NVDA screen reader, Windows High Contrast themes, Magnifier 200%/400%
- **CI:** GitHub Actions with `windows-latest`, self-hosted runner for multi-monitor? Or manual

### 4.3 Test Data

- Wallhaven API sample JSON (24 data, meta) saved as `tests/data/wallhaven_search_nature.json` for offline unit test
- Bing HPImageArchive sample JSON 8 images
- NASA APOD sample JSON 5 images
- MediaRSS sample RSS with media:content + media:thumbnail + media:group cases
- 100 banned images list old format filename vs new id for normalizer test
- 4K + 1440p + portrait test images for Piler, total resolution union test
- Symlink/junction test dirs

### 4.4 Automated Tests to Add

- **Unit:** Wallhaven parser brace counting + id sanitization + extension whitelist, Settings GetSafeCachePath blocks Windows dir + reparse point, PictureDownload private IP blocking, MediaRSS DtdProcessing.Prohibit
- **Integration:** ProviderManager finds providers in exe dir + LocalAppData fallback, duplicate name suffix (2), Obsolete filter hides retired
- **ImageSharp:** ShrinkImage center-crop Lanczos3, RotateImage high quality, AppendBorder 25px, ReduceQuality 90, CalcAverageColor 1x1
- **Security:** File:// blocked, .dll extension blocked, symlink blocked, SSRF redirect blocked, TLS only 1.2, ApiKey DPAPI + redacted logs

### 4.5 Manual Checklist (from VERIFICATION.md expanded)

- [ ] Legacy net4.8: `msbuild Pulse/Pulse.sln` succeeds after SAK/vspscc cleanup, dual exe distinct, no CodePlex dll
- [ ] Modern net8: `dotnet build modern/Pulse.Modern.sln -c Release` succeeds without NU1201/CS0260 after Bing/Piler TFM + partial fix
- [ ] Single-file publish: `dotnet publish modern/src/Pulse/Pulse.csproj -r win-x64 --self-contained true -p:PublishSingleFile=true` creates ~60MB exe, runs from LocalAppData, finds Providers in exe dir + LocalAppData fallback
- [ ] Wallhaven search 24 thumbs, toplist with TopRange, random with seed, collection needs username throws gracefully not crash
- [ ] Bing 8 UHD with fallback, NASA 20 images via DEMO_KEY
- [ ] Wallpaper set via IDesktopWallpaper on Win11, Span across 3 monitors, registry WallpaperStyle 22, TotalScreenResolution union
- [ ] Accent single broadcast on Win10+, taskbar color changes, no flicker
- [ ] Lock screen non-admin logs requirement, admin writes HKLM policy
- [ ] Color panel Visible 44x24 Button, keyboard Space/Enter opens dialog, hex label, eye-toggle 44x44, Get Key link 44x24 opens browser, TopRange no overlap at 200% DPI
- [ ] TabOrder sequential, AccessibleName for all combos, ProviderComboBox no GDI leak, high contrast SystemColors, BanImage balloon live region
- [ ] Performance: Max 25 concurrent 2, ClearOldPics true, cache quota LRU, Bilinear thumb, metered skip, log rotation
- [ ] Multi-platform: macOS osascript sets wallpaper, Linux gsettings sets, ImageSharp works headless, no System.Drawing.Common in modern

## 5. Acceptance Criteria for v3.0 Release

- Legacy net4.8: builds clean, runs on Win7-11, no SAK, no binary blobs, no vspscc, no CodePlex, no packages folder, no legacy.cs backups, no dead commented block, dual exe fixed, workflow at root .github, ProductVersion/SchemaVersion removed, ClickOnce remnants removed
- Modern net8: builds clean, single library ImageSharp only, no System.Drawing.Common, Piler/AeroGlass using ImageSharp, Bing default immediate wallpaper, retired hidden, TopRange + color panel visible 44x44, eye-toggle, STA thread + ReleaseComObject, 32x9/48x9 ratios, duplicate provider suffix, Span reachable from UI, total resolution for Span
- Security: 9/9 fixed + residual bypasses (file://, .dll ext, symlink, redirect, logs ACL, private IP full CIDR, TLS =) fixed and verified via penetration tests
- Docs: README index, no dead README, TFM consistency net4.8 vs net8.0-windows10.0.22621.0, settings location LocalAppData, no migration needed per your note, API key instructions 45 req/min
- CI: root .github/workflows/build.yml runs both legacy and modern, publishes artifacts, python probe wallhaven 24 data

## 6. Next Steps After Integration Testing

- Tag v2.1-win1011 for net4.8 with Win10 fixes
- Branch v3-net8 with full ImageSharp single library, multi-platform IWallpaperSetter, MSIX packaging, GPO ADMX template for enterprise CacheQuota/MaxDownload/EnableUHD, Winget manifest
- Remove System.Drawing.Common completely from repo (legacy Framework still uses System.Drawing from GAC, not package, so okay - only modern should have no package)
- Add CONTRIBUTING-PROVIDER.md 5-min quickstart + test harness modern/tests/Pulse.Providers.Tests
