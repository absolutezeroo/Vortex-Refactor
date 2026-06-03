// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/LocalizedSprite.as

using System;
using System.Text.RegularExpressions;

using Godot;

namespace Vortex.OnBoardingHcUi;

/// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/LocalizedSprite.as
public partial class LocalizedSprite : Control
{
    private static readonly Regex LocalizationPattern = new(@"\$\{(?<key>[^}]+)\}", RegexOptions.Compiled);

    /// Godot adaptation: a lightweight resolver replaces AS3 localization listeners.
    public static Func<string, string>? LocalizationResolver { get; set; }

    protected bool _localized;
    protected string? _localizationKey;

    public virtual new void Dispose()
    {
        RemoveOldLocalization(_localizationKey);
    }

    public virtual string Localization
    {
        set => _ = value;
    }

    public static string ResolveLocalizedText(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (LocalizationResolver == null)
        {
            return value;
        }

        return LocalizationPattern.Replace(
            value,
            match =>
            {
                string key = match.Groups["key"].Value;
                string resolved = LocalizationResolver.Invoke(key);

                return string.IsNullOrWhiteSpace(resolved) ? match.Value : resolved;
            }
        );
    }

    protected void RemoveOldLocalization(string? key)
    {
        if (_localized && !string.IsNullOrWhiteSpace(key))
        {
            _localized = false;
        }
    }

    protected void CheckLocalization(string? key)
    {
        _localized = !string.IsNullOrWhiteSpace(key) && key.StartsWith("${", StringComparison.Ordinal);
        _localizationKey = key;
    }
}
