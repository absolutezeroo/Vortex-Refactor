using System;
using System.Globalization;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.UserUpdateMessageEventParser
public class UserUpdateMessageEventParser : IMessageParser
{
    private readonly List<UserUpdateMessageData> _users = [];

    public int UserUpdateCount => _users.Count;

    public UserUpdateMessageData? GetUserUpdateData(int index)
    {
        return index >= 0 && index < _users.Count ? _users[index] : null;
    }

    public bool Flush()
    {
        _users.Clear();
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        _users.Clear();
        int count = param1.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            int id = param1.ReadInteger();
            double x = (double)param1.ReadInteger();
            double y = (double)param1.ReadInteger();
            double z = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
            double localZ = 0;
            int bodyDir = param1.ReadInteger();
            int headDir = param1.ReadInteger();
            string actionString = param1.ReadString();
            bool canStandUp = false;
            int dir = bodyDir % 8 * 45;
            int dirHead = headDir % 8 * 45;
            List<AvatarActionMessageData> actions = new();
            double targetX = 0, targetY = 0, targetZ = 0;
            bool isMoving = false;

            string[] actionParts = actionString.Split('/');
            bool skipPositionUpdate = false;
            foreach (string actionPart in actionParts)
            {
                string[] tokens = actionPart.Split(' ');
                string actionType = tokens[0];
                string actionParam = "";
                if (actionType == "")
                {
                    continue;
                }

                if (actionType == "wf")
                {
                    skipPositionUpdate = true;
                }

                if (tokens.Length >= 2)
                {
                    actionParam = tokens[1];
                    switch (actionType)
                    {
                        case "mv":
                            string[] coords = actionParam.Split(',');
                            if (coords.Length >= 3)
                            {
                                targetX = int.Parse(coords[0], CultureInfo.InvariantCulture);
                                targetY = int.Parse(coords[1], CultureInfo.InvariantCulture);
                                targetZ = double.Parse(coords[2], CultureInfo.InvariantCulture);
                                isMoving = true;
                            }
                            break;
                        case "sit":
                            localZ = double.Parse(actionParam, CultureInfo.InvariantCulture);
                            if (tokens.Length >= 3)
                            {
                                canStandUp = tokens[2] == "1";
                            }
                            break;
                        case "lay":
                            double layZ = double.Parse(actionParam, CultureInfo.InvariantCulture);
                            localZ = Math.Abs(layZ);
                            break;
                    }
                }

                actions.Add(new AvatarActionMessageData(actionType, actionParam));
            }

            _users.Add(new UserUpdateMessageData(
                id, x, y, z, localZ, dirHead, dir,
                targetX, targetY, targetZ, isMoving, canStandUp,
                actions, skipPositionUpdate));
        }
        return true;
    }
}
