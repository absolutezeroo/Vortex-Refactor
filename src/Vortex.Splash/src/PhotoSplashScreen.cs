// @see WIN63-202111081545-75921380-Source-main/src/splash/PhotoSplashScreen.as

using System;

using Godot;

namespace Vortex.Splash;

/// @see WIN63-202111081545-75921380-Source-main/src/splash/PhotoSplashScreen.as
public partial class PhotoSplashScreen : Control
{
    private const string IMAGE_ROOT = "res://assets/images/";
    private const string SPLASH_BACKGROUND_PATH = IMAGE_ROOT + "splash_bg_class.png";
    private const string SPLASH_TOP_PATH = IMAGE_ROOT + "splash_top_class.png";
    private const int SPLASH_IMAGE_COUNT = 30;
    private const int SPLASH_IMAGE_OFFSET_X = 96;
    private const int SPLASH_IMAGE_OFFSET_Y = 51;
    private const int FALLBACK_WIDTH = 500;
    private const int FALLBACK_HEIGHT = 434;

    /// @see WIN63-202111081545-75921380-Source-main/src/splash/PhotoSplashScreen.as::PhotoSplashScreen
    public PhotoSplashScreen() : this(null) { }

    /// @see WIN63-202111081545-75921380-Source-main/src/splash/PhotoSplashScreen.as::PhotoSplashScreen
    public PhotoSplashScreen(Node? param1)
    {
        _ = param1;
        MouseFilter = MouseFilterEnum.Ignore;
        Build();
    }

    private void Build()
    {
        TextureRect? background = CreateTextureLayer(SPLASH_BACKGROUND_PATH, Vector2.Zero);

        if (background != null)
        {
            AddChild(background);

            Vector2 baseSize = background.Texture?.GetSize() ?? new Vector2(FALLBACK_WIDTH, FALLBACK_HEIGHT);
            Size = baseSize;
            CustomMinimumSize = baseSize;
        }
        else
        {
            ColorRect fallback = new()
            {
                Name = "FallbackSplash",
                Color = new Color(0.12f, 0.2f, 0.24f),
                Size = new Vector2(FALLBACK_WIDTH, FALLBACK_HEIGHT),
                CustomMinimumSize = new Vector2(FALLBACK_WIDTH, FALLBACK_HEIGHT),
                MouseFilter = MouseFilterEnum.Ignore,
            };
            AddChild(fallback);

            Size = fallback.Size;
            CustomMinimumSize = fallback.Size;
        }

        int imageIndex = Random.Shared.Next(1, SPLASH_IMAGE_COUNT + 1);
        string imagePath = $"{IMAGE_ROOT}splash_img{imageIndex}.png";
        TextureRect? image = CreateTextureLayer(imagePath, new Vector2(SPLASH_IMAGE_OFFSET_X, SPLASH_IMAGE_OFFSET_Y));

        if (image != null)
        {
            AddChild(image);
        }

        TextureRect? top = CreateTextureLayer(SPLASH_TOP_PATH, Vector2.Zero);

        if (top != null)
        {
            AddChild(top);
        }
    }

    private static TextureRect? CreateTextureLayer(string texturePath, Vector2 position)
    {
        if (!ResourceLoader.Exists(texturePath))
        {
            return null;
        }

        Texture2D? texture = GD.Load<Texture2D>(texturePath);

        if (texture == null)
        {
            return null;
        }

        return new TextureRect
        {
            Texture = texture,
            Position = position,
            Size = texture.GetSize(),
            CustomMinimumSize = texture.GetSize(),
            MouseFilter = MouseFilterEnum.Ignore,
        };
    }
}
