// Godot adaptation: loads .vortex bundle files via the standard IAssetLoader pattern.

using System;
using System.IO;
using System.Net.Http;

using Vortex.Bundle.Data;
using Vortex.Bundle.IO;

using FileAccess = Godot.FileAccess;
using NetHttpClient = System.Net.Http.HttpClient;

namespace Vortex.Core.Assets.Loaders;

/// <summary>
/// Asset loader for .vortex bundle files.
/// Follows the same pattern as BinaryFileLoader/BitmapFileLoader.
/// On completion, Content holds the parsed VortexBundleData.
/// </summary>
public class VortexBundleFileLoader : AssetLoaderBase, IAssetLoader
{
    private static readonly NetHttpClient SharedHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30),
    };

    private VortexBundleData? _bundleData;

    public VortexBundleFileLoader(string mimeType, string? url = null, int id = -1)
    {
        MimeType = mimeType;
        Url = url ?? "";
        Id = id;

        if (!string.IsNullOrEmpty(url))
        {
            Load(url);
        }
    }

    public string? Url { get; private set; }

    /// <summary>
    /// Returns the parsed VortexBundleData on successful load.
    /// </summary>
    public object? Content => _bundleData;

    public byte[]? Bytes { get; private set; }

    public string? MimeType { get; private set; }

    public uint BytesLoaded => (uint)(Bytes?.Length ?? 0);

    public uint BytesTotal => (uint)(Bytes?.Length ?? 0);

    public int Id { get; }

    public void Load(string url)
    {
        Url = url;
        Bytes = null;
        _bundleData = null;
        _retryCount = 0;

        try
        {
            byte[]? raw = null;

            if (url.StartsWith("res://", StringComparison.Ordinal) ||
                url.StartsWith("user://", StringComparison.Ordinal))
            {
                raw = FileAccess.GetFileAsBytes(url);
            }
            else if (IsHttpUrl(url))
            {
                raw = DownloadBytes(url);
            }
            else if (File.Exists(url))
            {
                raw = File.ReadAllBytes(url);
            }

            if (raw == null || raw.Length == 0)
            {
                DispatchLoaderEvent("ioError");
                return;
            }

            Bytes = raw;

            // Log first 4 bytes (magic) to detect format mismatches (e.g. SWF instead of VRTX)
            if (raw.Length >= 4)
            {
                string magic = System.Text.Encoding.ASCII.GetString(raw, 0, 4);
                Logger.Info(
                    $"[VortexBundleFileLoader] File magic: '{magic}' (0x{raw[0]:X2} 0x{raw[1]:X2} 0x{raw[2]:X2} 0x{raw[3]:X2}), size={raw.Length}b, url={url}");
            }

            using MemoryStream ms = new(raw);
            VortexBundleReader reader = new();

            _bundleData = reader.Read(ms, true);

            DispatchLoaderEvent("complete");
        }
        catch (Exception e)
        {
            Logger.Warn($"[VortexBundleFileLoader] Parse failed for {url}: {e.Message}");
            DispatchLoaderEvent("ioError");
        }
    }

    /// Downloads bytes from an HTTP/HTTPS URL.
    private static byte[]? DownloadBytes(string url)
    {
        try
        {
            Logger.Info($"[VortexBundleFileLoader] Downloading: {url}");

            using HttpResponseMessage response = SharedHttpClient.GetAsync(url).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                Logger.Warn($"[VortexBundleFileLoader] HTTP {(int)response.StatusCode} for {url}");
                return null;
            }

            byte[] data = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            Logger.Info($"[VortexBundleFileLoader] Downloaded {data.Length} bytes from {url}");
            return data;
        }
        catch (Exception e)
        {
            Logger.Warn($"[VortexBundleFileLoader] HTTP download failed for {url}: {e.Message}");
            return null;
        }
    }

    /// Checks if a URL is an HTTP or HTTPS URL.
    private static bool IsHttpUrl(string url)
    {
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    protected override bool Retry()
    {
        if (_disposed || ++_retryCount > _maxRetries)
        {
            return false;
        }

        if (Url == null)
        {
            return true;
        }

        try
        {
            Load(Url);

            return true;
        }
        catch
        {
            // fall through
        }

        return true;
    }

    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        base.Dispose();
        Bytes = null;
        _bundleData = null;
        MimeType = null;
        Url = null;
    }
}
