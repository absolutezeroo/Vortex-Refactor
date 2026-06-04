// @see com.sulake.habbo.session.events.RoomSessionQueueEvent

using System;

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionQueueEvent
public class RoomSessionQueueEvent : RoomSessionEvent
{
    public const string QUEUE_STATUS = "RSQE_QUEUE_STATUS";
    public const string QUEUE_TYPE_DOOR = "c";
    public const string QUEUE_TYPE_NORMAL = "d";
    public const int QUEUE_TYPE_COUNT = 2;
    public const int ACTIVE_QUEUE_SIZE = 1;

    private readonly Dictionary<string, int> _queues = new(StringComparer.Ordinal);

    /// @see RoomSessionQueueEvent.as::RoomSessionQueueEvent
    public RoomSessionQueueEvent(IRoomSession session, string queueSetName, int queueSetTarget, bool isActive = false)
        : base(QUEUE_STATUS, session)
    {
        this.queueSetName = queueSetName;
        this.queueSetTarget = queueSetTarget;
        this.isActive = isActive;
    }

    /// @see RoomSessionQueueEvent.as::get isActive
    public bool isActive { get; }

    /// @see RoomSessionQueueEvent.as::get queueSetName
    public string queueSetName { get; }

    /// @see RoomSessionQueueEvent.as::get queueSetTarget
    public int queueSetTarget { get; }

    /// @see RoomSessionQueueEvent.as::get queueTypes
    public IReadOnlyCollection<string> queueTypes => _queues.Keys;

    /// @see RoomSessionQueueEvent.as::getQueueSize
    public int GetQueueSize(string queueType)
    {
        return _queues.TryGetValue(queueType, out int size) ? size : 0;
    }

    /// @see RoomSessionQueueEvent.as::addQueue
    public void AddQueue(string queueType, int size)
    {
        _queues[queueType] = size;
    }
}
