// @see WIN63-202407091256-704579380-Source-main/core/runtime/class_516.as

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/class_516.as
public sealed class class_516 : ICoreErrorReporter
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/class_516.as::class_516
    public class_516() { }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/class_516.as::logError
    public void LogError(string param1, bool param2, int param3 = -1, System.Exception? param4 = null)
    {
        _ = param2;
        _ = param3;

        Logger.Error(param1, param4);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/class_516.as::set errorLogger
    public ICoreErrorLogger? errorLogger
    {
        set => _ = value;
    }
}
