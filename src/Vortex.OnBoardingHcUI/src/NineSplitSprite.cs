// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/NineSplitSprite.as

using System;

using Godot;

namespace Vortex.OnBoardingHcUi;

/// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/NineSplitSprite.as
public sealed class NineSplitSprite
(
    string texturePath,
    int left,
    int centerWidth,
    int rightHint,
    int top,
    int centerHeight,
    int bottomHint
)
{
    public static readonly NineSplitSprite BALLOON_HIGHLIGHTED = new("res://assets/images/white_balloon.png", 5, 4, 5, 11, 1, 5);
    public static readonly NineSplitSprite BALLOON_SHADED = new("res://assets/images/white_balloon.png", 5, 4, 5, 5, 1, 11);
    public static readonly NineSplitSprite BORDER_SUNK = new("res://assets/images/border_sunk.png", 12, 2, 6, 14, 2, 4);
    public static readonly NineSplitSprite DARK_POPUP = new("res://assets/images/dark_popup.png", 5, 5, 5, 5, 12, 5);
    public static readonly NineSplitSprite DIVIDER = new("res://assets/images/divider.png", 2, 2, 2, 8, 0, 0);
    public static readonly NineSplitSprite FRAME = new("res://assets/images/frame.png", 4, 3, 4, 5, 1, 7);
    public static readonly NineSplitSprite INPUT_CORRECTED = new("res://assets/images/input_corrected.png", 5, 2, 5, 5, 2, 6);
    public static readonly NineSplitSprite INPUT_ERROR = new("res://assets/images/input_error.png", 5, 2, 5, 5, 2, 6);
    public static readonly NineSplitSprite INPUT_FIELD = new("res://assets/images/input_field.png", 5, 4, 5, 7, 2, 5);
    public static readonly NineSplitSprite INPUT_CORRECTED_HITCH = new("res://assets/images/input_corrected_hitch.png", 10, 310, 10, 5, 21, 5);
    public static readonly NineSplitSprite INPUT_ERROR_HITCH = new("res://assets/images/input_error_hitch.png", 10, 310, 10, 5, 21, 5);
    public static readonly NineSplitSprite INPUT_FIELD_HITCH = new("res://assets/images/input_field_hitch.png", 10, 310, 10, 5, 21, 5);
    public static readonly NineSplitSprite DARK_BALLOON = new("res://assets/images/block_dark_base.png", 5, 4, 5, 11, 1, 5);

    public Texture2D? LoadTexture()
    {
        return !ResourceLoader.Exists(texturePath) ? null : GD.Load<Texture2D>(texturePath);
    }

    public StyleBoxTexture? CreateStyleBox
    (
        float contentMarginLeft = 0,
        float contentMarginTop = 0,
        float contentMarginRight = 0,
        float contentMarginBottom = 0
    )
    {
        Texture2D? texture = LoadTexture();

        if (texture == null)
        {
            return null;
        }

        int width = texture.GetWidth();
        int height = texture.GetHeight();

        float right = Math.Max(0, width - (left + centerWidth));
        if (right <= 0)
        {
            right = rightHint;
        }

        float bottom = Math.Max(0, height - (top + centerHeight));

        if (bottom <= 0)
        {
            bottom = bottomHint;
        }

        return new StyleBoxTexture
        {
            Texture = texture,
            TextureMarginLeft = left,
            TextureMarginTop = top,
            TextureMarginRight = right,
            TextureMarginBottom = bottom,
            ContentMarginLeft = contentMarginLeft,
            ContentMarginTop = contentMarginTop,
            ContentMarginRight = contentMarginRight,
            ContentMarginBottom = contentMarginBottom,
            DrawCenter = true,
        };
    }

    public NinePatchRect Render(int width, int height)
    {
        NinePatchRect panel = new()
        {
            Name = "NineSplit",
            Texture = LoadTexture(),
            CustomMinimumSize = new Vector2(width, height),
            Size = new Vector2(width, height),
            DrawCenter = true,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        StyleBoxTexture? style = CreateStyleBox();

        if (style == null)
        {
            return panel;
        }

        panel.PatchMarginLeft = (int)style.TextureMarginLeft;
        panel.PatchMarginTop = (int)style.TextureMarginTop;
        panel.PatchMarginRight = (int)style.TextureMarginRight;
        panel.PatchMarginBottom = (int)style.TextureMarginBottom;

        return panel;
    }
}
