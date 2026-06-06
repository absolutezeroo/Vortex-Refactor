namespace Vortex.Habbo.Room.Object.Logic;

using System;
using System.Globalization;
using System.Xml.Linq;

using Events;
using Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureSoundBlockLogic (class_3451)
public class FurnitureSoundBlockLogic : FurnitureMultiStateLogic
{
    private const int SEMITONES_TOTAL = 12;
    private const int SEMITONES_OFFSET = -12;

    private int _sampleId;
    private bool _noPitch;
    private int _lastState;

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectSamplePlaybackEvent.PLAY_SAMPLE,
            RoomObjectSamplePlaybackEvent.ROOM_OBJECT_INITIALIZED,
            RoomObjectSamplePlaybackEvent.ROOM_OBJECT_DISPOSED,
            RoomObjectSamplePlaybackEvent.CHANGE_PITCH,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Initialize(XElement? xml)
    {
        base.Initialize(xml);

        if (xml == null || Object == null)
        {
            return;
        }

        XElement? sound = xml.Element("sound");

        if (sound != null)
        {
            XElement? sample = sound.Element("sample");

            if (sample != null)
            {
                string? idStr = (string?)sample.Attribute("id");

                if (idStr != null && int.TryParse(idStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
                {
                    _sampleId = id;
                }

                string? noPitchStr = (string?)sample.Attribute("nopitch");
                _noPitch = noPitchStr == "true";
            }
        }

        DispatchEvent(new RoomObjectSamplePlaybackEvent(
            RoomObjectSamplePlaybackEvent.ROOM_OBJECT_INITIALIZED, Object, _sampleId));
    }

    public override void Dispose()
    {
        if (Object != null)
        {
            DispatchEvent(new RoomObjectSamplePlaybackEvent(
                RoomObjectSamplePlaybackEvent.ROOM_OBJECT_DISPOSED, Object, _sampleId));
        }

        base.Dispose();
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        switch (message)
        {
            case RoomObjectDataUpdateMessage when Object != null:
                {
                    int state = Object.GetState(0);

                    if (state != _lastState)
                    {
                        _lastState = state;
                        double pitch = CalculatePitch();

                        DispatchEvent(new RoomObjectSamplePlaybackEvent(
                            RoomObjectSamplePlaybackEvent.PLAY_SAMPLE, Object, _sampleId, pitch));
                    }

                    break;
                }
            case RoomObjectMoveUpdateMessage when Object != null:
                {
                    double pitch = CalculatePitch();

                    DispatchEvent(new RoomObjectSamplePlaybackEvent(
                        RoomObjectSamplePlaybackEvent.CHANGE_PITCH, Object, _sampleId, pitch));

                    break;
                }
        }

    }

    private double CalculatePitch()
    {
        if (_noPitch || Object == null)
        {
            return 1.0;
        }

        double z = Object.Location.Z;
        int semitone = (int)z + SEMITONES_OFFSET;

        return Math.Pow(2.0, (double)semitone / SEMITONES_TOTAL);
    }
}
