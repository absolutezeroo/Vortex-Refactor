using System;
using System.Text.Json;

using Godot;

using Vortex.Room.Object;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureExternalImageVisualization
public class FurnitureExternalImageVisualization : FurnitureRoomBrandingVisualization
{
    private bool _useExtraData;
    private string? _baseUrl;
    private string? _extraDataUrl;
    private string? _resolvedUrl;
    private bool _urlRequested;
    private string _urlPrefix = "";
    private string? _externalImageUuid;

    public override void SetExternalBaseUrls(string baseUrl, string extraDataUrl, bool useExtraData)
    {
        _baseUrl = baseUrl;
        _extraDataUrl = extraDataUrl;
        _useExtraData = useExtraData;
    }

    public override void Dispose()
    {
        // TODO: ExtraDataManager.FurnitureDisposed(this) when ExtraDataManager is ported
        base.Dispose();
    }

    public string? GetExternalImageUuid()
    {
        return _externalImageUuid;
    }

    public void OnUrlFromExtraDataService(string url)
    {
        _resolvedUrl = BuildThumbnailUrl(url, _urlPrefix);
    }

    public string? GetExtraDataUrl()
    {
        return _extraDataUrl;
    }

    protected override string? GetThumbnailUrl()
    {
        IRoomObject? obj = Object;

        if (obj == null || _baseUrl == "disabled" || _resolvedUrl == "REJECTED")
        {
            return null;
        }

        if (_resolvedUrl != null)
        {
            return _resolvedUrl;
        }

        string? furnitureData = obj.Model.GetString("furniture_data");

        if (furnitureData == null)
        {
            return null;
        }

        try
        {
            if (obj.Type.Contains("external_image_wallitem_poster"))
            {
                _urlPrefix = "";
            }
            else
            {
                _urlPrefix = "postcards/selfie/";
            }

            string? id = GetJsonValue(furnitureData, "id", null);

            if (!string.IsNullOrEmpty(id))
            {
                if (!_urlRequested)
                {
                    _externalImageUuid = id;
                    _urlRequested = true;

                    if (_useExtraData)
                    {
                        // TODO: ExtraDataManager.RequestExtraDataUrl(this) when ExtraDataManager is ported
                    }
                    else
                    {
                        // TODO: HTTP loading of extra data when networking is ported
                        Logger.Debug("loadExtraData deferred: " + _extraDataUrl + id);
                    }
                }

                return null;
            }

            string? url = GetJsonValue(furnitureData, "w", "url");
            url = BuildThumbnailUrl(url, _urlPrefix);
            _resolvedUrl = url;
            return url;
        }
        catch (Exception)
        {
            return null;
        }
    }

    protected override string? GetLibraryAssetNameForSprite(IGraphicAsset asset, IRoomObjectSprite sprite)
    {
        return _resolvedUrl;
    }

    private string? BuildThumbnailUrl(string? url, string prefix)
    {
        if (url == null || url == "REJECTED")
        {
            return url;
        }

        if (!url.StartsWith("http", StringComparison.Ordinal))
        {
            url = _baseUrl + prefix + url;
        }

        url = url.Replace(".png", "_small.png");

        if (!url.Contains(".png"))
        {
            url += "_small.png";
        }

        return url;
    }

    private static string? GetJsonValue(string json, string key, string? fallbackKey)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty(key, out JsonElement value))
            {
                return value.ToString();
            }

            if (fallbackKey != null && root.TryGetProperty(fallbackKey, out JsonElement fallback))
            {
                return fallback.ToString();
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }
}
