// @see habbo/window/utils/class_3723.as

using Godot;

namespace Vortex.Habbo.Window.Utils;

/// <summary>
/// Generates bitmap text for serial/supply numbers on limited items
/// using per-digit glyph assets.
/// </summary>
/// @see habbo/window/utils/class_3723.as
public static class LimitedItemOverlayNumberBitmapGenerator
{
    private const string GLYPH_ASSET_PREFIX = "unique_item_number_glyph_";

    /// @see class_3723.as::createBitmap
    /// <summary>
    /// Creates a bitmap image containing the given number rendered with glyph assets.
    /// Digits are centered horizontally within the given dimensions.
    /// </summary>
    /// <param name="assetLibrary">Asset library containing glyph bitmaps.</param>
    /// <param name="number">The number to render (0-9999).</param>
    /// <param name="width">Output bitmap width.</param>
    /// <param name="height">Output bitmap height.</param>
    public static Image? CreateBitmap(object? assetLibrary, int number, int width, int height)
    {
        // TODO(assets): Wire glyph asset loading from IAssetLibrary when asset system is ported.
        // AS3 decomposes number into digits (units/tens/hundreds/thousands),
        // loads "unique_item_number_glyph_N" assets for each significant digit,
        // calculates total width, centers horizontally, and composites via copyPixels.

        int units = number % 10;
        int tens = number / 10 % 10;
        int hundreds = number / 100 % 10;
        int thousands = number / 1000 % 10;

        List<int> digits = new();

        if (thousands > 0)
        {
            digits.Add(thousands);
        }

        if (thousands > 0 || hundreds > 0)
        {
            digits.Add(hundreds);
        }

        if (thousands > 0 || hundreds > 0 || tens > 0)
        {
            digits.Add(tens);
        }

        digits.Add(units);

        // Asset names would be: GLYPH_ASSET_PREFIX + digit
        // Each glyph is composited left-to-right, centered in the output.

        return null; // Stub until asset system provides glyph bitmaps
    }
}
