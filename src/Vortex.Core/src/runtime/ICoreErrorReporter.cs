// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreErrorReporter.as

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreErrorReporter.as
public interface ICoreErrorReporter
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreErrorReporter.as::logError
    void LogError(string param1, bool param2, int param3 = -1, System.Exception? param4 = null);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreErrorReporter.as::set errorLogger
    ICoreErrorLogger? errorLogger { set; }
}
