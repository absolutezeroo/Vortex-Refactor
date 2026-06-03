// @see com.sulake.habbo.session.furniture.FurnitureData

namespace Vortex.Habbo.Session.Furniture;

/// @see com.sulake.habbo.session.furniture.FurnitureData
public class FurnitureData : IFurnitureData
{
    /// @see FurnitureData.as::FurnitureData
    public FurnitureData(
        string type, int id, string className, string baseName,
        string localizedName, string description, int revision,
        int tileSizeX, int tileSizeY, int tileSizeZ,
        int[]? colours, bool hasIndexedColor, int colourIndex,
        string adUrl, int purchaseOfferId, bool purchaseCouldBeUsedForBuyout,
        int rentOfferId, bool rentCouldBeUsedForBuyout,
        bool availableForBuildersClub, string? customParams,
        int category, bool canStandOn, bool canSitOn, bool canLayOn,
        bool excludedFromDynamic, string furniLine)
    {
        this.type = type;
        this.id = id;
        fullName = className;
        this.className = baseName;
        this.localizedName = localizedName;
        this.description = description;
        this.revision = revision;
        this.tileSizeX = tileSizeX;
        this.tileSizeY = tileSizeY;
        this.tileSizeZ = tileSizeZ;
        this.colours = colours;
        this.hasIndexedColor = hasIndexedColor;
        this.colourIndex = colourIndex;
        this.adUrl = adUrl;
        this.purchaseOfferId = purchaseOfferId;
        this.purchaseCouldBeUsedForBuyout = purchaseCouldBeUsedForBuyout;
        this.rentOfferId = rentOfferId;
        this.rentCouldBeUsedForBuyout = rentCouldBeUsedForBuyout;
        this.availableForBuildersClub = availableForBuildersClub;
        this.customParams = customParams;
        this.category = category;
        this.canStandOn = canStandOn;
        this.canSitOn = canSitOn;
        this.canLayOn = canLayOn;
        this.excludedFromDynamic = excludedFromDynamic;
        this.furniLine = furniLine;
        // specialtype == 2 means external image furniture in AS3
        isExternalImageType = category == 2;
    }

    public string type { get; }
    public int id { get; }
    public string className { get; }
    public string fullName { get; }
    public bool hasIndexedColor { get; }
    public int colourIndex { get; }
    public int revision { get; }
    public int tileSizeX { get; }
    public int tileSizeY { get; }
    public int tileSizeZ { get; }
    public int[]? colours { get; }
    public string localizedName { get; }
    public string description { get; }
    public string adUrl { get; }
    public int purchaseOfferId { get; }
    public int rentOfferId { get; }
    public string? customParams { get; }
    public int category { get; }
    public bool purchaseCouldBeUsedForBuyout { get; }
    public bool rentCouldBeUsedForBuyout { get; }
    public bool availableForBuildersClub { get; }
    public bool canStandOn { get; }
    public bool canSitOn { get; }
    public bool canLayOn { get; }
    public bool isExternalImageType { get; }
    public bool excludedFromDynamic { get; }
    public string furniLine { get; }
}
