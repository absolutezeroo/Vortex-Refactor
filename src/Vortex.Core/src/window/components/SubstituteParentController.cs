// @see WIN63-202407091256-704579380-Source-main/core/window/components/SubstituteParentController.as

using System.Linq;

using Godot;

namespace Vortex.Core.Window.Components;

/// @see WIN63-202407091256-704579380-Source-main/core/window/components/SubstituteParentController.as
public class SubstituteParentController : WindowController
{
    /// @see SubstituteParentController.as::SubstituteParentController
    public SubstituteParentController(IWindowContext param1)
        : base("_CONTEXT_SUBSTITUTE_PARENT", 0, 0, 16, param1, new Rect2(0, 0, 2000, 2000), null, null, null, null, 0)
    {
    }

    /// @see SubstituteParentController.as::addChild
    public override bool AddChild(IWindow param1)
    {
        _children.Add(param1);
        return true;
    }

    /// @see SubstituteParentController.as::addChildAt
    public override IWindow? AddChildAt(IWindow param1, int param2)
    {
        param1.parent?.RemoveChild(param1);

        param2 = System.Math.Clamp(param2, 0, _children.Count);
        _children.Insert(param2, param1);
        SetChildParentInternal(param1, this);

        return param1;
    }

    /// @see SubstituteParentController.as::removeChild
    public override bool RemoveChild(IWindow param1)
    {
        int idx = _children.IndexOf(param1);

        if (idx < 0)
        {
            return false;
        }

        _children.RemoveAt(idx);
        SetChildParentInternal(param1, null);

        return true;
    }

    /// @see SubstituteParentController.as::getChildByName
    public IWindow? GetChildByNameOverride(string param1)
    {
        return _children.FirstOrDefault(child => string.Equals(child.name, param1, System.StringComparison.Ordinal));
    }

    /// @see SubstituteParentController.as::findChildByName
    public IWindow? FindChildByNameOverride(string param1)
    {
        foreach (IWindow child in _children.Where(child => string.Equals(child.name, param1, System.StringComparison.Ordinal)))
        {
            return child;
        }

        return _children.Select(child => child.FindChildByName(param1)).OfType<IWindow>().FirstOrDefault();
    }

    private static void SetChildParentInternal(IWindow child, IWindow? newParent)
    {
        if (child is WindowModel model)
        {
            model.SetParentInternal(newParent);
        }
    }
}
