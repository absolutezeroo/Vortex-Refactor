// @see WIN63-202111081545-75921380-Source-main/src/login/WebCaptchaView.as

using Godot;

using Vortex.Habbo.Communication.Login;

namespace Vortex.Login;

/// @see WIN63-202111081545-75921380-Source-main/src/login/WebCaptchaView.as
public partial class WebCaptchaView(ICaptchaListener listener) : Control, ICaptchaView
{
    public new void Dispose()
    {
        GetParent()?.RemoveChild(this);

        QueueFree();
    }

    public override void _Ready()
    {
        base._Ready();

        PanelContainer panel = new()
        {
            Name = "CaptchaPanel",
            CustomMinimumSize = new Vector2(520, 120),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
        };

        panel.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
        panel.OffsetLeft = -260;
        panel.OffsetTop = -60;
        panel.OffsetRight = 260;
        panel.OffsetBottom = 60;
        panel.AddThemeStyleboxOverride(
            "panel", new StyleBoxFlat
            {
                BgColor = new Color(0.08f, 0.11f, 0.17f, 0.92f),
                BorderWidthLeft = 2,
                BorderWidthTop = 2,
                BorderWidthRight = 2,
                BorderWidthBottom = 2,
                BorderColor = new Color(0.31f, 0.45f, 0.6f),
                CornerRadiusTopLeft = 6,
                CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6,
                CornerRadiusBottomRight = 6,
            }
        );
        AddChild(panel);

        Label text = new()
        {
            Text = "Captcha webview not yet available in Godot runtime.\nPress Enter to continue without captcha token.",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        panel.AddChild(text);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false, Keycode: Key.Enter })
        {
            return;
        }

        listener.HandleCaptchaResult(string.Empty);

        Dispose();
    }
}
