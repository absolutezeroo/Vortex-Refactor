using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Habbo.Avatar;
using Vortex.Room.Object.Visualization;

namespace Vortex.Habbo.Room.Object.Visualization.Avatar;

/// @see com.sulake.habbo.room.object.visualization.avatar.AvatarVisualizationData
public class AvatarVisualizationData : IRoomObjectVisualizationData
{
    public IAvatarRenderManager? AvatarRenderer { get; set; }

    public bool Initialize(XElement xml)
    {
        return true;
    }

    public void Dispose()
    {
        AvatarRenderer = null;
    }

    /// @see AvatarVisualizationData.as::getAvatar
    public IAvatarImage? GetAvatar(string? figure, double scale, string? gender,
        IAvatarImageListener? listener = null, IAvatarEffectListener? effectListener = null)
    {
        if (AvatarRenderer == null)
        {
            return null;
        }

        if (scale > 48)
        {
            return AvatarRenderer.CreateAvatarImage(figure!, "h", gender, listener, effectListener);
        }

        return AvatarRenderer.CreateAvatarImage(figure!, "h_50", gender, listener, effectListener);
    }

    public double GetLayerCount(string type)
    {
        return 0;
    }

    /// @see AvatarVisualizationData.as::getAvatarRendererAsset
    public IAsset? GetAvatarRendererAsset(string name)
    {
        if (AvatarRenderer == null)
        {
            return null;
        }

        return AvatarRenderer.Assets.GetAssetByName(name);
    }
}
