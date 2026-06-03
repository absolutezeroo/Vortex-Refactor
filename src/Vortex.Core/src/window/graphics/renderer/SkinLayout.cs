// @see core/window/graphics/renderer/SkinLayout.as

using System;

using Godot;

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/SkinLayout.as
public class SkinLayout : ChildEntityArray<SkinLayoutEntity>, ISkinLayout, IDisposable
{
    private readonly string _name;

    /// @see SkinLayout.as::SkinLayout
    public SkinLayout(string name, bool transparent, string blendMode) : base()
    {
        _name = name;
        Width = 0;
        Height = 0;
        BlendMode = blendMode;
        Transparent = transparent;
    }

    string ISkinLayout.Name => _name;
    public uint Width { get; private set; }

    public uint Height { get; private set; }

    public string BlendMode { get; }

    public bool Transparent { get; }

    /// @see SkinLayout.as::dispose
    public void Dispose()
    {
        uint count = (uint)NumChildren;

        for (uint i = 0;
             i < count;
             i++)
        {
            ((SkinLayoutEntity)RemoveChildAt(0)!).Dispose();
        }
    }

    /// @see SkinLayout.as::calculateActualRect
    public void CalculateActualRect(ref Rect2 result)
    {
        uint count = (uint)NumChildren;

        result = new Rect2(uint.MaxValue, uint.MaxValue, 0, 0);

        for (uint i = 0;
             i < count;
             i++)
        {
            SkinLayoutEntity entity = GetTypedChildAt((int)i);
            Rect2 region = entity.Region;

            if (region.Position.X < result.Position.X)
            {
                result = new Rect2(region.Position.X, result.Position.Y, result.Size.X, result.Size.Y);
            }

            if (region.Position.Y < result.Position.Y)
            {
                result = new Rect2(result.Position.X, region.Position.Y, result.Size.X, result.Size.Y);
            }

            if (region.End.X > result.End.X)
            {
                result = new Rect2(result.Position.X, result.Position.Y, region.End.X - result.Position.X, result.Size.Y);
            }

            if (region.End.Y > result.End.Y)
            {
                result = new Rect2(result.Position.X, result.Position.Y, result.Size.X, region.End.Y - result.Position.Y);
            }
        }
    }

    /// @see SkinLayout.as::isFixedWidth
    public bool IsFixedWidth()
    {
        uint count = (uint)NumChildren;

        if (count == 0)
        {
            return false;
        }

        for (uint i = 0;
             i < count;
             i++)
        {
            if (GetTypedChildAt((int)i).ScaleH != 0)
            {
                return false;
            }
        }

        return true;
    }

    /// @see SkinLayout.as::calculateWidth
    public uint CalculateWidth()
    {
        uint maxRight = 0;
        uint count = (uint)NumChildren;

        for (uint i = 0;
             i < count;
             i++)
        {
            Rect2 region = GetTypedChildAt((int)i).Region;
            uint right = (uint)region.End.X;

            if (right > maxRight)
            {
                maxRight = right;
            }
        }

        return maxRight;
    }

    /// @see SkinLayout.as::isFixedHeight
    public bool IsFixedHeight()
    {
        uint count = (uint)NumChildren;

        if (count == 0)
        {
            return false;
        }

        for (uint i = 0;
             i < count;
             i++)
        {
            if (GetTypedChildAt((int)i).ScaleV != 0)
            {
                return false;
            }
        }

        return true;
    }

    /// @see SkinLayout.as::calculateHeight
    public uint CalculateHeight()
    {
        uint maxBottom = 0;
        uint count = (uint)NumChildren;

        for (uint i = 0;
             i < count;
             i++)
        {
            Rect2 region = GetTypedChildAt((int)i).Region;
            uint bottom = (uint)region.End.Y;

            if (bottom > maxBottom)
            {
                maxBottom = bottom;
            }
        }

        return maxBottom;
    }

    /// @see SkinLayout.as::getDefaultRegion
    public void GetDefaultRegion(string entityName, ref Rect2 result)
    {
        if (GetChildByName(entityName) is not SkinLayoutEntity entity)
        {
            throw new Exception($"Entity not found: {entityName}!");
        }

        result = entity.Region;
    }

    /// @see SkinLayout.as::addChild (override updates width/height)
    public override IChildEntity AddChild(IChildEntity child)
    {
        SkinLayoutEntity entity = (SkinLayoutEntity)child;
        uint right = (uint)entity.Region.End.X;
        uint bottom = (uint)entity.Region.End.Y;

        if (right > Width)
        {
            Width = right;
        }

        if (bottom > Height)
        {
            Height = bottom;
        }

        return base.AddChild(child);
    }

    /// @see SkinLayout.as::addChildAt (override updates width/height)
    public override IChildEntity AddChildAt(IChildEntity child, int index)
    {
        SkinLayoutEntity entity = (SkinLayoutEntity)child;
        uint right = (uint)entity.Region.End.X;
        uint bottom = (uint)entity.Region.End.Y;

        if (right > Width)
        {
            Width = right;
        }

        if (bottom > Height)
        {
            Height = bottom;
        }

        return base.AddChildAt(child, index);
    }

    /// @see SkinLayout.as::removeChild (override recalculates width/height)
    public override IChildEntity? RemoveChild(IChildEntity child)
    {
        base.RemoveChild(child);

        Width = CalculateWidth();
        Height = CalculateHeight();

        return child;
    }

    /// @see SkinLayout.as::removeChildAt (override recalculates width/height)
    public override IChildEntity? RemoveChildAt(int index)
    {
        IChildEntity? child = base.RemoveChildAt(index);

        Width = CalculateWidth();
        Height = CalculateHeight();

        return child;
    }
}
