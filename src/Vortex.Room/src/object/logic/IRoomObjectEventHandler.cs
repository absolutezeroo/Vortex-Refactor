using System.Xml.Linq;

using Vortex.Room.Messages;

namespace Vortex.Room.Object.Logic;

/// @see com.sulake.room.object.logic.IRoomObjectEventHandler
public interface IRoomObjectEventHandler : IRoomObjectMouseHandler
{
    IRoomObjectController? Object { get; set; }
    void Dispose();
    void Initialize(XElement? xml);
    void TearDown();
    void Update(int time);
    void ProcessUpdateMessage(RoomObjectUpdateMessage message);
    void UseObject();
    object? EventDispatcher { get; set; }
    string[]? GetEventTypes();
    string? Widget { get; }
    string? ContextMenu { get; }
}
