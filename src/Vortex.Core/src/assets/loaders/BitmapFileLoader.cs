// @see core/assets/loaders/BitmapFileLoader.as

using System;
using System.IO;

using Godot;

using NetHttpClient = System.Net.Http.HttpClient;

namespace Vortex.Core.Assets.Loaders;

/// @see core/assets/loaders/BitmapFileLoader.as
/// Godot adaptation: uses Image.Load/LoadPngFromBuffer instead of Flash Loader.
/// Content returns Godot Image instead of Flash Bitmap.
public class BitmapFileLoader : AssetLoaderBase, IAssetLoader
{
    private static readonly NetHttpClient SharedHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30),
    };

    protected string? _url;
    protected string? _mimeType;
    protected Image? _image;
    protected byte[]? _rawBytes;

    /// @see BitmapFileLoader.as::BitmapFileLoader
    public BitmapFileLoader(string mimeType, string? url = null, int id = -1)
    {
        _mimeType = mimeType;
        _url = url ?? "";
        Id = id;

        if (!string.IsNullOrEmpty(url))
        {
            Load(url);
        }
    }

    /// @see BitmapFileLoader.as::get url
    public string? Url => _url;

    /// @see BitmapFileLoader.as::get content
    public object? Content => _image;

    /// @see BitmapFileLoader.as::get bytes
    /// Godot adaptation: encode loaded image to PNG bytes.
    public byte[]? Bytes
    {
        get
        {
            if (_image == null)
            {
                return null;
            }

            return _image.SavePngToBuffer();
        }
    }

    /// @see BitmapFileLoader.as::get mimeType
    public string? MimeType => _mimeType;

    /// @see BitmapFileLoader.as::get bytesLoaded
    public uint BytesLoaded => (uint)(_rawBytes?.Length ?? 0);

    /// @see BitmapFileLoader.as::get bytesTotal
    public uint BytesTotal => (uint)(_rawBytes?.Length ?? 0);

    /// @see BitmapFileLoader.as::get id
    public int Id { get; }

    /// @see BitmapFileLoader.as::load
    /// Godot adaptation: loads image from Godot resource path, filesystem, or HTTP URL.
    public void Load(string url)
    {
        _url = url;
        _retryCount = 0;
        LoadUrl(url);
    }

    private void LoadUrl(string url)
    {
        _image = null;
        _rawBytes = null;

        try
        {
            Image image = new();
            Error error;

            if (url.StartsWith("res://", StringComparison.Ordinal) || url.StartsWith("user://", StringComparison.Ordinal))
            {
                error = image.Load(url);
            }
            else if (IsHttpUrl(url))
            {
                _rawBytes = SharedHttpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
                error = TryLoadFromBytes(image, _rawBytes, url);
            }
            else if (File.Exists(url))
            {
                _rawBytes = File.ReadAllBytes(url);
                error = TryLoadFromBytes(image, _rawBytes, url);
            }
            else
            {
                DispatchLoaderEvent("ioError");
                return;
            }

            if (error == Error.Ok)
            {
                _image = image;
                DispatchLoaderEvent("complete");
            }
            else
            {
                DispatchLoaderEvent("ioError");
            }
        }
        catch (Exception)
        {
            DispatchLoaderEvent("ioError");
        }
    }

    /// @see BitmapFileLoader.as::retry
    protected override bool Retry()
    {
        if (_disposed || ++_retryCount > _maxRetries)
        {
            return false;
        }

        if (_url == null)
        {
            return true;
        }

        try
        {
            string retryUrl = _url + (_url.Contains("?", StringComparison.Ordinal) ? "&" : "?") + "retry=" + _retryCount;
            LoadUrl(retryUrl);
        }
        catch
        {
            // Retry failed
        }
        return true;
    }

    /// @see BitmapFileLoader.as::dispose
    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        base.Dispose();

        _image = null;
        _rawBytes = null;
        _mimeType = null;
        _url = null;
    }

    private static Error TryLoadFromBytes(Image image, byte[] bytes, string url)
    {
        string ext = Path.GetExtension(url).ToLowerInvariant();

        return ext switch
        {
            ".png" => image.LoadPngFromBuffer(bytes),
            ".jpg" or ".jpeg" => image.LoadJpgFromBuffer(bytes),
            ".bmp" => image.LoadBmpFromBuffer(bytes),
            ".webp" => image.LoadWebpFromBuffer(bytes),
            ".tga" => image.LoadTgaFromBuffer(bytes),
            _ => image.LoadPngFromBuffer(bytes) == Error.Ok
                ? Error.Ok
                : image.LoadJpgFromBuffer(bytes),
        };
    }

    private static bool IsHttpUrl(string url)
    {
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }
}
