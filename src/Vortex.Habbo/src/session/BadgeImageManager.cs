// @see com.sulake.habbo.session.BadgeImageManager

using Godot;

using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.BadgeImageManager
public class BadgeImageManager
{
    public const string TYPE_GROUP = "group_badge";
    public const string TYPE_NORMAL = "normal_badge";

    private const string ASSET_PREFIX = "badge_";
    private const string ASSET_SMALL_POSTFIX = "_32";

    private readonly EventDispatcherWrapper _events;
    private readonly Dictionary<string, Image> _cache = new();
    private readonly HashSet<string> _pendingRequests = new();

    // TODO(assets): Replace with proper ICoreConfiguration access once available.
    private string _imageLibraryUrl = "";
    private string _groupBadgeUrl = "";

    public BadgeImageManager(EventDispatcherWrapper events)
    {
        _events = events;
    }

    public void Dispose()
    {
        _cache.Clear();
        _pendingRequests.Clear();
    }

    /// @see BadgeImageManager.as::configure — set URLs from configuration
    public void Configure(string imageLibraryUrl, string groupBadgeUrl)
    {
        _imageLibraryUrl = imageLibraryUrl;
        _groupBadgeUrl = groupBadgeUrl;
    }

    /// @see BadgeImageManager.as::getBadgeImage
    public Image? GetBadgeImage(string badgeId, string type = TYPE_NORMAL, bool usePlaceholder = true, bool small = false)
    {
        Image? image = GetBadgeImageInternal(badgeId, type, small);
        if (image == null && usePlaceholder)
        {
            // TODO(assets): Return placeholder image from asset library.
            return null;
        }
        return image;
    }

    /// @see BadgeImageManager.as::getSmallBadgeImage
    public Image? GetSmallBadgeImage(string badgeId, string type = TYPE_NORMAL)
    {
        Image? small = GetBadgeImageInternal(badgeId, type, true);
        if (small == null && GetBadgeImageInternal(badgeId, type, false) != null)
        {
            CreateSmallBadgeBitmap(badgeId);
        }
        return GetBadgeImage(badgeId, type, false, true);
    }

    /// @see BadgeImageManager.as::getBadgeImageWithInfo
    public BadgeInfo GetBadgeImageWithInfo(string badgeId)
    {
        Image? image = GetBadgeImageInternal(badgeId);
        return image != null
            ? new BadgeInfo(image, false)
            : new BadgeInfo(null, true);
    }

    /// @see BadgeImageManager.as::getBadgeImageAssetName
    public string? GetBadgeImageAssetName(string badgeId, string type = TYPE_NORMAL, bool small = false)
    {
        string key = ASSET_PREFIX + badgeId + (small ? ASSET_SMALL_POSTFIX : "");
        if (_cache.ContainsKey(key))
        {
            return key;
        }

        GetBadgeImageInternal(badgeId, type, small);
        return null;
    }

    /// @see BadgeImageManager.as::getSmallScaleBadgeAssetName
    public string? GetSmallScaleBadgeAssetName(string badgeId, string type = TYPE_NORMAL)
    {
        string? name = GetBadgeImageAssetName(badgeId, type, true);
        if (name == null)
        {
            CreateSmallBadgeBitmap(badgeId);
        }
        return GetBadgeImageAssetName(badgeId, type, true);
    }

    /// Called externally when an image finishes loading (e.g. from HttpRequest callback).
    public void OnImageLoaded(string badgeId, Image image)
    {
        string key = ASSET_PREFIX + badgeId;
        _cache[key] = image;
        _pendingRequests.Remove(key);
        _events.DispatchEvent(new BadgeImageReadyEvent(badgeId, image));
    }

    // --- Private helpers ---

    private Image? GetBadgeImageInternal(string badgeId, string type = TYPE_NORMAL, bool small = false)
    {
        string key = ASSET_PREFIX + badgeId + (small ? ASSET_SMALL_POSTFIX : "");
        if (_cache.TryGetValue(key, out Image? cached))
        {
            return cached.Duplicate() as Image;
        }

        if (small)
        {
            return null;
        }

        RequestBadgeLoad(badgeId, type, key);
        return null;
    }

    /// @see BadgeImageManager.as::getBadgeImageInternal — HTTP fetch branch
    /// TODO(assets): Implement Godot HttpRequest fetch and call OnImageLoaded on success.
    private void RequestBadgeLoad(string badgeId, string type, string key)
    {
        if (_pendingRequests.Contains(key))
        {
            return;
        }

        _pendingRequests.Add(key);

        string url = type switch
        {
            TYPE_NORMAL => _imageLibraryUrl + "album1584/" + badgeId + ".png",
            TYPE_GROUP => _groupBadgeUrl.Replace("%imagerdata%", badgeId),
            _ => "",
        };

        if (!string.IsNullOrEmpty(url))
        {
            // TODO(assets): Initiate Godot HttpRequest to url; on success call OnImageLoaded(badgeId, image).
            _ = url;
        }
    }

    /// @see BadgeImageManager.as::createSmallBadgeBitmap
    private void CreateSmallBadgeBitmap(string badgeId)
    {
        string key = ASSET_PREFIX + badgeId;
        if (!_cache.TryGetValue(key, out Image? src))
        {
            return;
        }

        Image small = new();
        small.CopyFrom(src);
        small.Resize(src.GetWidth() / 2, src.GetHeight() / 2, Image.Interpolation.Bilinear);
        _cache[ASSET_PREFIX + badgeId + ASSET_SMALL_POSTFIX] = small;
    }
}
