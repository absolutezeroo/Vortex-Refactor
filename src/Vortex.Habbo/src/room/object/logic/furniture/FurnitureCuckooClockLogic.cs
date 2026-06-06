namespace Vortex.Habbo.Room.Object.Logic;

using System;

using Events;
using Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureCuckooClockLogic
public class FurnitureCuckooClockLogic : FurnitureMultiStateLogic
{
    private int _lastState;

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectPlaySoundIdEvent.PLAY_SOUND_AT_PITCH];
        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage || Object == null)
        {
            return;
        }

        int state = Object.GetState(0);

        if (state == _lastState)
        {
            return;
        }

        _lastState = state;

        PlaySoundAt();
    }

    private void PlaySoundAt()
    {
        if (Object == null)
        {
            return;
        }

        double z = Object.Location.Z;
        double pitch = Math.Pow(2.0, z - 1.2);

        DispatchEvent(new RoomObjectPlaySoundIdEvent(
            RoomObjectPlaySoundIdEvent.PLAY_SOUND_AT_PITCH,
            Object,
            "FURNITURE_cuckoo_clock",
            pitch));
    }
}
