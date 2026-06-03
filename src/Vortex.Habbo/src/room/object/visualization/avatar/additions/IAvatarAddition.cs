using Vortex.Room.Object.Visualization;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Room.Object.Visualization.Avatar.Additions;

/// @see com.sulake.habbo.room.object.visualization.avatar.additions.class_3545
public interface IAvatarAddition : IDisposable
{
    int Id { get; }

    void Update(IRoomObjectSprite sprite, double scale);

    bool Animate(IRoomObjectSprite sprite);
}
