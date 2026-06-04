// @see com.sulake.habbo.communication.messages.parser.room.data.GetGuestRoomResultMessageParser

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.RoomSettings;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Data;

/// @see com.sulake.habbo.communication.messages.parser.room.data.GetGuestRoomResultMessageParser
public class GetGuestRoomResultMessageEventParser : IMessageParser
{
    public bool RoomForward { get; private set; }
    public int RoomId { get; private set; }
    public string? RoomName { get; private set; }
    public int OwnerId { get; private set; }
    public string? OwnerName { get; private set; }
    public int DoorMode { get; private set; }
    public int MaxUsers { get; private set; }
    public int CurrentUsers { get; private set; }
    public string? Description { get; private set; }
    public int TradeMode { get; private set; }
    public int Score { get; private set; }
    public int Ranking { get; private set; }
    public int CategoryId { get; private set; }
    public bool IsGuildRoom { get; private set; }
    public bool ArePetsAllowed { get; private set; }
    public RoomModerationSettings? ModerationSettings { get; private set; }

    public bool Flush()
    {
        RoomForward = false;
        RoomId = 0;
        RoomName = null;
        OwnerId = 0;
        OwnerName = null;
        DoorMode = 0;
        MaxUsers = 0;
        CurrentUsers = 0;
        Description = null;
        TradeMode = 0;
        Score = 0;
        Ranking = 0;
        CategoryId = 0;
        IsGuildRoom = false;
        ArePetsAllowed = false;
        ModerationSettings = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify full wire format order from AS3 source — GetGuestRoomResultParser is large
        RoomForward = param1.ReadBoolean();
        RoomId = param1.ReadInteger();
        RoomName = param1.ReadString();
        OwnerId = param1.ReadInteger();
        OwnerName = param1.ReadString();
        DoorMode = param1.ReadInteger();
        MaxUsers = param1.ReadInteger();
        CurrentUsers = param1.ReadInteger();
        Description = param1.ReadString();
        TradeMode = param1.ReadInteger();
        Score = param1.ReadInteger();
        Ranking = param1.ReadInteger();
        CategoryId = param1.ReadInteger();
        int tagCount = param1.ReadInteger();
        for (int i = 0; i < tagCount; i++)
        {
            param1.ReadString(); // tags (not needed for session update)
        }

        param1.ReadInteger(); // isPublicRoom flags
        IsGuildRoom = param1.ReadBoolean();
        ArePetsAllowed = param1.ReadBoolean();
        ModerationSettings = new RoomModerationSettings(param1);
        return true;
    }
}
