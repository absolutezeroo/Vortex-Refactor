// @see WIN63-202111081545-75921380-Source-main/src/login/ImageLoader.as

using System;

using Godot;

using NetHttpClient = System.Net.Http.HttpClient;

namespace Vortex.Login;

/// @see WIN63-202111081545-75921380-Source-main/src/login/ImageLoader.as
public sealed class ImageLoader
{
    private static readonly NetHttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10),
    };

    private readonly TextureRect _loader;
    private readonly string _url;

    public ImageLoader(TextureRect loader, string url)
    {
        _loader = loader;
        _url = url;

        Load();
    }

    public event EventHandler<ImageLoaderEvent>? Complete;

    public static ImageLoader CreateLoader(TextureRect loader, string url, Action<ImageLoaderEvent> callback)
    {
        ImageLoader imageLoader = new(loader, url);
        imageLoader.Complete += (_, args) => callback(args);

        return imageLoader;
    }

    private void Load()
    {
        try
        {
            Texture2D? texture = TryLoadTexture(_url);

            if (texture == null)
            {
                Logger.Warn($"[ImageLoader] Failed to load image {_url}");
                return;
            }

            _loader.Texture = texture;

            Logger.Debug($"[ImageLoader] Loaded image {_url}");

            Complete?.Invoke(this, new ImageLoaderEvent(_loader, _url));
        }
        catch (Exception exception)
        {
            Logger.Error("[ImageLoader] Failed to load image", exception);
        }
    }

    private static Texture2D? TryLoadTexture(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (url.StartsWith("res://", StringComparison.Ordinal) && ResourceLoader.Exists(url))
        {
            return GD.Load<Texture2D>(url);
        }

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        byte[] bytes = HttpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();

        if (bytes.Length == 0)
        {
            return null;
        }

        Image image = new();

        Error error = image.LoadPngFromBuffer(bytes);

        if (error != Error.Ok)
        {
            error = image.LoadJpgFromBuffer(bytes);
        }

        return error != Error.Ok ? null : ImageTexture.CreateFromImage(image);
    }
}
