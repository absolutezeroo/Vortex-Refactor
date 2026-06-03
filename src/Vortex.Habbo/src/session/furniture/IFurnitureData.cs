// @see com.sulake.habbo.session.furniture.IFurnitureData

namespace Vortex.Habbo.Session.Furniture;

/// @see com.sulake.habbo.session.furniture.IFurnitureData
public interface IFurnitureData
{
    string type { get; }
    int id { get; }
    string className { get; }
    string fullName { get; }
    bool hasIndexedColor { get; }
    int colourIndex { get; }
    int revision { get; }
    int tileSizeX { get; }
    int tileSizeY { get; }
    int tileSizeZ { get; }
    int[]? colours { get; }
    string localizedName { get; }
    string description { get; }
    string adUrl { get; }
    int purchaseOfferId { get; }
    int rentOfferId { get; }
    string? customParams { get; }
    int category { get; }
    bool purchaseCouldBeUsedForBuyout { get; }
    bool rentCouldBeUsedForBuyout { get; }
    bool availableForBuildersClub { get; }
    bool canStandOn { get; }
    bool canSitOn { get; }
    bool canLayOn { get; }
    bool isExternalImageType { get; }
    bool excludedFromDynamic { get; }
    string furniLine { get; }
}
