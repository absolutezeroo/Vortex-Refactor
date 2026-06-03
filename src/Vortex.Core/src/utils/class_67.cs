// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as

using System;

namespace Vortex.Core.Utils;

/// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as
public interface IClass67
{
    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::clearCache
    void ClearCache();

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::localFilePath
    string LocalFilePath(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::cacheFilePath
    string CacheFilePath(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::loadLocalBitmapData
    void LoadLocalBitmapData(string param1, Action<object?> param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::cacheFileExists
    bool CacheFileExists(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::localFileExists
    bool LocalFileExists(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::readCache
    byte[]? ReadCache(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::readCacheAsync
    void ReadCacheAsync(string param1, Action<byte[]?> param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::writeCache
    void WriteCache(string param1, byte[] param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::writeCacheAsync
    void WriteCacheAsync(string param1, byte[] param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::deleteCacheDirectory
    void DeleteCacheDirectory(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::swapObjectToDisk
    int SwapObjectToDisk(object param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/class_67.as::swapObjectFromDisk
    object? SwapObjectFromDisk(int param1);
}
