using Godot;

namespace Vortex.Habbo.Room.Object.Visualization.Pet;

/// @see com.sulake.habbo.room.object.visualization.pet.ExperienceData
public class ExperienceData
{
    private bool _ownsImage;
    private Image? _cleanBackup;
    private int _amount = -1;

    public ExperienceData(Image? source, bool ownsImage = true)
    {
        ImageData = source;
        _ownsImage = ownsImage;

        if (ImageData != null)
        {
            _cleanBackup = (Image)ImageData.Duplicate();
        }

        SetExperience(0);
    }

    public int Alpha { get; set; }

    public Image? ImageData { get; private set; }

    public void Dispose()
    {
        _cleanBackup = null;

        if (ImageData != null)
        {
            if (_ownsImage)
            {
                ImageData = null;
            }

            ImageData = null;
        }
    }

    /// @see ExperienceData.as::setExperience
    /// AS3 uses TextField to render "+N" text. Godot adaptation: draw text onto Image
    /// using SystemFont and DrawStringOutline/DrawString.
    public void SetExperience(int amount)
    {
        if (_amount == amount || ImageData == null)
        {
            return;
        }

        _amount = amount;

        // Reset to clean backup
        if (_cleanBackup != null)
        {
            ImageData.BlitRect(_cleanBackup, new Rect2I(Vector2I.Zero, new Vector2I(_cleanBackup.GetWidth(), _cleanBackup.GetHeight())),
                Vector2I.Zero);
        }

        // TODO: Render "+{amount}" text overlay at position (15, 19).
        // AS3 uses TextField with font "Volter", color white (0xFFFFFF), size 9.
        // Godot equivalent requires Font resource + Image drawing — deferred until
        // font system integration is available.
    }
}
