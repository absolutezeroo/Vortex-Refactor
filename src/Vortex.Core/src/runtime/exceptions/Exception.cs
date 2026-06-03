// @see WIN63-202407091256-704579380-Source-main/core/runtime/exceptions/Exception.as

using System.Text;

namespace Vortex.Core.Runtime.Exceptions;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/exceptions/Exception.as
public class Exception : System.Exception
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/exceptions/Exception.as::Exception
    protected Exception(string param1, int param2 = 0, System.Exception? param3 = null)
        : base(param1)
    {
        HResult = param2;
        cause = param3;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/exceptions/Exception.as::get cause
    public System.Exception? cause { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/exceptions/Exception.as::getChainedStackTrace
    public static string? GetChainedStackTrace(System.Exception? param1)
    {
        StringBuilder builder = new();

        while (param1 != null)
        {
            if (!string.IsNullOrEmpty(param1.StackTrace))
            {
                if (builder.Length > 0)
                {
                    builder.Append('\n').Append("caused by ");
                }

                builder.Append(param1.StackTrace);
            }

            param1 = param1 is Exception runtimeException ? runtimeException.cause : null;
        }

        return builder.Length > 0 ? builder.ToString() : null;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/exceptions/Exception.as::toString
    public override string ToString()
    {
        string value = GetType().FullName + ": " + Message;

        if (cause != null)
        {
            value += ", caused by " + cause;
        }

        return value;
    }
}
