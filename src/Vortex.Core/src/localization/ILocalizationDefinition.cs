namespace Vortex.Core.Localization;

public interface ILocalizationDefinition
{
    string id { get; }

    string languageCode { get; }

    string countryCode { get; }

    string encoding { get; }

    string name { get; }

    string url { get; }
}
