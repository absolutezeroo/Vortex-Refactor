// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreErrorLogger.as

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreErrorLogger.as
public interface ICoreErrorLogger
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreErrorLogger.as::logCrash
    void LogCrash(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreErrorLogger.as::logError
    void LogError(string param1);
}
