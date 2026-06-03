// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStruct.as

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStruct.as
internal sealed class InterfaceStruct : IDisposable
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStruct.as::InterfaceStruct
    public InterfaceStruct(IID param1, IUnknown param2)
    {
        iid = param1;
        iis = param1.GetType().FullName ?? param1.GetType().Name;
        unknown = param2;
        references = 0;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStruct.as::get iid
    public IID? iid { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStruct.as::get iis
    public string? iis { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStruct.as::get unknown
    public IUnknown? unknown { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStruct.as::get references
    public uint references { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStruct.as::get disposed
    public bool disposed => unknown == null;

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStruct.as::dispose
    public void Dispose()
    {
        iid = null;
        iis = null;
        unknown = null;
        references = 0;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStruct.as::reserve
    public uint Reserve()
    {
        references++;

        return references;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStruct.as::release
    public uint Release()
    {
        if (references > 0)
        {
            references--;
        }

        return references;
    }
}
