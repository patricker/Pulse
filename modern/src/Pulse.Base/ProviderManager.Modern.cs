using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Pulse.Base.Providers;

namespace Pulse.Base
{
    /// <summary>
    /// Modern ProviderManager using AssemblyLoadContext for .NET 8 plugin isolation.
    /// Old version used Assembly.LoadFrom which locks file and loads into default ALC.
    /// New version uses collectible ALC per plugin DLL, supports unload.
    /// </summary>
    public class ProviderLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public ProviderLoadContext(string pluginPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // Resolve dependencies from plugin's folder first, then default
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return null; // fallback to default ALC
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }
            return IntPtr.Zero;
        }
    }

    public partial class ProviderManager
    {
        private static readonly List<ProviderLoadContext> _loadedContexts = new();

        // Modern FindProviders using ALC
        private Dictionary<string, Type> FindProvidersModern()
        {
            var result = new Dictionary<string, Type>();

            // FIX: AppContext.BaseDirectory points to temp extraction dir in single-file publish
            // Use Environment.ProcessPath for exe dir, fallback to AppContext
            string exeDir = Path.GetDirectoryName(Environment.ProcessPath ?? AppContext.BaseDirectory) ?? AppContext.BaseDirectory;
            var providersDirectory = Path.Combine(exeDir, "Providers");

            if (!Directory.Exists(providersDirectory))
            {
                var alt = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pulse", "Providers");
                if (Directory.Exists(alt)) providersDirectory = alt;
                else return result;
            }

            var files = Directory.GetFiles(providersDirectory).Where(x => x.EndsWith(".dll") || x.EndsWith(".exe"));

            foreach (var f in files)
            {
                try
                {
                    var alc = new ProviderLoadContext(f);
                    // Track ALC to keep alive (collectible illusion fix)
                    _loadedContexts.Add(alc);
                    var assembly = alc.LoadFromAssemblyPath(f);

                    var providerTypes = assembly.GetTypes().Where(type => typeof(IProvider).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface);

                    bool anyAdded = false;
                    foreach (Type ipType in providerTypes)
                    {
                        // Hide retired
                        if (ipType.IsDefined(typeof(ObsoleteAttribute), false))
                        {
                            Log.Logger.Write($"Provider '{ipType.FullName}' skipped - retired/Obsolete", Log.LoggerLevels.Info);
                            continue;
                        }

                        var strName = GetProviderName(ipType);
                        var attrPlatform = GetSupportedPlatformsForType(ipType);

                        if (attrPlatform.Any())
                        {
                            var currentOS = Environment.OSVersion;
                            var ppa = from ProviderPlatformAttribute ppaI in attrPlatform
                                      where ppaI.Platform == currentOS.Platform
                                            && (ppaI.MajorVersion == 0 ||
                                                currentOS.Version.Major > ppaI.MajorVersion ||
                                                (currentOS.Version.Major == ppaI.MajorVersion &&
                                                 (ppaI.MinorVersion == 0 || currentOS.Version.Minor >= ppaI.MinorVersion)))
                                      select ppaI;

                            if (!ppa.Any())
                            {
                                continue;
                            }
                        }

                        string uniqueName = strName;
                        int suffix = 2;
                        while (result.ContainsKey(uniqueName))
                        {
                            if (result[uniqueName] == ipType) break;
                            uniqueName = $"{strName} ({suffix})";
                            suffix++;
                            if (suffix > 10) break;
                        }

                        if (!result.ContainsKey(uniqueName))
                        {
                            result.Add(uniqueName, ipType);
                            anyAdded = true;
                            if (uniqueName != strName)
                                Log.Logger.Write($"Provider duplicate renamed '{strName}' -> '{uniqueName}' from {f}", Log.LoggerLevels.Info);
                        }
                    }

                    // If no providers added from this dll, unload ALC
                    if (!anyAdded)
                    {
                        _loadedContexts.Remove(alc);
                        alc.Unload();
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Write($"Failed to load provider {f}: {ex.Message}", Log.LoggerLevels.Errors);
                }
            }

            return result;
        }

        public static void UnloadAllProviders()
        {
            foreach (var alc in _loadedContexts)
            {
                try { alc.Unload(); } catch { }
            }
            _loadedContexts.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static readonly System.Net.Http.HttpClient _updateHttpClient = new System.Net.Http.HttpClient(new System.Net.Http.SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            AutomaticDecompression = System.Net.DecompressionMethods.All
        });

        static ProviderManager()
        {
            // Static ctor for modern to init HttpClient UA once
            try
            {
                _updateHttpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Pulse", "2.0"));
                _updateHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            }
            catch { }
        }

        public static async Task<string?> CheckForUpdateModernAsync()
        {
            try
            {
                string json = await _updateHttpClient.GetStringAsync("https://api.github.com/repos/patricker/Pulse/releases/latest").ConfigureAwait(false);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                string? tag = doc.RootElement.TryGetProperty("tag_name", out var tp) ? tp.GetString() : null;
                string? name = doc.RootElement.TryGetProperty("name", out var np) ? np.GetString() : null;
                string? htmlUrl = doc.RootElement.TryGetProperty("html_url", out var hp) ? hp.GetString() : null;

                Log.Logger.Write($"Latest release: {tag} - {name} - {htmlUrl}", Log.LoggerLevels.Info);
                return tag;
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"Update check failed: {ex.Message}", Log.LoggerLevels.Warnings);
                return null;
            }
        }
    }
}
