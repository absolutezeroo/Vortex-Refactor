using Godot;

namespace Vortex.Core.Utils;

/// <summary>
/// Godot node lifecycle helpers.
/// </summary>
public static class NodeUtils
{
    /// <summary>
    /// Queues a node for deletion only when it is orphaned (no parent).
    /// Nodes still in the scene tree are managed by Godot's tree cleanup.
    /// </summary>
    public static void FreeIfOrphaned(Node? node)
    {
        if (node != null && GodotObject.IsInstanceValid(node) && node.GetParent() == null)
        {
            node.QueueFree();
        }
    }
}
