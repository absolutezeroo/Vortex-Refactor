// @see WIN63-202111081545-75921380-Source-main/src/login/Background.as

using Godot;

namespace Vortex.Login;

/// @see WIN63-202111081545-75921380-Source-main/src/login/Background.as
public partial class Background : Control
{
    private TextureRect? _gradientRect;
    private TextureRect? _tiles;

    public Background()
    {
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public bool Disposed { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        OnAddedToStage();
    }

    public override void _ExitTree()
    {
        Disposed = true;
        base._ExitTree();
    }

    public void DisposeBackground()
    {
        if (Disposed)
        {
            return;
        }

        Disposed = true;

        while (GetChildCount() > 0)
        {
            Node? child = GetChild(0);
            RemoveChild(child);
            child.QueueFree();
        }
    }

    public void Resize()
    {
        if (!IsInsideTree())
        {
            return;
        }

        Vector2 size = GetViewportRect().Size;
        Size = size;
        CustomMinimumSize = size;

        _gradientRect?.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _tiles?.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/login/Background.as::onAddedToStage
    private void OnAddedToStage()
    {
        // AS3: vertical linear gradient [0x0C5A7F, 0x0C3A65] ratios [127, 255]
        // Top half is solid 0x0C5A7F, bottom half blends to 0x0C3A65.
        if (_gradientRect == null)
        {
            Gradient gradient = new();
            gradient.SetColor(0, new Color(0x0C / 255f, 0x5A / 255f, 0x7F / 255f));
            gradient.SetColor(1, new Color(0x0C / 255f, 0x3A / 255f, 0x65 / 255f));
            gradient.SetOffset(0, 127f / 255f);
            gradient.SetOffset(1, 1f);

            GradientTexture2D gradientTexture = new()
            {
                Gradient = gradient,
                Fill = GradientTexture2D.FillEnum.Linear,
                FillFrom = new Vector2(0.5f, 0f),
                FillTo = new Vector2(0.5f, 1f),
                Width = 1,
                Height = 256,
            };

            _gradientRect = new TextureRect
            {
                Name = "GradientOverlay",
                Texture = gradientTexture,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.Scale,
                MouseFilter = MouseFilterEnum.Ignore,
            };
        }

        if (_gradientRect.GetParent() == null)
        {
            AddChild(_gradientRect);
        }

        // AS3: beginBitmapFill with no alpha/blend — tile image alpha is native.
        _tiles ??= new TextureRect
        {
            Name = "BackgroundTiles",
            MouseFilter = MouseFilterEnum.Ignore,
            StretchMode = TextureRect.StretchModeEnum.Tile,
        };

        if (ResourceLoader.Exists("res://assets/images/HabboBackground_background_tiles.png"))
        {
            _tiles.Texture = GD.Load<Texture2D>("res://assets/images/HabboBackground_background_tiles.png");
        }

        if (_tiles.GetParent() == null)
        {
            AddChild(_tiles);
        }

        Resize();
    }
}
