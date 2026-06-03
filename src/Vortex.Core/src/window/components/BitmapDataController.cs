// @see core/window/components/BitmapDataController.as

using System;

using Godot;

using Vortex.Core.Window.Enum;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Theme;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/BitmapDataController.as
/// Godot adaptation: AS3 BitmapData → Godot Image
public class BitmapDataController : WindowController, IBitmapDataContainer
{
    protected Image? _bitmapData;
    private Vector2 _etchingPointValue = new(0, -1);

    /// @see BitmapDataController.as::BitmapDataController (default)
    public BitmapDataController() : base() { }

    /// @see BitmapDataController.as::BitmapDataController (name + rect)
    public BitmapDataController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see BitmapDataController.as::BitmapDataController (full AS3 11-param signature)
    public BitmapDataController
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
        // @see BitmapDataController.as — read property defaults from theme manager
        try
        {
            IWindowFactory? factory = param5.GetWindowFactory();

            IThemeManager? themeManager = factory?.GetThemeManager();
            IPropertyMap? propDefaults = themeManager?.GetPropertyDefaults(param3);

            if (propDefaults != null)
            {
                PropertyStruct? pivotProp = propDefaults.GetValue("pivot_point");

                if (pivotProp?.value is string pivotName)
                {
                    PivotPointValue = PivotPoint.PivotFromName(pivotName);
                }

                if (propDefaults.GetValue("stretched_x")?.value is bool sx)
                {
                    StretchedX = sx;
                }

                if (propDefaults.GetValue("stretched_y")?.value is bool sy)
                {
                    StretchedY = sy;
                }

                if (propDefaults.GetValue("zoom_x")?.value != null)
                {
                    ZoomX = Convert.ToSingle(propDefaults.GetValue("zoom_x")!.value);
                }

                if (propDefaults.GetValue("zoom_y")?.value != null)
                {
                    ZoomY = Convert.ToSingle(propDefaults.GetValue("zoom_y")!.value);
                }

                if (propDefaults.GetValue("wrap_x")?.value is bool wx)
                {
                    WrapX = wx;
                }

                if (propDefaults.GetValue("wrap_y")?.value is bool wy)
                {
                    WrapY = wy;
                }

                if (propDefaults.GetValue("rotation")?.value != null)
                {
                    RotationValue = Convert.ToSingle(propDefaults.GetValue("rotation")!.value);
                }
            }
        }
        catch
        {
            // Fallback: keep defaults
        }
    }

    /// @see BitmapDataController.as::get bitmapData
    public Image? BitmapImage => _bitmapData;

    /// @see BitmapDataController.as::get pivotPoint
    public uint PivotPointValue { get; set; }

    /// @see BitmapDataController.as::get stretchedX
    public bool StretchedX { get; set; }

    /// @see BitmapDataController.as::get stretchedY
    public bool StretchedY { get; set; }

    /// @see BitmapDataController.as::get zoomX
    public float ZoomX { get; set; } = 1f;

    /// @see BitmapDataController.as::get zoomY
    public float ZoomY { get; set; } = 1f;

    /// @see BitmapDataController.as::get greyscale
    public bool Greyscale { get; set; }

    /// @see BitmapDataController.as::get etchingColor
    public uint EtchingColorValue { get; set; }

    /// @see BitmapDataController.as::get etchingPoint
    public override Vector2 etchingPoint => _etchingPointValue;

    /// @see BitmapDataController.as::set etching (line 158-162)
    public override void SetEtching(uint color, float px, float py)
    {
        EtchingColorValue = color;
        _etchingPointValue = new Vector2(px, py);
    }

    /// @see BitmapDataController.as::set etching — convenience overload
    public void SetEtching(int x, int y)
    {
        _etchingPointValue = new Vector2(x, y);
    }

    /// @see BitmapDataController.as::get fitSizeToContents
    public bool FitSizeToContents { get; set; }

    /// @see BitmapDataController.as::get wrapX
    public bool WrapX { get; set; }

    /// @see BitmapDataController.as::get wrapY
    public bool WrapY { get; set; }

    /// @see BitmapDataController.as::get rotation
    public float RotationValue { get; set; }

    /// @see BitmapDataController.as::fitSize
    public void FitSize()
    {
        if (!FitSizeToContents || _bitmapData == null)
        {
            return;
        }

        float newWidth = _bitmapData.GetWidth() * ZoomX;
        float newHeight = _bitmapData.GetHeight() * ZoomY;

        if ((int)newWidth == (int)width && (int)newHeight == (int)height)
        {
            return;
        }

        width = newWidth;
        height = newHeight;
    }

    /// @see BitmapDataController.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        foreach (PropertyStruct prop in properties)
        {
            switch (prop.key)
            {
                case "pivot_point":
                    if (prop.value is string pivotName)
                    {
                        PivotPointValue = PivotPoint.PivotFromName(pivotName);
                    }
                    else if (prop.value != null)
                    {
                        PivotPointValue = Convert.ToUInt32(prop.value);
                    }
                    break;
                case "stretched_x":
                    if (prop.value is bool sx)
                    {
                        StretchedX = sx;
                    }
                    break;
                case "stretched_y":
                    if (prop.value is bool sy)
                    {
                        StretchedY = sy;
                    }
                    break;
                case "zoom":
                    if (prop.value != null)
                    {
                        float z = Convert.ToSingle(prop.value);
                        ZoomX = z;
                        ZoomY = z;
                    }
                    break;
                case "zoom_x":
                    if (prop.value != null)
                    {
                        ZoomX = Convert.ToSingle(prop.value);
                    }
                    break;
                case "zoom_y":
                    if (prop.value != null)
                    {
                        ZoomY = Convert.ToSingle(prop.value);
                    }
                    break;
                case "wrap_x":
                    if (prop.value is bool wx)
                    {
                        WrapX = wx;
                    }
                    break;
                case "wrap_y":
                    if (prop.value is bool wy)
                    {
                        WrapY = wy;
                    }
                    break;
                case "fit_size_to_contents":
                    if (prop.value is bool fstc)
                    {
                        FitSizeToContents = fstc;
                    }
                    break;
                case "rotation":
                    if (prop.value != null)
                    {
                        RotationValue = Convert.ToSingle(prop.value);
                    }
                    break;
                case "greyscale":
                    if (prop.value is bool gs)
                    {
                        Greyscale = gs;
                    }
                    break;
            }
        }

        base.ApplyProperties(properties);
    }

    /// @see BitmapDataController.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        _bitmapData = null;

        return base.Destroy();
    }

    // IBitmapDataContainer — explicit implementations for mismatched names
    Image? IBitmapDataContainer.BitmapData => _bitmapData;
    uint IBitmapDataContainer.PivotPoint { get => PivotPointValue; set => PivotPointValue = value; }
    uint IBitmapDataContainer.EtchingColor { get => EtchingColorValue; set => EtchingColorValue = value; }
    Vector2 IBitmapDataContainer.EtchingPoint => _etchingPointValue;
    float IBitmapDataContainer.Rotation { get => RotationValue; set => RotationValue = value; }
}
