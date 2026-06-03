using System;

using Vortex.Room.Object.Logic;

namespace Vortex.Room;

/// @see com.sulake.room.IRoomObjectFactory
public interface IRoomObjectFactory
{
    void AddObjectEventListener(Action<object?> listener);
    void RemoveObjectEventListener(Action<object?> listener);
    IRoomObjectEventHandler? CreateRoomObjectLogic(string type);
    IRoomObjectManager CreateRoomObjectManager();
    object? Events { get; }
}
