using Godot;

using Vortex.Habbo.Avatar;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// <summary>
/// Extends FurnitureVisualizationData with avatar rendering support for furniture
/// items that display avatar figures (e.g., mannequins).
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.furniture.AvatarFurnitureVisualizationData
public class AvatarFurnitureVisualizationData : FurnitureVisualizationData
{
    // TODO(avatar): AvatarVisualizationData dependency — avatar rendering on furniture
    // needs the avatar visualization subsystem (AvatarVisualizationData) which bridges
    // IAvatarRenderManager → IAvatarImage for furniture-hosted avatars (mannequins).
    // Without this, mannequin furniture will render blank.

    public IAvatarRenderManager? AvatarRenderer { get; set; }

    public override void Dispose()
    {
        base.Dispose();
        AvatarRenderer = null;
    }

    public IAvatarImage? GetAvatar(
        string figure,
        double size,
        string? gender = null,
        IAvatarImageListener? listener = null)
    {
        // TODO(avatar): delegate to AvatarVisualizationData.GetAvatar() once ported
        GD.PushWarning("[AvatarFurnitureVisualizationData] GetAvatar() not implemented — mannequin will render blank (figure: ", figure, ")");
        return null;
    }
}
