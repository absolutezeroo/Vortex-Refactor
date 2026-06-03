namespace Vortex.Core.Localization;

/// @see WIN63-202407091256-704579380-Source-main/core/localization/LocalizationDefinition.as
public class LocalizationDefinition
{
    /// @see WIN63-202407091256-704579380-Source-main/core/localization/LocalizationDefinition.as::LocalizationDefinition
    public LocalizationDefinition(string param1, string param2, string param3)
    {
        string[] parts = param1.Split('_');
        string[] subParts = (parts.Length > 1 ? parts[1] : "").Split('.');
        languageCode = parts[0];
        countryCode = subParts[0];
        encoding = subParts.Length > 1 ? subParts[1] : string.Empty;
        name = param2;
        url = param3;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/LocalizationDefinition.as::get id
    public string id => languageCode + "_" + countryCode + "." + encoding;

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/LocalizationDefinition.as::get languageCode
    public string languageCode { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/LocalizationDefinition.as::get countryCode
    public string countryCode { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/LocalizationDefinition.as::get encoding
    public string encoding { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/LocalizationDefinition.as::get name
    public string name { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/LocalizationDefinition.as::get url
    public string url { get; }
}
