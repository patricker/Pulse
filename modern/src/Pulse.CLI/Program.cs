using System;
using System.Threading.Tasks;
using Pulse.Base;
using wallbase;
using BingWallpaper;

Console.WriteLine("Pulse.CLI - Cross-platform provider test (buildable from Linux)");
Console.WriteLine($"OS: {Environment.OSVersion} Platform: {Environment.OSVersion.Platform} Framework: {Environment.Version}");
Console.WriteLine($"BaseDirectory: {AppContext.BaseDirectory}");
Console.WriteLine();

// Test wallhaven API (no key, SFW)
try
{
    Console.WriteLine("=== Testing Wallhaven API v1 (SFW, no key) ===");
    var settings = new WallbaseImageSearchSettings
    {
        Query = "nature",
        WG = true,
        W = true,
        HR = false,
        SFW = true,
        SKETCHY = false,
        NSFW = false,
        OB = "relevance",
        OBD = "desc",
        ImageWidth = 1920,
        ImageHeight = 1080,
        SO = "gteq"
    };

    var provider = new wallbase.Provider();
    provider.Initialize(null);

    var search = new PictureSearch
    {
        SearchProvider = new ActiveProviderInfo { ProviderConfig = settings.Save(), ProviderInstanceID = Guid.NewGuid() },
        MaxPictureCount = 5,
        PageToRetrieve = 1,
        BannedURLs = new System.Collections.Generic.List<string>(),
        SaveFolder = Path.Combine(Path.GetTempPath(), "PulseTest"),
        PreviewOnly = true
    };

    Directory.CreateDirectory(search.SaveFolder);

    var list = provider.GetPictures(search);
    Console.WriteLine($"Got {list.Pictures.Count} pictures from Wallhaven");
    foreach (var pic in list.Pictures.Take(3))
    {
        Console.WriteLine($"  - {pic.Id}: {pic.Url} thumb={pic.Properties.ContainsKey(Picture.StandardProperties.Thumbnail) ? pic.Properties[Picture.StandardProperties.Thumbnail].Substring(0, Math.Min(60, pic.Properties[Picture.StandardProperties.Thumbnail].Length)) : "none"}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Wallhaven test failed: {ex}");
}

Console.WriteLine();

// Test Bing (no key)
try
{
    Console.WriteLine("=== Testing Bing Wallpaper (no key) ===");
    var provider = new BingWallpaper.Provider();
    provider.Initialize(null);

    var search = new PictureSearch
    {
        MaxPictureCount = 3,
        BannedURLs = new System.Collections.Generic.List<string>(),
        SaveFolder = Path.Combine(Path.GetTempPath(), "PulseTest"),
        PreviewOnly = true,
        SearchProvider = new ActiveProviderInfo { ProviderConfig = "", ProviderInstanceID = Guid.NewGuid() }
    };

    Directory.CreateDirectory(search.SaveFolder);

    var list = provider.GetPictures(search);
    Console.WriteLine($"Got {list.Pictures.Count} pictures from Bing");
    foreach (var pic in list.Pictures)
    {
        Console.WriteLine($"  - {pic.Id}: {pic.Url}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Bing test failed: {ex}");
}

Console.WriteLine();
Console.WriteLine("Done. This CLI builds and runs on Linux, macOS, Windows (net8.0).");
Console.WriteLine("For Windows wallpaper setting, need net8.0-windows target with IDesktopWallpaper (not available on Linux).");
