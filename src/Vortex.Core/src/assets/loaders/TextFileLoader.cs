// @see core/assets/loaders/TextFileLoader.as

using System.IO;
using System.IO.Compression;
using System.Text;

namespace Vortex.Core.Assets.Loaders;

/// @see core/assets/loaders/TextFileLoader.as
/// Extends BinaryFileLoader. Attempts GZip/Deflate decompression, falls back to raw UTF-8.
public class TextFileLoader : BinaryFileLoader
{
    private string? _textContent;
    private bool _decompressed;

    /// @see TextFileLoader.as::TextFileLoader
    public TextFileLoader(string mimeType, string? url = null, int id = -1)
        : base(mimeType, url, id)
    {
    }

    /// @see TextFileLoader.as — content is the decompressed text string.
    /// Godot adaptation: lazy decompress on first access. In AS3, decompression happened
    /// in a "complete" event handler (async). Here loads are synchronous, so we decompress
    /// lazily when Content is first accessed, ensuring bytes are available.
    public override object? Content
    {
        get
        {
            if (!_decompressed && Bytes is { Length: > 0 })
            {
                Decompress();
            }

            return _textContent ?? base.Content;
        }
    }

    /// @see TextFileLoader.as::load
    public override void Load(string url)
    {
        _textContent = null;
        _decompressed = false;
        base.Load(url);
    }

    /// @see TextFileLoader.as::unCompress
    /// Attempts GZip decompression first, then Deflate, falls back to raw UTF-8.
    private void Decompress()
    {
        _decompressed = true;
        byte[]? bytes = Bytes;

        if (bytes == null || bytes.Length == 0)
        {
            _textContent = "";
            return;
        }

        try
        {
            using MemoryStream inputStream = new(bytes);
            using GZipStream gzipStream = new(inputStream, CompressionMode.Decompress);
            using StreamReader reader = new(gzipStream, Encoding.UTF8);
            _textContent = reader.ReadToEnd();
        }
        catch
        {
            try
            {
                using MemoryStream inputStream = new(bytes);
                using DeflateStream deflateStream = new(inputStream, CompressionMode.Decompress);
                using StreamReader reader = new(deflateStream, Encoding.UTF8);
                _textContent = reader.ReadToEnd();
            }
            catch
            {
                _textContent = Encoding.UTF8.GetString(bytes);
            }
        }
    }

    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _textContent = null;
        _decompressed = false;
        base.Dispose();
    }
}
