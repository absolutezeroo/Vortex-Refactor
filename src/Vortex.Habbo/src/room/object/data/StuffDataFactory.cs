namespace Vortex.Habbo.Room.Object.Data;

/// <summary>
/// Factory that creates the correct StuffData implementation based on the
/// format type encoded in the lower byte of the type field.
/// </summary>
/// @see com.sulake.habbo.room.object.data.StuffDataFactory (class_1697)
public static class StuffDataFactory
{
    public static IStuffData? GetStuffDataWrapperForType(int type)
    {
        int formatKey = type & 0xFF;
        IStuffData? stuffData = formatKey switch
        {
            StuffDataFormatKeyEnum.LEGACY_TYPE_KEY => new LegacyStuffData(),
            StuffDataFormatKeyEnum.MAP_TYPE_KEY => new MapStuffData(),
            StuffDataFormatKeyEnum.STRING_ARRAY_TYPE_KEY => new StringArrayStuffData(),
            StuffDataFormatKeyEnum.VOTE_RESULT_TYPE_KEY => new VoteResultStuffData(),
            StuffDataFormatKeyEnum.EMPTY_TYPE_KEY => new EmptyStuffData(),
            StuffDataFormatKeyEnum.INT_ARRAY_TYPE_KEY => new IntArrayStuffData(),
            StuffDataFormatKeyEnum.HIGH_SCORE_TYPE_KEY => new HighScoreStuffData(),
            StuffDataFormatKeyEnum.CRACKABLE_TYPE_KEY => new CrackableStuffData(),
            _ => null,
        };

        if (stuffData != null)
        {
            stuffData.Flags = type & 0xFF00;
        }

        return stuffData;
    }
}
