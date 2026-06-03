namespace Vortex.Habbo.Room.Object.Data;

/// <summary>
/// Single high score entry containing a score and associated user names.
/// </summary>
/// @see com.sulake.habbo.room.object.data.HighScoreData
public class HighScoreData
{
    public int Score { get; set; } = -1;

    public List<string> Users { get; set; } =
        [];

    public void AddUser(string name)
    {
        Users.Add(name);
    }
}
