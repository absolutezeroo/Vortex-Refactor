using Godot;

namespace Vortex.Core.Utils;

/// <summary>
/// AS3 Sprite.width/height auto-computes the bounding box of all children.
/// Godot Control.Size does not. This utility emulates the AS3 behavior by
/// recursively scanning descendant Controls and returning the bounding rect.
/// </summary>
public static class DisplayUtils
{
    public static Rect2 ComputeDescendantBounds(Node root)
    {
        float minX = float.MaxValue,
            minY = float.MaxValue;
        float maxX = float.MinValue,
            maxY = float.MinValue;

        ScanBounds(root, Vector2.Zero, ref minX, ref minY, ref maxX, ref maxY);

        if (minX > maxX || minY > maxY)
        {
            return new Rect2(0, 0, 0, 0);
        }

        return new Rect2(minX, minY, maxX - minX, maxY - minY);
    }

    private static void ScanBounds
    (
        Node parent,
        Vector2 offset,
        ref float minX,
        ref float minY,
        ref float maxX,
        ref float maxY
    )
    {
        for (int i = 0;
             i < parent.GetChildCount();
             i++)
        {
            if (parent.GetChild(i) is not Control child)
            {
                continue;
            }

            Vector2 pos = offset + child.Position;
            float right = pos.X + child.Size.X;
            float bottom = pos.Y + child.Size.Y;

            if (pos.X < minX)
            {
                minX = pos.X;
            }

            if (pos.Y < minY)
            {
                minY = pos.Y;
            }

            if (right > maxX)
            {
                maxX = right;
            }

            if (bottom > maxY)
            {
                maxY = bottom;
            }

            ScanBounds(child, pos, ref minX, ref minY, ref maxX, ref maxY);
        }
    }
}
