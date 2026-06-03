// @see core/window/utils/ChildEntityArray.as
// @see core/window/utils/class_3835.as (base class)
// @see core/window/utils/class_3488.as (IChildEntity)
// @see core/window/utils/class_3412.as (IChildEntityCollection)

using System.Linq;

namespace Vortex.Core.Window.Utils;

/// <summary>
/// Typed array wrapper for child entity management.
/// Combines AS3 class_3835 (base) + ChildEntityArray (mutations).
/// </summary>
/// @see core/window/utils/class_3835.as
/// @see core/window/utils/ChildEntityArray.as
public class ChildEntityArray<T> : IChildEntityArray where T : class, IChildEntity
{
    protected readonly List<T> _children = [];

    public int NumChildren => _children.Count;

    /// @see class_3835.as::getChildAt
    public IChildEntity GetChildAt(int index)
    {
        return _children[index];
    }

    public T GetTypedChildAt(int index)
    {
        return _children[index];
    }

    /// @see class_3835.as::getChildByID
    public IChildEntity? GetChildByID(uint id)
    {
        return _children.FirstOrDefault(child => child.Id == id);
    }

    /// @see class_3835.as::getChildByName
    public IChildEntity? GetChildByName(string name)
    {
        return _children.FirstOrDefault(child => child.Name == name);
    }

    /// @see class_3835.as::getChildIndex
    public int GetChildIndex(IChildEntity child)
    {
        return _children.IndexOf((T)child);
    }

    /// @see class_3835.as::groupChildrenWithID
    public uint GroupChildrenWithID(uint id, IList<IChildEntity> results)
    {
        uint count = 0;

        foreach (T child in _children.Where(child => child.Id == id))
        {
            results.Add(child);
            count++;
        }

        return count;
    }

    /// @see ChildEntityArray.as::addChild
    public virtual IChildEntity AddChild(IChildEntity child)
    {
        _children.Add((T)child);

        return child;
    }

    /// @see ChildEntityArray.as::addChildAt
    public virtual IChildEntity AddChildAt(IChildEntity child, int index)
    {
        _children.Insert(index, (T)child);

        return child;
    }

    /// @see ChildEntityArray.as::removeChild
    public virtual IChildEntity? RemoveChild(IChildEntity child)
    {
        int index = _children.IndexOf((T)child);

        if (index < 0)
        {
            return null;
        }

        _children.RemoveAt(index);

        return child;
    }

    /// @see ChildEntityArray.as::removeChildAt
    public virtual IChildEntity? RemoveChildAt(int index)
    {
        if (index < 0 || index >= _children.Count)
        {
            return null;
        }

        T child = _children[index];

        _children.RemoveAt(index);

        return child;
    }

    /// @see ChildEntityArray.as::setChildIndex
    public void SetChildIndex(IChildEntity child, int index)
    {
        int current = _children.IndexOf((T)child);

        if (current <= -1 || index == current)
        {
            return;
        }

        _children.RemoveAt(current);
        _children.Insert(index, (T)child);
    }

    /// @see ChildEntityArray.as::swapChildren
    public void SwapChildren(IChildEntity? child1, IChildEntity? child2)
    {
        if (child1 == null || child2 == null || child1 == child2)
        {
            return;
        }

        int idx1 = _children.IndexOf((T)child1);

        if (idx1 < 0)
        {
            return;
        }

        int idx2 = _children.IndexOf((T)child2);

        if (idx2 < 0)
        {
            return;
        }

        // AS3 ensures idx1 < idx2 for splice order
        if (idx2 < idx1)
        {
            (child1, child2) = (child2, child1);
            (idx1, idx2) = (idx2, idx1);
        }

        _children.RemoveAt(idx2);
        _children.RemoveAt(idx1);
        _children.Insert(idx1, (T)child2);
        _children.Insert(idx2, (T)child1);
    }

    /// @see ChildEntityArray.as::swapChildrenAt
    public void SwapChildrenAt(int index1, int index2)
    {
        SwapChildren(_children[index1], _children[index2]);
    }
}
