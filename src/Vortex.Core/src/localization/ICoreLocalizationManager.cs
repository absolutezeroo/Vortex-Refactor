using Vortex.Core.Runtime;

namespace Vortex.Core.Localization;

/// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as
public interface ICoreLocalizationManager : IUnknown
{
    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::hasLocalization
    bool HasLocalization(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::getLocalization
    string GetLocalization(string param1, string param2 = "");

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::updateLocalization
    void UpdateLocalization(string param1, string param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::registerParameter
    string RegisterParameter(string param1, string param2, string param3, string param4 = "%");

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::getKeys
    string[] GetKeys();

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::interpolate
    string? Interpolate(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::getProperty
    string GetProperty(string param1, IDictionary<string, string>? param2 = null);

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::registerLocalizationDefinition
    void RegisterLocalizationDefinition(string param1, string param2, string param3, string param4);

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::activateLocalizationDefinition
    bool ActivateLocalizationDefinition(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::loadLocalizationFromURL
    void LoadLocalizationFromURL(string param1, string param2, bool param3 = false);

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::registerListener
    bool RegisterListener(string param1, ILocalizable param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::removeListener
    bool RemoveListener(string param1, ILocalizable param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/ICoreLocalizationManager.as::getGameDataResources
    IGameDataResources? GetGameDataResources();
}
