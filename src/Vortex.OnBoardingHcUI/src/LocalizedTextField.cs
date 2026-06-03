// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/LocalizedTextField.as

using Godot;

namespace Vortex.OnBoardingHcUi;

/// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/LocalizedTextField.as
public partial class LocalizedTextField : Label
{
    private bool _localized;
    private string? _key;

    public void DisposeLocalization()
    {
        RemoveOldLocalization(_key);
    }

    public string HtmlText
    {
        get => Text;
        set
        {
            Text = LocalizedSprite.ResolveLocalizedText(value);
            CheckLocalization(value);
        }
    }

    public string Localization
    {
        set => Text = value;
    }

    private void RemoveOldLocalization(string? key)
    {
        if (_localized && !string.IsNullOrWhiteSpace(key))
        {
            _localized = false;
        }
    }

    private void CheckLocalization(string? key)
    {
        if (string.IsNullOrWhiteSpace(key) || !key.StartsWith("${", System.StringComparison.Ordinal))
        {
            return;
        }

        RemoveOldLocalization(_key);

        _localized = true;
        _key = key;
    }
}
