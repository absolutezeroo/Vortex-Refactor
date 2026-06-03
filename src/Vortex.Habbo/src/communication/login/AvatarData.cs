// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/AvatarData.as

using System.Text.Json.Nodes;

namespace Vortex.Habbo.Communication.Login;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/AvatarData.as
public sealed class AvatarData
{
    public AvatarData() { }

    public AvatarData(JsonObject? data)
    {
        if (data == null)
        {
            return;
        }

        UniqueId = ReadString(data, "uniqueId");
        Name = ReadString(data, "name");
        Motto = ReadString(data, "motto");
        Figure = ReadString(data, "figureString");
        Gender = ReadString(data, "gender");
        LastAccess = ReadInt(data, "lastWebAccess");
        HabboClubMember = ReadString(data, "habboClubMember") == "true";
        BuildersClubMember = ReadString(data, "buildersClubMember") == "true";
        CreationTime = ReadString(data, "creationTime");
    }

    public int Id { get; set; }
    public string UniqueId { get; set; } = string.Empty;
    public string Motto { get; set; } = string.Empty;
    public string Figure { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public bool HabboClubMember { get; set; }
    public bool BuildersClubMember { get; set; }
    public string CreationTime { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HeadFigure { get; set; } = string.Empty;
    public int LastAccess { get; set; }

    private static string ReadString(JsonObject data, string key)
    {
        return data[key]?.GetValue<string>() ?? string.Empty;
    }

    private static int ReadInt(JsonObject data, string key)
    {
        JsonNode? value = data[key];
        if (value == null)
        {
            return 0;
        }

        if (value is JsonValue jsonValue && jsonValue.TryGetValue(out int intValue))
        {
            return intValue;
        }

        if (int.TryParse(value.ToJsonString(), out int parsed))
        {
            return parsed;
        }

        return 0;
    }
}
