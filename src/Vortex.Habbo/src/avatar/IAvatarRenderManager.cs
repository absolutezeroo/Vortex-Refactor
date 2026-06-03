// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/IAvatarRenderManager.as

using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Runtime;
using Vortex.Habbo.Avatar.Animation;
using Vortex.Habbo.Avatar.Structure;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/IAvatarRenderManager.as
public interface IAvatarRenderManager : IUnknown
{
    /// @see IAvatarRenderManager.as::createAvatarImage
    IAvatarImage? CreateAvatarImage
    (
        string param1, string param2, string? param3 = null,
        IAvatarImageListener? param4 = null, IAvatarEffectListener? param5 = null
    );

    /// @see IAvatarRenderManager.as::get assets
    IAssetLibrary Assets { get; }

    /// @see IAvatarRenderManager.as::getFigureData
    IFigureData? GetFigureData();

    /// @see IAvatarRenderManager.as::getFigureStringWithFigureIds
    string GetFigureStringWithFigureIds(string param1, string param2, int[] param3);

    /// @see IAvatarRenderManager.as::isValidFigureSetForGender
    bool IsValidFigureSetForGender(int param1, string param2);

    /// @see IAvatarRenderManager.as::getMandatoryAvatarPartSetIds
    string[]? GetMandatoryAvatarPartSetIds(string param1, int param2);

    /// @see IAvatarRenderManager.as::getAssetByName
    IAsset? GetAssetByName(string param1);

    /// @see IAvatarRenderManager.as::get mode
    string? Mode { get; set; }

    /// @see IAvatarRenderManager.as::injectFigureData
    void InjectFigureData(XElement param1);

    /// @see IAvatarRenderManager.as::createFigureContainer
    IFigureContainer CreateFigureContainer(string param1);

    /// @see IAvatarRenderManager.as::isFigureReady
    bool IsFigureReady(IFigureContainer param1);

    /// @see IAvatarRenderManager.as::downloadFigure
    void DownloadFigure(IFigureContainer param1, IAvatarImageListener param2);

    /// @see IAvatarRenderManager.as::getAnimationManager
    IAnimationManager? GetAnimationManager();

    /// @see IAvatarRenderManager.as::resetAssetManager
    void ResetAssetManager();

    /// @see IAvatarRenderManager.as::resolveClubLevel
    int ResolveClubLevel(IFigureContainer param1, string param2, string[]? param3 = null);

    /// @see IAvatarRenderManager.as::getItemIds
    string[]? GetItemIds();

    /// @see IAvatarRenderManager.as::get effectMap
    IDictionary<int, object>? EffectMap { get; }

    /// @see IAvatarRenderManager.as::purgeAssets
    void PurgeAssets();
}
