// @see core/window/components/BitmapWrapperController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/BitmapWrapperController.as
/// Godot adaptation: AS3 BitmapData → Godot Image
public class BitmapWrapperController : BitmapDataController, IBitmapWrapperWindow
{
    private string _bitmapAssetName = "";

    /// @see BitmapWrapperController.as::BitmapWrapperController (default)
    public BitmapWrapperController() : base() { }

    /// @see BitmapWrapperController.as::BitmapWrapperController (name + rect)
    public BitmapWrapperController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see BitmapWrapperController.as::BitmapWrapperController (full AS3 11-param signature)
    public BitmapWrapperController
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
        // @see BitmapWrapperController.as — read handle_bitmap_disposing from theme defaults
        // AS3 casts the PropertyStruct itself to Boolean (truthy if non-null = property exists)
        try
        {
            IWindowFactory? factory = param5.GetWindowFactory();
            Theme.IPropertyMap? propDefaults = factory?.GetThemeManager()?.GetPropertyDefaults(param3);

            if (propDefaults?.GetValue("handle_bitmap_disposing") != null)
            {
                DisposesBitmap = true;
            }
        }
        catch
        {
            // Fallback: keep default (false)
        }
    }

    /// @see BitmapWrapperController.as::get bitmap / set bitmap
    public Image? Bitmap
    {
        get => _bitmapData;
        set
        {
            // @see BitmapWrapperController.as — dispose old if handling disposal
            if (DisposesBitmap && _bitmapData != null && _bitmapData != value)
            {
                _bitmapData = null;
            }

            _bitmapData = value;
            FitSize();

            // @see BitmapWrapperController.as — invalidate for redraw
            _context?.Invalidate(this, new Rect2(0, 0, width, height), Class3655.REDRAW);
        }
    }

    /// @see BitmapWrapperController.as::get bitmapAssetName
    public string BitmapAssetName
    {
        get => _bitmapAssetName;
        set => _bitmapAssetName = value ?? "";
    }

    /// @see BitmapWrapperController.as::get disposesBitmap
    public bool DisposesBitmap { get; set; }

    /// @see BitmapWrapperController.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        foreach (PropertyStruct prop in properties)
        {
            switch (prop.key)
            {
                case "handle_bitmap_disposing":
                    if (prop.value is bool hbd)
                    {
                        DisposesBitmap = hbd;
                    }
                    break;
                case "asset_name":
                    if (prop.value is string name)
                    {
                        _bitmapAssetName = name;
                    }
                    break;
            }
        }

        base.ApplyProperties(properties);
    }

    /// @see BitmapWrapperController.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        if (_bitmapData != null && DisposesBitmap)
        {
            _bitmapData = null;
        }

        return base.Destroy();
    }

}
