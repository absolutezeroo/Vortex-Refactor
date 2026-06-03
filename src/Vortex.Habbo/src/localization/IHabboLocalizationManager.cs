using Vortex.Core.Localization;

namespace Vortex.Habbo.Localization;

/// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as
public interface IHabboLocalizationManager : ICoreLocalizationManager
{
    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::loadDefaultEmbedLocalizations
    bool LoadDefaultEmbedLocalizations(string param1, bool param2 = true);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::requestLocalizationInit
    void RequestLocalizationInit();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::getActiveEnvironmentId
    string GetActiveEnvironmentId();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::getExternalVariablesUrl
    string GetExternalVariablesUrl();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::getExternalVariablesHash
    string GetExternalVariablesHash();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::getLocalizationWithParams
    string GetLocalizationWithParams(string param1, string param2 = "", params string[] rest);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::getBadgeName
    string GetBadgeName(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::getBadgeDesc
    string GetBadgeDesc(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::getBadgeBaseName
    string GetBadgeBaseName(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::getAchievementName
    string GetAchievementName(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::getAchievementDesc
    string GetAchievementDesc(string param1, int param2);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::getAchievementInstruction
    string GetAchievementInstruction(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::setBadgePointLimit
    void SetBadgePointLimit(string param1, int param2);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/IHabboLocalizationManager.as::getPreviousLevelBadgeId
    string GetPreviousLevelBadgeId(string param1);
}
