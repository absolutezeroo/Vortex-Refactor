// @see core/assets/loaders/BinaryFileLoader.as

using System;
using System.IO;
using System.Net.Http;

using FileAccess = Godot.FileAccess;
using NetHttpClient = System.Net.Http.HttpClient;

namespace Vortex.Core.Assets.Loaders;

/// @see core/assets/loaders/BinaryFileLoader.as
/// Godot adaptation: uses FileAccess for local files, HttpClient for remote URLs.
/// Flash URLLoader with binary dataFormat replaced with direct file I/O.
public class BinaryFileLoader : AssetLoaderBase, IAssetLoader
{
    private static readonly NetHttpClient SharedHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30),
    };

    protected string? _url;
    protected string? _mimeType;
    protected byte[]? _data;

    /// @see BinaryFileLoader.as::BinaryFileLoader
    public BinaryFileLoader(string mimeType, string? url = null, int id = -1)
    {
        _mimeType = mimeType;
        _url = url ?? "";
        Id = id;

        if (!string.IsNullOrEmpty(url))
        {
            Load(url);
        }
    }

    /// @see BinaryFileLoader.as::get url
    public string? Url => _url;

    /// @see BinaryFileLoader.as::get content
    public virtual object? Content => _data;

    /// @see BinaryFileLoader.as::get bytes
    public byte[]? Bytes => _data;

    /// @see BinaryFileLoader.as::get mimeType
    public string? MimeType => _mimeType;

    /// @see BinaryFileLoader.as::get bytesLoaded
    public uint BytesLoaded => (uint)(_data?.Length ?? 0);

    /// @see BinaryFileLoader.as::get bytesTotal
    public uint BytesTotal => (uint)(_data?.Length ?? 0);

    /// @see BinaryFileLoader.as::get id
    public int Id { get; }

    /// @see BinaryFileLoader.as::load
    /// Godot adaptation: loads from Godot resource path, filesystem path, or HTTP URL.
    public virtual void Load(string url)
    {
        _url = url;
        _data = null;
        _retryCount = 0;

        try
        {
            if (url.StartsWith("res://", StringComparison.Ordinal) || url.StartsWith("user://", StringComparison.Ordinal))
            {
                byte[]? bytes = FileAccess.GetFileAsBytes(url);

                if (bytes is { Length: > 0 })
                {
                    _data = bytes;

                    DispatchLoaderEvent("complete");

                    return;
                }
            }

            if (IsHttpUrl(url))
            {
                LoadFromHttp(url);
                return;
            }

            if (File.Exists(url))
            {
                _data = File.ReadAllBytes(url);

                DispatchLoaderEvent("complete");

                return;
            }

            DispatchLoaderEvent("ioError");
        }
        catch (Exception)
        {
            DispatchLoaderEvent("ioError");
        }
    }

    /// Downloads content from an HTTP/HTTPS URL.
    protected void LoadFromHttp(string url)
    {
        try
        {
            Logger.Info($"[BinaryFileLoader] Downloading: {url}");

            using HttpResponseMessage response = SharedHttpClient.GetAsync(url).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                Logger.Warn($"[BinaryFileLoader] HTTP {(int)response.StatusCode} for {url}");
                DispatchLoaderEvent("ioError");
                return;
            }

            _data = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

            if (_data is { Length: > 0 })
            {
                Logger.Info($"[BinaryFileLoader] Downloaded {_data.Length} bytes from {url}");
                DispatchLoaderEvent("complete");
            }
            else
            {
                DispatchLoaderEvent("ioError");
            }
        }
        catch (Exception e)
        {
            Logger.Warn($"[BinaryFileLoader] HTTP download failed for {url}: {e.Message}");
            DispatchLoaderEvent("ioError");
        }
    }

    /// Checks if a URL is an HTTP or HTTPS URL.
    protected static bool IsHttpUrl(string url)
    {
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    /// @see BinaryFileLoader.as::retry
    protected override bool Retry()
    {
        if (_disposed || ++_retryCount > _maxRetries)
        {
            return false;
        }

        try
        {
            if (_url != null)
            {
                string retryUrl = _url + (_url.Contains('?') ? "&" : "?") + "retry=" + _retryCount;
                _data = null;

                if (File.Exists(_url))
                {
                    _data = File.ReadAllBytes(_url);

                    DispatchLoaderEvent("complete");

                    return true;
                }
            }
        }
        catch
        {
            // Retry failed, fall through
        }
        return true;
    }

    /// @see BinaryFileLoader.as::dispose
    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        base.Dispose();

        _data = null;
        _mimeType = null;
        _url = null;
    }
}
