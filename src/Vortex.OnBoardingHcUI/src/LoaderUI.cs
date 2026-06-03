// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/LoaderUI.as

using System;

using Godot;

namespace Vortex.OnBoardingHcUi;

/// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/LoaderUI.as
public static class LoaderUI
{
    public const int STYLE_ILLUMINA = 1;
    public const int STYLE_HITCH = 2;
    public const string ANCHOR_LEFT = "l";
    public const string ANCHOR_CENTRE = "c";
    public const string ANCHOR_RIGHT = "r";
    public const string ANCHOR_TOP = "t";
    public const string ANCHOR_MIDDLE = "m";
    public const string ANCHOR_BOTTOM = "b";
    public const uint HITCH_TEXT_BODY_COLOUR = 8309486;
    public const uint HITCH_TEXT_HIGHLIGHT_COLOUR = 0xFFFFFF;

    public static Control CreateTextField
    (
        string text,
        int fontSize,
        uint textColor,
        bool bold = false,
        bool multiline = false,
        bool input = false,
        bool italic = false,
        string align = "left",
        bool kerning = false,
        bool underline = false
    )
    {
        if (input)
        {
            LineEdit lineEdit = new()
            {
                Text = LocalizedSprite.ResolveLocalizedText(text),
                Editable = true,
                SelectAllOnFocus = false,
                Alignment = ParseAlignment(align),
            };
            ApplyFont(lineEdit, fontSize, bold, italic, underline);
            lineEdit.AddThemeColorOverride("font_color", ToColor(textColor));
            lineEdit.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
            lineEdit.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());
            lineEdit.AddThemeStyleboxOverride("read_only", new StyleBoxEmpty());
            lineEdit.AddThemeStyleboxOverride("disabled", new StyleBoxEmpty());
            return lineEdit;
        }

        LocalizedTextField label = new()
        {
            HtmlText = text,
            AutowrapMode = multiline ? TextServer.AutowrapMode.WordSmart : TextServer.AutowrapMode.Off,
            HorizontalAlignment = ParseAlignment(align),
        };

        ApplyFont(label, fontSize, bold, italic, underline);
        label.AddThemeColorOverride("font_color", ToColor(textColor));
        label.Size = label.GetCombinedMinimumSize();

        return label;
    }

    public static void AddEtching(Control control, bool negative = false)
    {
        Color outline = negative ? new Color(0f, 0f, 0f, 0.7f) : new Color(0.82f, 0.83f, 0f, 0.85f);
        control.AddThemeColorOverride("font_outline_color", outline);
        control.AddThemeConstantOverride("outline_size", 1);
    }

    public static void LineUpHorizontally(Control anchor, params object[] args)
    {
        if (args.Length % 2 != 0)
        {
            return;
        }

        Control current = anchor;

        for (int i = 0;
             i < args.Length;
             i += 2)
        {
            if (args[i] is not int spacing || args[i + 1] is not Control next)
            {
                continue;
            }

            next.Position = new Vector2(current.Position.X + current.Size.X + spacing, next.Position.Y);
            current = next;
        }
    }

    public static void LineUpHorizontallyRevers(Control anchor, params object[] args)
    {
        if (args.Length % 2 != 0)
        {
            return;
        }

        Control current = anchor;

        for (int i = 0;
             i < args.Length;
             i += 2)
        {
            if (args[i] is not int spacing || args[i + 1] is not Control next)
            {
                continue;
            }

            next.Position = new Vector2(current.Position.X - spacing - next.Size.X, next.Position.Y);
            current = next;
        }
    }

    public static void LineUpVertically(Control anchor, params object[] args)
    {
        if (args.Length % 2 != 0)
        {
            return;
        }

        Control current = anchor;

        for (int i = 0;
             i < args.Length;
             i += 2)
        {
            if (args[i] is not int spacing || args[i + 1] is not Control next)
            {
                continue;
            }

            next.Position = new Vector2(next.Position.X, current.Position.Y + current.Size.Y + spacing);
            current = next;
        }
    }

    public static void LineUpVerticallyRevers(Control anchor, params object[] args)
    {
        if (args.Length % 2 != 0)
        {
            return;
        }

        Control current = anchor;

        for (int i = 0;
             i < args.Length;
             i += 2)
        {
            if (args[i] is not int spacing || args[i + 1] is not Control next)
            {
                continue;
            }

            next.Position = new Vector2(next.Position.X, current.Position.Y - spacing - next.Size.Y);
            current = next;
        }
    }

    public static void LineUpElementsVertically(IReadOnlyList<Control>? elements, int spacing)
    {
        if (elements == null || elements.Count < 2)
        {
            return;
        }

        Control current = elements[0];

        for (int i = 1;
             i < elements.Count;
             i++)
        {
            Control next = elements[i];
            next.Position = new Vector2(next.Position.X, current.Position.Y + current.Size.Y + spacing);
            current = next;
        }
    }

    public static void AlignAnchors(Control anchor, int margin, string mode, params Control[] controls)
    {
        foreach (Control control in controls)
        {
            Vector2 position = control.Position;

            if (mode.Contains(ANCHOR_LEFT, StringComparison.Ordinal))
            {
                position.X = anchor.Position.X + margin;
            }

            if (mode.Contains(ANCHOR_CENTRE, StringComparison.Ordinal))
            {
                position.X = anchor.Position.X + ((anchor.Size.X - control.Size.X) / 2f);
            }

            if (mode.Contains(ANCHOR_RIGHT, StringComparison.Ordinal))
            {
                position.X = anchor.Position.X + anchor.Size.X - control.Size.X - margin;
            }

            if (mode.Contains(ANCHOR_TOP, StringComparison.Ordinal))
            {
                position.Y = anchor.Position.Y + margin;
            }

            if (mode.Contains(ANCHOR_MIDDLE, StringComparison.Ordinal))
            {
                position.Y = anchor.Position.Y + ((anchor.Size.Y - control.Size.Y) / 2f);
            }

            if (mode.Contains(ANCHOR_BOTTOM, StringComparison.Ordinal))
            {
                position.Y = anchor.Position.Y + anchor.Size.Y - control.Size.Y - margin;
            }

            control.Position = position;
        }
    }

    public static Control CreateBalloon
    (
        int width,
        int height,
        int arrowOffset,
        bool _unused,
        uint color = 0xFFFFFF,
        string direction = "up"
    )
    {
        Control container = new()
        {
            CustomMinimumSize = new Vector2(width, height),
            Size = new Vector2(width, height),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        NinePatchRect balloon = NineSplitSprite.DARK_BALLOON.Render(width, height);
        balloon.Modulate = ToColor(color);
        container.AddChild(balloon);

        string? pointerPath = direction switch
        {
            "up" => "res://assets/images/block_dark_point_up.png",
            "down" => "res://assets/images/block_dark_point_down.png",
            "left" => "res://assets/images/block_dark_point_left.png",
            "right" => "res://assets/images/block_dark_point_right.png",
            _ => null,
        };

        if (string.IsNullOrWhiteSpace(pointerPath) || !ResourceLoader.Exists(pointerPath))
        {
            return container;
        }

        Texture2D? pointerTexture = GD.Load<Texture2D>(pointerPath);

        if (pointerTexture == null)
        {
            return container;
        }

        TextureRect pointer = new()
        {
            Texture = pointerTexture,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        arrowOffset = arrowOffset < 0 ? Math.Max(0, (width - pointerTexture.GetWidth()) / 2) : arrowOffset;

        pointer.Position = direction switch
        {
            "up" => new Vector2(arrowOffset, -pointerTexture.GetHeight()),
            "down" => new Vector2(arrowOffset, height),
            "left" => new Vector2(-pointerTexture.GetWidth(), arrowOffset),
            "right" => new Vector2(width, arrowOffset),
            _ => pointer.Position,
        };

        container.AddChild(pointer);

        return container;
    }

    public static Control CreateFrame(string caption, string subCaption, Rect2 rectangle, int style = STYLE_ILLUMINA)
    {
        Control root = new()
        {
            Position = rectangle.Position,
            Size = rectangle.Size,
            CustomMinimumSize = rectangle.Size,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        if (style == STYLE_ILLUMINA)
        {
            PanelContainer panel = new()
            {
                Name = "FramePanel",
                CustomMinimumSize = rectangle.Size,
                Size = rectangle.Size,
            };

            StyleBox frameStyle = (StyleBox?)NineSplitSprite.FRAME.CreateStyleBox() ?? new StyleBoxFlat();
            panel.AddThemeStyleboxOverride("panel", frameStyle);
            root.AddChild(panel);
        }

        uint titleColour = style == STYLE_HITCH ? HITCH_TEXT_BODY_COLOUR : HITCH_TEXT_HIGHLIGHT_COLOUR;
        int titleSize = style == STYLE_HITCH ? 24 : 40;

        if (!string.IsNullOrWhiteSpace(caption))
        {
            if (CreateTextField(caption, titleSize, titleColour) is Label title)
            {
                title.Position = new Vector2(0, -(titleSize + 8));
                if (style == STYLE_HITCH)
                {
                    title.Size = new Vector2(rectangle.Size.X, title.Size.Y);
                }

                root.AddChild(title);
            }
        }

        if (string.IsNullOrWhiteSpace(subCaption) || CreateTextField(subCaption, 10, 0xAAAAAA, true) is not Label subTitle)
        {
            return root;
        }

        subTitle.Position = new Vector2(8, -(titleSize + 16));
        root.AddChild(subTitle);

        return root;
    }

    public static void ResizeFrame(Control frame, int width, int height)
    {
        frame.Size = new Vector2(width, height);
        frame.CustomMinimumSize = new Vector2(width, height);

        PanelContainer? panel = frame.GetNodeOrNull<PanelContainer>("FramePanel");

        if (panel == null)
        {
            return;
        }

        panel.Size = new Vector2(width, height);
        panel.CustomMinimumSize = new Vector2(width, height);
    }

    public static StyleBoxTexture CreateScale9GridShapeFromImage(Texture2D image, Rect2 scale9Rect)
    {
        float right = Math.Max(0f, image.GetWidth() - (scale9Rect.Position.X + scale9Rect.Size.X));
        float bottom = Math.Max(0f, image.GetHeight() - (scale9Rect.Position.Y + scale9Rect.Size.Y));

        return new StyleBoxTexture
        {
            Texture = image,
            TextureMarginLeft = scale9Rect.Position.X,
            TextureMarginTop = scale9Rect.Position.Y,
            TextureMarginRight = right,
            TextureMarginBottom = bottom,
            DrawCenter = true,
        };
    }

    public static StyleBoxTexture? CreateScale9GridShapeFromPath(string texturePath, Rect2 scale9Rect)
    {
        if (!ResourceLoader.Exists(texturePath))
        {
            return null;
        }

        Texture2D? image = GD.Load<Texture2D>(texturePath);

        return image == null ? null : CreateScale9GridShapeFromImage(image, scale9Rect);
    }

    public static StyleBoxTexture? CreateTextBorder()
    {
        return CreateScale9GridShapeFromPath("res://assets/images/border_text_hitch.png", new Rect2(10, 10, 10, 10));
    }

    private static void ApplyFont(Control control, int fontSize, bool bold, bool italic, bool _underline)
    {
        string fontPath = (bold, italic) switch
        {
            (true, true) => "res://assets/fonts/Ubuntu-ib.ttf",
            (true, false) => "res://assets/fonts/Ubuntu-b.ttf",
            (false, true) => "res://assets/fonts/Ubuntu-i.ttf",
            _ => "res://assets/fonts/Ubuntu.ttf",
        };

        if (ResourceLoader.Exists(fontPath))
        {
            FontFile? font = GD.Load<FontFile>(fontPath);

            if (font != null)
            {
                control.AddThemeFontOverride("font", font);
            }
        }

        control.AddThemeFontSizeOverride("font_size", fontSize);
    }

    /// <summary>
    /// Delegates to <see cref="Vortex.Core.Utils.DisplayUtils.ComputeDescendantBounds"/>.
    /// </summary>
    public static Rect2 ComputeDescendantBounds(Node root)
    {
        return Core.Utils.DisplayUtils.ComputeDescendantBounds(root);
    }

    private static Color ToColor(uint value)
    {
        float r = ((value >> 16) & 0xFF) / 255f;
        float g = ((value >> 8) & 0xFF) / 255f;
        float b = (value & 0xFF) / 255f;

        return new Color(r, g, b, 1f);
    }

    private static HorizontalAlignment ParseAlignment(string align)
    {
        return align switch
        {
            "center" => HorizontalAlignment.Center,
            "right" => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Left,
        };
    }
}
