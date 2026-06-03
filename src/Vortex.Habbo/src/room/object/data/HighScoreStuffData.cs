using Vortex.Core.Communication.Messages;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Data;

/// <summary>
/// High score StuffData (format key 6). Stores a state string, score/clear
/// type configuration, and a list of high score entries with user names.
/// Note: initializeFromIncomingMessage does NOT call base (no unique serial reading).
/// </summary>
/// @see com.sulake.habbo.room.object.data.HighScoreStuffData
public class HighScoreStuffData : StuffDataBase, IStuffData
{
    public const int FORMAT_KEY = 6;

    private string _state = "";

    public int ScoreType { get; private set; } = -1;

    public int ClearType { get; private set; } = -1;

    public List<HighScoreData> Entries { get; private set; } =
        [];

    public override void InitializeFromIncomingMessage(IMessageDataWrapper wrapper)
    {
        // NOTE: Does NOT call base — high score data has no unique serial info on the wire
        Entries = [];
        _state = wrapper.ReadString();
        ScoreType = wrapper.ReadInteger();
        ClearType = wrapper.ReadInteger();
        int entryCount = wrapper.ReadInteger();
        for (int i = 0; i < entryCount; i++)
        {
            HighScoreData entry = new()
            {
                Score = wrapper.ReadInteger(),
            };
            int userCount = wrapper.ReadInteger();
            for (int j = 0; j < userCount; j++)
            {
                entry.AddUser(wrapper.ReadString());
            }
            Entries.Add(entry);
        }
    }

    public override void InitializeFromRoomObjectModel(IRoomObjectModel model)
    {
        Entries = [];
        base.InitializeFromRoomObjectModel(model);
        ScoreType = (int)model.GetNumber(RoomObjectVariableEnum.FURNITURE_HIGHSCORE_SCORE_TYPE);
        ClearType = (int)model.GetNumber(RoomObjectVariableEnum.FURNITURE_HIGHSCORE_CLEAR_TYPE);
        int entryCount = (int)model.GetNumber(RoomObjectVariableEnum.FURNITURE_HIGHSCORE_DATA_ENTRY_COUNT);
        for (int i = 0; i < entryCount; i++)
        {
            HighScoreData entry = new()
            {
                Score = (int)model.GetNumber(RoomObjectVariableEnum.FURNITURE_HIGHSCORE_DATA_ENTRY_BASE_SCORE + i),
                Users = new List<string>(model.GetStringArray(RoomObjectVariableEnum.FURNITURE_HIGHSCORE_DATA_ENTRY_BASE_USERS + i) ?? []),
            };
            Entries.Add(entry);
        }
    }

    public override void WriteRoomObjectModel(IRoomObjectModelController model)
    {
        base.WriteRoomObjectModel(model);
        model.SetNumber(RoomObjectVariableEnum.FURNITURE_DATA_FORMAT, FORMAT_KEY);
        model.SetNumber(RoomObjectVariableEnum.FURNITURE_HIGHSCORE_SCORE_TYPE, ScoreType);
        model.SetNumber(RoomObjectVariableEnum.FURNITURE_HIGHSCORE_CLEAR_TYPE, ClearType);
        if (Entries != null)
        {
            model.SetNumber(RoomObjectVariableEnum.FURNITURE_HIGHSCORE_DATA_ENTRY_COUNT, Entries.Count);
            for (int i = 0; i < Entries.Count; i++)
            {
                HighScoreData entry = Entries[i];
                model.SetNumber(RoomObjectVariableEnum.FURNITURE_HIGHSCORE_DATA_ENTRY_BASE_SCORE + i, entry.Score);
                model.SetStringArray(RoomObjectVariableEnum.FURNITURE_HIGHSCORE_DATA_ENTRY_BASE_USERS + i, entry.Users.ToArray());
            }
        }
    }

    public override string GetLegacyString()
    {
        return _state;
    }
}
