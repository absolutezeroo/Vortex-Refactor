using System.Globalization;

using Vortex.Core.Communication.Messages;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.WiredMovementsMessageEventParser
public class WiredMovementsMessageEventParser : IMessageParser
{
    private readonly List<WiredUserMoveData> _userMoves = [];
    private readonly List<WiredFurniMoveData> _furniMoves = [];
    private readonly List<WiredWallItemMoveData> _wallItemMoves = [];
    private readonly List<WiredUserDirUpdateData> _userDirectionUpdates = [];

    public IReadOnlyList<WiredUserMoveData> UserMoves => _userMoves;
    public IReadOnlyList<WiredFurniMoveData> FurniMoves => _furniMoves;
    public IReadOnlyList<WiredWallItemMoveData> WallItemMoves => _wallItemMoves;
    public IReadOnlyList<WiredUserDirUpdateData> UserDirectionUpdates => _userDirectionUpdates;

    public bool Flush()
    {
        _userMoves.Clear();
        _furniMoves.Clear();
        _wallItemMoves.Clear();
        _userDirectionUpdates.Clear();
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        _userMoves.Clear();
        _furniMoves.Clear();
        _wallItemMoves.Clear();
        _userDirectionUpdates.Clear();
        int count = param1.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            int type = param1.ReadInteger();
            switch (type)
            {
                case 0:
                    _userMoves.Add(ParseUserMove(param1));
                    break;
                case 1:
                    _furniMoves.Add(ParseFurniMove(param1));
                    break;
                case 2:
                    _wallItemMoves.Add(ParseWallItemMove(param1));
                    break;
                case 3:
                    _userDirectionUpdates.Add(ParseUserDirUpdate(param1));
                    break;
            }
        }
        return true;
    }

    private static WiredUserMoveData ParseUserMove(IMessageDataWrapper param1)
    {
        double srcX = (double)param1.ReadInteger();
        double srcY = (double)param1.ReadInteger();
        double tgtX = (double)param1.ReadInteger();
        double tgtY = (double)param1.ReadInteger();
        double srcZ = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
        double tgtZ = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
        double userIndex = (double)param1.ReadInteger();
        double moveTypeId = (double)param1.ReadInteger();
        double animTime = (double)param1.ReadInteger();
        double bodyDir = (double)param1.ReadInteger();
        double headDir = (double)param1.ReadInteger();
        return new WiredUserMoveData(
            (int)userIndex, new Vector3d(srcX, srcY, srcZ), new Vector3d(tgtX, tgtY, tgtZ),
            moveTypeId == 0 ? "mv" : "sld", animTime, bodyDir, headDir);
    }

    private static WiredFurniMoveData ParseFurniMove(IMessageDataWrapper param1)
    {
        double srcX = (double)param1.ReadInteger();
        double srcY = (double)param1.ReadInteger();
        double tgtX = (double)param1.ReadInteger();
        double tgtY = (double)param1.ReadInteger();
        double srcZ = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
        double tgtZ = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
        double furniId = (double)param1.ReadInteger();
        double animTime = (double)param1.ReadInteger();
        double rotation = (double)param1.ReadInteger();
        return new WiredFurniMoveData(
            (int)furniId, new Vector3d(srcX, srcY, srcZ), new Vector3d(tgtX, tgtY, tgtZ),
            animTime, rotation);
    }

    private static WiredWallItemMoveData ParseWallItemMove(IMessageDataWrapper param1)
    {
        int itemId = param1.ReadInteger();
        bool isDirectionRight = param1.ReadBoolean();
        int oldWallX = param1.ReadInteger();
        int oldWallY = param1.ReadInteger();
        int oldOffsetX = param1.ReadInteger();
        int oldOffsetY = param1.ReadInteger();
        int newWallX = param1.ReadInteger();
        int newWallY = param1.ReadInteger();
        int newOffsetX = param1.ReadInteger();
        int newOffsetY = param1.ReadInteger();
        int animTime = param1.ReadInteger();
        return new WiredWallItemMoveData(
            itemId, isDirectionRight,
            oldWallX, oldWallY, oldOffsetX, oldOffsetY,
            newWallX, newWallY, newOffsetX, newOffsetY,
            animTime);
    }

    private static WiredUserDirUpdateData ParseUserDirUpdate(IMessageDataWrapper param1)
    {
        int userIndex = param1.ReadInteger();
        int bodyDir = param1.ReadInteger();
        int headDir = param1.ReadInteger();
        return new WiredUserDirUpdateData(userIndex, bodyDir, headDir);
    }
}
