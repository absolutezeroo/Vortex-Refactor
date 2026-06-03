// @see core/window/components/StaticBitmapWrapperController.as

using System;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/StaticBitmapWrapperController.as
/// Godot adaptation: AS3 BitmapData → Godot Image
public class StaticBitmapWrapperController : BitmapDataController, IStaticBitmapWrapperWindow, IAssetReceiver
{
    private string _assetUri = "";
    private bool _createdLocally;

    /// @see StaticBitmapWrapperController.as::StaticBitmapWrapperController (default)
    public StaticBitmapWrapperController() : base() { }

    /// @see StaticBitmapWrapperController.as::StaticBitmapWrapperController (name + rect)
    public StaticBitmapWrapperController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see StaticBitmapWrapperController.as::StaticBitmapWrapperController (full AS3 11-param signature)
    public StaticBitmapWrapperController
    (
        string param1,
        uint param2,
        uint param3,
        uint param4,
        IWindowContext param5,
        Rect2 param6,
        IWindow? param7,
        Action<WindowEvent, IWindow>? param8 = null,
        IList<object>? param9 = null,
        IList<string>? param10 = null,
        uint param11 = 0, string param12 = ""
    ) : base(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12)
    {
    }

    /// @see StaticBitmapWrapperController.as::get assetUri / set assetUri
    public string AssetUriValue
    {
        get => _assetUri;
        set
        {
            if (value == _assetUri)
            {
                return;
            }

            _assetUri = value;

            if (string.IsNullOrEmpty(value))
            {
                // @see StaticBitmapWrapperController.as — clear bitmap on empty URI
                if (_createdLocally)
                {
                    _bitmapData = null;
                }

                _bitmapData = null;
                _createdLocally = false;
                _assetUri = "";

                _context?.Invalidate(this, new Rect2(0, 0, width, height), Class3655.REDRAW);
                return;
            }

            // @see StaticBitmapWrapperController.as — request asset from resource manager
            _context?.GetResourceManager()?.RetrieveAsset(_assetUri, this);
        }
    }

    /// @see StaticBitmapWrapperController.as::receiveAsset
    public void ReceiveAsset(IAsset asset, string resolvedName)
    {
        if (_disposed || _context == null)
        {
            return;
        }

        // @see StaticBitmapWrapperController.as — verify asset still matches current URI
        if (!_context.GetResourceManager()!.IsSameAsset(_assetUri, resolvedName))
        {
            return;
        }

        if (asset is not BitmapDataAsset bitmapAsset)
        {
            return;
        }

        if (bitmapAsset.Content is not Image bitmapData)
        {
            return;
        }

        if (_bitmapData != bitmapData)
        {
            if (_createdLocally)
            {
                _bitmapData = null;
            }

            _createdLocally = false;

            // @see StaticBitmapWrapperController.as — check if sub-region extraction needed
            Rect2? rect = bitmapAsset.Rectangle;
            if (rect.HasValue &&
                (bitmapData.GetWidth() != (int)rect.Value.Size.X ||
                 bitmapData.GetHeight() != (int)rect.Value.Size.Y))
            {
                // Extract sub-region — AS3: new BitmapData + copyPixels
                int rw = (int)rect.Value.Size.X;
                int rh = (int)rect.Value.Size.Y;
                Image? subImage = Image.CreateEmpty(rw, rh, false, Image.Format.Rgba8);
                subImage.BlitRect(
                    bitmapData,
                    new Rect2I((int)rect.Value.Position.X, (int)rect.Value.Position.Y, rw, rh),
                    Vector2I.Zero
                );
                _bitmapData = subImage;
                _createdLocally = true;
            }
            else
            {
                _bitmapData = bitmapData;
            }

            _context.Invalidate(this, new Rect2(0, 0, width, height), Class3655.REDRAW);
        }

        FitSize();
    }

    /// @see StaticBitmapWrapperController.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        foreach (PropertyStruct prop in properties)
        {
            if (prop is { key: "asset_uri", value: string uri })
            {
                AssetUriValue = uri;
            }
        }

        base.ApplyProperties(properties);
    }

    /// @see StaticBitmapWrapperController.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        if (_createdLocally)
        {
            _bitmapData = null;
        }

        _createdLocally = false;

        return base.Destroy();
    }

    string? IStaticBitmapWrapperWindow.AssetUri
    {
        get => AssetUriValue;
        set => AssetUriValue = value ?? "";
    }
}
