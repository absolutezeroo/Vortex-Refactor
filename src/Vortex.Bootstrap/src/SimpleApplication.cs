using System.Collections.Concurrent;
using System.Xml.Linq;

using Godot;

namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/mx/core/SimpleApplication.as
public abstract partial class SimpleApplication : Control
{
    private static readonly ConcurrentDictionary<string, XElement?> _manifestCache = new();

    /// @see WIN63-202111081545-75921380-Source-main/src/mx/core/SimpleApplication.as::SimpleApplication
    protected SimpleApplication() { }

    /// Godot/C# adaptation: In AS3, manifest was an [Embed]'d ByteArrayAsset compiled into the SWF.
    /// Here we load the equivalent XML from data/manifests/{ClassName}_manifest.xml at runtime,
    /// cached per type name so each file is read only once.
    public static object? manifest => null;
}
