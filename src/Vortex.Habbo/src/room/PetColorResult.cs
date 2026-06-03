namespace Vortex.Habbo.Room;

/// @see com.sulake.habbo.room.PetColorResult
public class PetColorResult(int primaryColor, int secondaryColor, int breed, int tagIndex, string id, bool isMaster, string[] layerTags)
{
    private static readonly string[] COLOR_TAGS =
    [
        "Null", "Black", "White", "Grey", "Red", "Orange", "Pink",
        "Green", "Lime", "Blue", "Light-Blue", "Dark-Blue", "Yellow",
        "Brown", "Dark-Brown", "Beige", "Cyan", "Purple", "Gold",
    ];

    public int PrimaryColor { get; } = primaryColor & 0xFFFFFF;

    public int SecondaryColor { get; } = secondaryColor & 0xFFFFFF;

    public int Breed { get; } = breed;

    public string Tag { get; } = tagIndex > -1 && tagIndex < COLOR_TAGS.Length ? COLOR_TAGS[tagIndex] : "";

    public string Id { get; } = id;

    public bool IsMaster { get; } = isMaster;

    public string[] LayerTags { get; } = layerTags;
}
