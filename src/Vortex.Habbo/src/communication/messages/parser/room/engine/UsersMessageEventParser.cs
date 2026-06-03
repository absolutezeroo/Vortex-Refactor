using System;
using System.Globalization;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.UsersMessageEventParser
public class UsersMessageEventParser : IMessageParser
{
    private static readonly string[] OldPetColors =
    [
        "FF7B3A", "FF9763", "FFCDB3", "F59500", "FBBD5C", "FEE4B2", "EDD400", "F5E759", "FBF8B1",
        "84A95F", "B0C993", "DBEFC7", "65B197", "91C7B5", "C5EDDE", "7F89B2", "98A1C5", "CAD2EC",
        "A47FB8", "C09ED5", "DBC7E9", "BD7E9D", "DA9DBD", "ECC6DB", "DD7B7D", "F08B90", "F9BABF",
        "ABABAB", "D4D4D4", "FFFFFF", "D98961", "DFA281", "F1D2C2", "D5B35F", "DAC480", "FCFAD3",
        "EAA7AF", "86BC40", "E8CE25", "8E8839", "888F67", "5E9414", "84CE84", "96E75A", "88E70D",
        "B99105", "C8D71D", "838851", "C08337", "83A785", "E6AF26", "ECFF99", "94FFF9", "ABC8E5",
        "F2E5CC", "D2FF00",
    ];

    private static readonly string[] SwimColors =
    [
        "238,238,238", "250,56,49", "253,146,160", "42,199,210", "53,51,44", "239,255,146",
        "198,255,152", "255,146,90", "157,89,126", "182,243,255", "109,255,51", "51,120,201",
        "255,182,49", "223,161,233", "249,251,50", "202,175,143", "197,198,197", "71,98,61",
        "138,131,97", "255,140,51", "84,198,39", "30,108,153", "152,79,136", "119,200,255",
        "255,192,142", "60,75,135", "124,44,71", "215,255,227", "143,63,28", "255,99,147",
        "31,155,121", "253,255,51",
    ];

    private readonly List<UserMessageData> _users = [];

    public int UserCount => _users.Count;

    public UserMessageData? GetUser(int index)
    {
        if (index < 0 || index >= _users.Count)
        {
            return null;
        }
        UserMessageData data = _users[index];
        data.SetReadOnly();
        return data;
    }

    public bool Flush()
    {
        _users.Clear();
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        _users.Clear();
        int count = param1.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            int webId = param1.ReadInteger();
            string name = param1.ReadString();
            string custom = param1.ReadString();
            string figure = param1.ReadString();
            int roomIndex = param1.ReadInteger();
            int x = param1.ReadInteger();
            int y = param1.ReadInteger();
            string zStr = param1.ReadString();
            int dir = param1.ReadInteger();
            int userType = param1.ReadInteger();

            UserMessageData user = new(roomIndex)
            {
                Dir = dir,
                Name = name,
                Custom = custom,
                X = x,
                Y = y,
                Z = double.Parse(zStr, CultureInfo.InvariantCulture),
            };
            _users.Add(user);

            switch (userType)
            {
                case 1:
                    user.WebID = webId;
                    user.UserType = 1;
                    user.Sex = ResolveSex(param1.ReadString());
                    user.GroupID = param1.ReadInteger().ToString(CultureInfo.InvariantCulture);
                    user.GroupStatus = param1.ReadInteger();
                    user.GroupName = param1.ReadString();
                    string swimFigure = param1.ReadString();
                    if (swimFigure != "")
                    {
                        figure = ConvertSwimFigure(swimFigure, figure, user.Sex);
                    }
                    user.Figure = figure;
                    user.AchievementScore = param1.ReadInteger();
                    user.IsModerator = param1.ReadBoolean();
                    break;
                case 2:
                    user.UserType = 2;
                    user.Figure = figure;
                    user.WebID = webId;
                    user.SubType = param1.ReadInteger().ToString(CultureInfo.InvariantCulture);
                    user.OwnerId = param1.ReadInteger();
                    user.OwnerName = param1.ReadString();
                    user.RarityLevel = param1.ReadInteger();
                    user.HasSaddle = param1.ReadBoolean();
                    user.IsRiding = param1.ReadBoolean();
                    user.CanBreed = param1.ReadBoolean();
                    user.CanHarvest = param1.ReadBoolean();
                    user.CanRevive = param1.ReadBoolean();
                    user.HasBreedingPermission = param1.ReadBoolean();
                    user.PetLevel = param1.ReadInteger();
                    user.PetPosture = param1.ReadString();
                    break;
                case 3:
                    user.UserType = 3;
                    user.WebID = roomIndex * -1;
                    user.Figure = figure.Contains('/')
                        ? "hr-100-.hd-180-1.ch-876-66.lg-270-94.sh-300-64"
                        : figure;
                    user.Sex = "M";
                    break;
                case 4:
                    user.UserType = 4;
                    user.WebID = webId;
                    user.Sex = ResolveSex(param1.ReadString());
                    user.Figure = figure;
                    user.OwnerId = param1.ReadInteger();
                    user.OwnerName = param1.ReadString();
                    int skillCount = param1.ReadInteger();
                    if (skillCount > 0)
                    {
                        List<int> skills = new();
                        for (int j = 0; j < skillCount; j++)
                        {
                            skills.Add(param1.ReadShort());
                        }
                        user.BotSkills = skills;
                    }
                    break;
            }
        }
        return true;
    }

    public static string ConvertOldPetFigure(string param1)
    {
        string[] parts = param1.Split(' ');
        if (parts.Length < 3)
        {
            return "";
        }
        int breed = int.Parse(parts[0], CultureInfo.InvariantCulture);
        int variant = int.Parse(parts[1], CultureInfo.InvariantCulture) + 1;
        string colorStr = parts[2];
        colorStr = colorStr[^6..];
        int maxVariant = 25;
        int partId;
        if (breed <= 1)
        {
            partId = (maxVariant * breed) + variant;
        }
        else
        {
            partId = 64;
        }
        int colorIndex = Array.IndexOf(OldPetColors, colorStr.ToUpperInvariant()) + 1;
        return $"phd-{partId}-{colorIndex}.pbd-{partId}-{colorIndex}.ptl-{partId}-{colorIndex}";
    }

    private static string ResolveSex(string param1)
    {
        return param1.Length > 0 && char.ToLowerInvariant(param1[0]) == 'f' ? "F" : "M";
    }

    private static string ConvertSwimFigure(string swimData, string figure, string sex)
    {
        int skinColor = 1;
        string[] figureParts = figure.Split('.');
        foreach (string part in figureParts)
        {
            string[] partPieces = part.Split('-');
            if (partPieces.Length > 2 && partPieces[0] == "hd")
            {
                skinColor = int.Parse(partPieces[2], CultureInfo.InvariantCulture);
            }
        }

        int swimBodyType = 10001;
        int swimSuitType = sex == "F" ? 10010 : 10011;
        int swimColorId = 10000;

        string[] eqParts = swimData.Split('=');
        if (eqParts.Length > 1)
        {
            string[] valParts = eqParts[1].Split('/');
            string colorStr = valParts.Length > 1 ? valParts[1] : "";
            int colorIndex = Array.IndexOf(SwimColors, colorStr);
            swimColorId = 10000 + colorIndex + 1;
        }

        return figure + $".bds-{swimBodyType}-{skinColor}.ss-{swimSuitType}-{swimColorId}";
    }
}
