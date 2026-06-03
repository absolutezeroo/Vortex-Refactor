// @see core/window/utils/IChildEntityArray.as

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/IChildEntityArray.as
public interface IChildEntityArray
{
    IChildEntity AddChild(IChildEntity child);

    IChildEntity AddChildAt(IChildEntity child, int index);

    IChildEntity? RemoveChild(IChildEntity child);

    IChildEntity? RemoveChildAt(int index);

    void SetChildIndex(IChildEntity child, int index);

    void SwapChildren(IChildEntity child1, IChildEntity child2);

    void SwapChildrenAt(int index1, int index2);
}
