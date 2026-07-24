# Pulse Revival Plan

> Old repo: https://github.com/patricker/Pulse  
> Status as of 2026-07-23: wallbase.cc → wallhaven.cc migration complete, provider audit done, core fixes applied.

## Summary

Pulse is a .NET Framework 4.0 wallpaper changer with pluggable Input/Output providers. Last commit 2018, target framework 4.0, uses CsQuery for scraping, WebClient without TLS1.2 or User-Agent. Most providers dead due to site redesigns + Google dropping `gbv=1`.

This plan covers:
1. Live validation results (2026-07-23 Python probe)
2. What was fixed in this pass
3. What still needs work
4. Roadmap to .NET 8 modernization

---

## 1. Live Endpoint Validation (2026-07-23)

Script: `/tmp/pulse_validate.py`

### Wallhaven (wallbase replacement)
- **Old scraping URL** `http://alpha.wallhaven.cc/search?...` → 200 but length 22k, no `figure.thumb` (alpha subdomain dead/cloudflare)
- **New scraping URL** `https://wallhaven.cc/search?q=nature...` → 200, 42k, `figure.thumb` present (still scrapable but blocked by CF bot check without proper UA eventually)
- **New API v1** `https://wallhaven.cc/api/v1/search?...`
  - `q=nature` → 200, 24 results, meta 4315 pages, sample `id=zxqkdy path=https://w.wallhaven.cc/full/zx/wallhaven-zxqkdy.jpg`
  - `sorting=random` → 200, seed returned `rOckyS` (for pagination)
  - `sorting=toplist&topRange=1M` → 200, 24 results
- **Verdict**: NEW API WORKS. Old scraping LIKELY BROKEN long-term.

### Google Images
- `images.google.com/search?tbm=isch&gbv=1&q=nature` → 200 but 2536 bytes, final URL `www.google.com/search?...`, no `imgurl=` param.
- Consent redirect / JS required. Regex dead.
- **Verdict**: BROKEN, cannot be salvaged. Needs Custom Search JSON API or retirement.

### NASA APOD
- `apod.nasa.gov/apod/archivepix.html` (http & https) → 200, 333k, regex `<a href="ap*.html">` → 4222 matches both case-sensitive/insensitive. Archive list still works.
- Individual page `<IMG SRC="image/...">` regex → 0 matches on archive (expected, because that's for detail pages; we didn't test detail page in first script, second deeper test shows detail pages still work but need IgnoreCase).
- New API `api.nasa.gov/planetary/apod?api_key=DEMO_KEY&count=5` → 200, 5 items, mix `media_type=image` and `video`.
- **Verdict**: Scraping MAYBE WORKS with fixes (https + IgnoreCase + video filter). New API is better.

### National Geographic
- `ngm.nationalgeographic.com/wallpaper/download` → 200 but redirects to `/magazine/`, 312k, no `<option value=`
- `www.nationalgeographic.com/photography/photo-of-the-day/` → 200, React SPA, no option list.
- **Verdict**: BROKEN. Legacy XML endpoint gone ~2015. Needs total rewrite or retirement.

### MediaRSS / DeviantArt
- `backend.deviantart.com/rss.xml?q=boost:popular in:customization/wallpaper` → 200, 406k, valid RSS, contains `<item>` + `media:content`.
- NASA `lg_image_of_the_day.rss` → 200, 46k, has `<item>` and enclosure.
- **Verdict**: SURPRISINGLY STILL WORKS. DeviantArt backend still returns RSS. Generic engine valid, just needs UA + null guards and https.

### LocalDirectory
- FS API, no network.
- **Verdict**: WORKS

### WallpaperSetter, Piler, LogonBackground, AeroGlassChanger
- WallpaperSetter: `SystemParametersInfo` → WORKS on Win7-11
- Piler: Uses `IActiveDesktop` COM (ActiveDesktop) → BROKEN on Win7+, needs SPI fallback → FIXED in this pass
- LogonBackground: Writes to `%WINDIR%\System32\oobe\info\backgrounds` + registry `OEMBackground` → REMOVED in Win8+, requires elevation → BROKEN on modern, WORKS on Win7 only
- AeroGlassChanger: `dwmapi.dll #127/#131 DwmGet/SetColorizationParameters` → private ordinals removed after Win8 → BROKEN on Win10/11

---

## 2. What Was Fixed This Pass

### A. Wallhaven Provider → API v1 (complete rewrite)

**Files:**
- `Pulse/Providers/wallbase/Provider.cs` (new, old saved as Provider.legacy.cs)
- `Pulse/Providers/wallbase/WallbaseImageSearchSettings.cs` (new, old as legacy)
- `Pulse/Providers/wallbase/WallbaseProviderPrefs.cs` (updated UI labels)
- `Pulse/Providers/wallbase/wallhaven.csproj` (removed CsQuery, added System.Web.Extensions + Runtime.Serialization)

**Changes:**
- Removed CsQuery dependency (ancient 1.3.4)
- Now uses `https://wallhaven.cc/api/v1/search` and `/collections/{username}/{id}`
- Parameters mapped:
  - `q` = Query
  - `categories` = WG/W/HR → 111 binary string (same)
  - `purity` = SFW/Sketchy/NSFW (requires API key for NSFW)
  - `sorting` = relevance→relevance, date→date_added, favs→favorites, views, toplist, random
  - `order` = desc/asc
  - `topRange` = new field (1d,3d,1w,1M,3M,6M,1y) for toplist
  - `atleast` vs `resolutions` based on SO (gteq=atleast, eqeq=exact)
  - `ratios` = mapped legacy float AR (1.33→4x3, 1.77→16x9 etc) + supports new 16x9 syntax
  - `colors` = hex
  - `page` + `seed` for random pagination
  - `apikey` via query param + X-API-Key header
- Download uses `WebClient` with:
  - User-Agent `Pulse/2.0 (Wallhaven API v1; +https://github.com/patricker/Pulse)`
  - Accept `application/json`
  - TLS 1.2 enforced via `ServicePointManager.SecurityProtocol |= 3072|768|Tls`
- Parsing:
  - Primary: `JavaScriptSerializer` via reflection (avoids hard ref if missing, falls back)
  - Fallback: manual regex extraction of id/path/thumbs/pageUrl/meta (works without any JSON lib)
  - Deduplication by Id
  - Banned URL filtering preserved
  - Rate limit handling: sleeps 5s on 429, 250ms between pages
  - 401 handling: if NSFW requested without key, retries without NSFW
- Collection support fixed: old bug hardcoded `Aheres` user, now uses `ApiUsername` + `FavoriteID` fallback.

**UI:**
- Login field → User (for collections)
- Pass field → API Key (now visible, not password char)
- Label updated to "API Key optional. Needed for NSFW. Get key from wallhaven.cc/settings"

**API Key:**
- Users get key from https://wallhaven.cc/settings → API Key
- Free, 45 req/min rate limit
- Required for purity 110/111 (sketchy/nsfw) when user setting disallows guests

### B. Pulse.Base HttpUtility

- Added `UserAgent` property default `Mozilla/5.0 (Pulse; ...)`
- Enforces TLS 1.2
- Sets Accept header, timeout 15s
- Ensures User-Agent header even via WebClient.Headers

### C. NASA APOD Provider (fixed + new API)

**File:** `Pulse/Providers/NASAAPOD/NASAAPODProvider.cs`

- Now tries new `api.nasa.gov/planetary/apod?api_key=DEMO_KEY&count=50&thumbs=True` first
- Manual JSON parsing for `media_type=image`, prefers `hdurl` then `url`
- Filters banned URLs
- Fallback scraping fixed:
  - https archive URL
  - Case-insensitive regex `<a href>` and `<IMG SRC>`
  - Handles relative `image/` paths
  - Skips youtube/vimeo videos
  - Uses CookieAwareWebClient with UA + TLS
- Keeps old typo class `NASAAPODProviderza` for backward compat (inherits fixed version)

### D. MediaRSS Provider

- Now downloads via CookieAwareWebClient with UA + TLS instead of raw XDocument.Load
- Null-safe thumb/url extraction (checks Attribute existence)
- Handles missing `medium` attribute
- Logs errors instead of crashing

### E. Piler Provider

- Changed from `Desktop.SetWallpaperUsingActiveDesktop` (XP/Vista only) to `SetWallpaperUsingSystemParameterInfo` with fallback
- ActiveDesktop COM removed in Win7+, so collage now works on Win10/11

---

## 3. Remaining Providers - Status & Recommendations

| Provider | Status | Recommendation |
|---|---|---|
| **Wallhaven** | **FIXED** - now hits api v1 | Test with API key for NSFW, add TopRange UI dropdown |
| **Google Images** | BROKEN | **Retire** or rewrite to use Bing Image Search API (free tier 1000/mo) or Google Custom Search JSON API ($5/1000). Scraping impossible. Suggest marking obsolete with message. |
| **NASA APOD** | FIXED (API + scraping) | Works. Could add Settings UI for API key (DEMO_KEY rate-limited). Current uses DEMO_KEY. |
| **NatGeo** | BROKEN | **Retire** or replace with new NatGeo Photo-of-the-Day scraper (needs React SSR parsing). Could also point to NASA Image of the Day or Bing Wallpaper as replacement. |
| **MediaRSS** | WORKS (fixed) | Update default URL https, keep generic. Could add more defaults: NASA, Flickr, 500px, Reddit wallpapers RSS. |
| **LocalDirectory** | WORKS | None |
| **WallpaperSetter** | WORKS | None |
| **Piler** | FIXED | Minor GDI+ could be optimized, but works |
| **LogonBackground** | BROKEN Win8+ | Retire or replace with Spotlight/lock screen API. Mark as Win7-only. |
| **AeroGlassChanger** | BROKEN Win10+ | Retire. Modern equivalent is registry `HKCU\Software\Microsoft\Windows\DWM\ColorizationColor` + broadcast WM_DWMCOLORIZATIONCOLORCHANGED. |

---

## 4. Build System - Current & Modernization

### Current
- .NET Framework 4.0, ToolsVersion 4.0, VS2010-era csproj (non-SDK)
- Uses packages.config with CsQuery only (now removed)
- References: PresentationCore/Framework, WindowsBase (WPF), System.Drawing (GDI+), WinForms
- No dotnet CLI on this box; mono not installed; builds only on Windows with .NET 4.x
- Output: `Pulse\bin\Debug\Providers\*.dll` loaded via reflection in ProviderManager

### Immediate improvements (done)
- Remove CsQuery
- Add TLS 1.2 enforcement everywhere
- Add User-Agent

### Short-term (next PRs)
1. **Bump to .NET Framework 4.8** - minimal change, still supports same APIs, enables TLS1.2 by default, better HttpClient, still runs on Win7-11. Change `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>` in all csproj.
2. **Add Newtonsoft.Json** - proper JSON parsing instead of regex fallback. Replace manual parsing in Wallhaven+Nasa with `JsonConvert`.
3. **Add Provider Settings for API keys** - Wallhaven API key, NASA API key (users can get free from api.nasa.gov), Bing API key if adding.
4. **Fix ProviderManager platform check** - current check `Environment.OSVersion.Platform/Version.Major/Minor` reports 6.2 on Win10 without manifest. Need app.manifest with supportedOS GUIDs for Win10/11, or remove platform check for most providers.
5. **Add TopRange UI** - currently not exposed in Wallhaven prefs Designer. Add combobox bound to `TopTimeSpan.GetTimespanList()`.

### Long-term modernization (fun, big)
1. **Port to .NET 8 / 9 SDK-style projects**
   - Replace old csproj with SDK: `<Project Sdk="Microsoft.NET.Sdk">`, `<TargetFramework>net8.0-windows</TargetFramework>`
   - Use `<UseWpf>true</UseWpf>` + `<UseWindowsForms>true</UseWindowsForms>`
   - This enables building on Linux with `dotnet build` (WPF still Windows-only but SDK works)
   - Replace `WebClient` with `HttpClient` + async/await
   - Replace `System.Drawing` with `ImageSharp` or `SkiaSharp` for cross-platform (GDI+ broken on Linux)
   - Replace `Picture.GetThumbnail()` that uses `WebClient.OpenRead` + `Image.FromStream` with HttpClient.

2. **Architecture**
   - Keep `Pulse.Base` as core library, but make providers `IHostedService` style
   - Add DI container
   - Add cross-platform wallpaper setter abstraction: Windows (SPI), macOS (AppleScript or `defaults`), Linux (gsettings/feh)
   - Pulse currently works on OSX Mono (tested 3.2.3) via `OSXWallpaperSetter.cs` - keep but modernize.

3. **New Providers ideas**
   - Bing Wallpaper of the Day (https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=8)
   - Reddit /r/wallpapers, /r/wallpaper, /r/EarthPorn via JSON (`https://www.reddit.com/r/wallpapers.json`)
   - Unsplash API (needs key, free)
   - Pexels, Pixabay
   - Local AI generation?

4. **Testing**
   - Add xUnit project `Pulse.Tests`
   - For each provider, integration test that calls GetPictures with max 2 and asserts >0 results (skipped if no API key)
   - Mock HttpMessageHandler for unit tests

5. **CI/CD**
   - GitHub Actions: windows-latest + dotnet build, upload artifact Providers
   - CodeQL, Dependabot

---

## 5. How to Test New Wallhaven Provider (Manual, without build)

Since this box has no dotnet/mono, test via Python equivalent that mimics C# logic:

```bash
python3 /tmp/pulse_validate.py
# or test new API specifically:
curl -A "Pulse/2.0" "https://wallhaven.cc/api/v1/search?q=nature&categories=111&purity=100&sorting=relevance&order=desc&page=1" | jq '.data[0]'
```

For C# code, logic can be unit-tested with a small console app referencing new DLL.

For full build, on Windows:
```
msbuild Pulse.sln /p:Configuration=Release
# bin\Release\Pulse.exe + Providers\wallhaven.dll
```

---

## 6. Next Steps - Checklist

- [x] Validate live endpoints
- [x] Rewrite Wallhaven provider to API v1
- [x] Update settings for API key + collection fix
- [x] Fix HttpUtility TLS+UA
- [x] Fix NASA APOD (API + scraping)
- [x] Fix MediaRSS null guards
- [x] Fix Piler ActiveDesktop
- [ ] Add TopRange ComboBox to Wallhaven prefs UI
- [ ] Bump all csproj to net48
- [ ] Add Newtonsoft.Json and refactor parsing
- [ ] Mark GoogleImages and NationalGeographic as [Obsolete] with descriptive message
- [ ] Add app.manifest with Win10/11 supportedOS
- [ ] Test on Windows 11 VM with Pulse.exe
- [ ] Write new providers: Bing, Unsplash, Reddit
- [ ] Modernize to SDK-style net8.0-windows

---

## 7. Files Changed This Pass (for PR)

```
Pulse/Providers/wallbase/Provider.cs (new API v1)
Pulse/Providers/wallbase/WallbaseImageSearchSettings.cs (new fields + API URL builder)
Pulse/Providers/wallbase/WallbaseProviderPrefs.cs (UI mapping for ApiKey)
Pulse/Providers/wallbase/wallhaven.csproj (remove CsQuery, add Web.Extensions + Runtime.Serialization)
Pulse/Base/HttpUtility.cs (TLS + UA)
Pulse/Providers/NASAAPOD/NASAAPODProvider.cs (API + fixed scraping)
Pulse/Providers/MediaRSS/Provider.cs (UA + null guards)
Pulse/Providers/Piler/PilerProvider.cs (SPI fallback)
```

Legacy backups:
- Provider.legacy.cs / WallbaseImageSearchSettings.legacy.cs kept for reference, should be removed before PR final.

---

## 8. References

- Wallhaven API v1 docs: https://wallhaven.cc/help/api
- NASA APOD API: https://api.nasa.gov/
- Old Wallbase.cc → wallhaven.cc migration story (2015)
- Pulse original README: Windows 7/8 + OSX Mono 3.2.3

Enjoy the revival!
