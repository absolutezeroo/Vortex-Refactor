// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/ColouredButton.as

using System;

using Godot;

namespace Vortex.OnBoardingHcUi;

/// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/ColouredButton.as
public partial class ColouredButton : Button
{
    public const string BUTTON_RED = "red";
    public const string BUTTON_GREEN = "gfreen";
    public const string BUTTON_YELLOW = "yellow";

    private readonly string _defaultBackgroundPath;
    private readonly string _pressedBackgroundPath;
    private readonly string _inactiveBackgroundPath;
    private readonly string _rolloverBackgroundPath;
    private readonly Texture2D? _iconTexture;

    public ColouredButton()
        : base()
    {
        _defaultBackgroundPath = "res://assets/images/button_green.png";
        _pressedBackgroundPath = "res://assets/images/button_green_pressed.png";
        _inactiveBackgroundPath = "res://assets/images/button_green_inactive.png";
        _rolloverBackgroundPath = "res://assets/images/button_green_rollover.png";
    }

    public ColouredButton
    (
        string color,
        string text,
        Rect2 rect,
        bool fitWidthToText,
        Action<Button>? action,
        uint glowColour = 0xFFFFFF
    )
        : base(text, rect, fitWidthToText, action, glowColour)
    {
        (_defaultBackgroundPath, _pressedBackgroundPath, _inactiveBackgroundPath, _rolloverBackgroundPath, _iconTexture) = ResolveStyle(color);
    }

    protected override string DefaultBackgroundPath => _defaultBackgroundPath;
    protected override Rect2 DefaultBackgroundScale9 => new(8, 10, 6, 4);

    protected override string PressedBackgroundPath => _pressedBackgroundPath;
    protected override Rect2 PressedBackgroundScale9 => new(8, 10, 6, 4);

    protected override string InactiveBackgroundPath => _inactiveBackgroundPath;
    protected override Rect2 InactiveBackgroundScale9 => new(8, 10, 6, 4);

    protected override string CurrentBackgroundPath => _defaultBackgroundPath;
    protected override Rect2 CurrentBackgroundScale9 => new(8, 10, 6, 4);

    protected override string? RolloverBackgroundPath => _rolloverBackgroundPath;
    protected override Rect2 RolloverBackgroundScale9 => new(8, 10, 6, 4);

    protected override bool Etching => false;
    protected override int Padding => 64;
    protected override uint TextColour => 0xFFFFFF;
    protected override Texture2D? IconTexture => _iconTexture;

    private static (string defaultPath, string pressedPath, string inactivePath, string rolloverPath, Texture2D? iconTexture) ResolveStyle
    (
        string color
    )
    {
        return color switch
        {
            BUTTON_RED => (
                "res://assets/images/button_red.png",
                "res://assets/images/button_red_pressed.png",
                "res://assets/images/button_red_inactive.png",
                "res://assets/images/button_red_rollover.png",
                null),
            BUTTON_YELLOW => (
                "res://assets/images/button_yellow.png",
                "res://assets/images/button_yellow_pressed.png",
                "res://assets/images/button_yellow_inactive.png",
                "res://assets/images/button_yellow_rollover.png",
                ResourceLoader.Exists("res://assets/images/icon_hc.png")
                    ? GD.Load<Texture2D>("res://assets/images/icon_hc.png")
                    : null),
            _ => (
                "res://assets/images/button_green.png",
                "res://assets/images/button_green_pressed.png",
                "res://assets/images/button_green_inactive.png",
                "res://assets/images/button_green_rollover.png",
                null),
        };
    }
}
