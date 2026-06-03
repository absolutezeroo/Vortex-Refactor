// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/class_3557.as

namespace Vortex.Habbo.Avatar.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/class_3557.as
public interface IAnimation
{
    /// @see class_3557.as::get id
    string Id { get; }

    /// @see class_3557.as::hasAvatarData
    bool HasAvatarData();

    /// @see class_3557.as::hasDirectionData
    bool HasDirectionData();

    /// @see class_3557.as::hasAddData
    bool HasAddData();

    /// @see class_3557.as::get spriteData
    IList<ISpriteDataContainer>? SpriteData { get; }

    /// @see class_3557.as::get removeData
    IList<string> RemoveData { get; }

    /// @see class_3557.as::get addData
    IList<AddDataContainer> AddData { get; }

    /// @see class_3557.as::get resetOnToggle
    bool ResetOnToggle { get; }
}
