using System;
using System.Globalization;

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Furniture;

/// @see com.sulake.habbo.communication.messages.parser.room.furniture.RoomDimmerPresetsMessageParser
public class RoomDimmerPresetsMessageEventParser : IMessageParser
{
    private readonly List<RoomDimmerPresetsMessageData> _presets = [];

    public int PresetCount => _presets.Count;
    public int SelectedPresetId { get; private set; }

    public RoomDimmerPresetsMessageData? GetPreset(int index)
    {
        return index >= 0 && index < _presets.Count ? _presets[index] : null;
    }

    public bool Flush()
    {
        _presets.Clear();
        SelectedPresetId = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        _presets.Clear();
        int count = param1.ReadInteger();
        SelectedPresetId = param1.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            int presetId = param1.ReadInteger();
            int type = param1.ReadInteger();
            string colorStr = param1.ReadString();
            int color = int.Parse(colorStr.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int light = param1.ReadInteger();
            RoomDimmerPresetsMessageData preset = new(presetId)
            {
                Type = type,
                Color = color,
                Light = light,
            };
            preset.SetReadOnly();
            _presets.Add(preset);
        }
        return true;
    }
}
