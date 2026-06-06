// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/air/FileProxy.as

using System;
using System.IO;

using Godot;

using Vortex.Core.Utils;

namespace Vortex;

/// <summary>
/// Godot/.NET equivalent of AIR FileProxy for core cache persistence.
/// </summary>
/// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/air/FileProxy.as
public sealed class FileProxy : IFileProxy
{
    private const string LOCAL_FILE_PATH = "local_include/";
    private const string CACHE_PATH = "com.sulake.habbo/";

    /// @see FileProxy.as::clearCache
    public void ClearCache()
    {
        DeleteCacheDirectory(string.Empty);
    }

    /// @see FileProxy.as::localFilePath
    public string LocalFilePath(string path)
    {
        return ProjectSettings.GlobalizePath($"res://{LOCAL_FILE_PATH}{NormalizeGodotPath(path)}");
    }

    /// @see FileProxy.as::cacheFilePath
    public string CacheFilePath(string path)
    {
        return ResolveCachePath(path);
    }

    /// @see FileProxy.as::loadLocalBitmapData
    public void LoadLocalBitmapData(string path, Action<object?> callback)
    {
        string localPath = LocalFilePath(path);

        if (!File.Exists(localPath))
        {
            callback(null);
            return;
        }

        Image image = new();
        Error result = image.Load(localPath);

        callback(result == Error.Ok ? image : null);
    }

    /// @see FileProxy.as::cacheFileExists
    public bool CacheFileExists(string path)
    {
        return File.Exists(ResolveCachePath(path));
    }

    /// @see FileProxy.as::localFileExists
    public bool LocalFileExists(string path)
    {
        return File.Exists(LocalFilePath(path));
    }

    /// @see FileProxy.as::readCache
    public byte[]? ReadCache(string path)
    {
        string cachePath = ResolveCachePath(path);

        return File.Exists(cachePath) ? File.ReadAllBytes(cachePath) : null;
    }

    /// @see FileProxy.as::readCacheAsync
    public void ReadCacheAsync(string path, Action<byte[]?> callback)
    {
        callback(ReadCache(path));
    }

    /// @see FileProxy.as::writeCache
    public void WriteCache(string path, byte[] payload)
    {
        string cachePath = ResolveCachePath(path);
        string? directory = Path.GetDirectoryName(cachePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(cachePath, payload);
    }

    /// @see FileProxy.as::writeCacheAsync
    public void WriteCacheAsync(string path, byte[] payload)
    {
        WriteCache(path, payload);
    }

    /// @see FileProxy.as::deleteCacheDirectory
    public void DeleteCacheDirectory(string path)
    {
        string cachePath = ResolveCachePath(path);

        if (Directory.Exists(cachePath))
        {
            Directory.Delete(cachePath, true);
        }
    }

    /// @see FileProxy.as::swapObjectToDisk
    public int SwapObjectToDisk(object value)
    {
        _ = value;

        return -1;
    }

    /// @see FileProxy.as::swapObjectFromDisk
    public object? SwapObjectFromDisk(int id)
    {
        _ = id;

        return null;
    }

    private static string ResolveCachePath(string path)
    {
        string rootPath = ProjectSettings.GlobalizePath($"user://{CACHE_PATH}");
        string fullRoot = EnsureTrailingSeparator(Path.GetFullPath(rootPath));
        string fullPath = Path.GetFullPath(Path.Combine(fullRoot, NormalizeFileSystemPath(path)));

        if (!fullPath.StartsWith(fullRoot, PathComparison))
        {
            throw new InvalidOperationException($"Invalid cache path: {path}");
        }

        return fullPath;
    }

    private static string NormalizeFileSystemPath(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar)
                   .Replace('/', Path.DirectorySeparatorChar)
                   .TrimStart(Path.DirectorySeparatorChar);
    }

    private static string NormalizeGodotPath(string path)
    {
        return path.Replace('\\', '/')
                   .TrimStart('/');
    }

    private static string EnsureTrailingSeparator(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
    }

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
