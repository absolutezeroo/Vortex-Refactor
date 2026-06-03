using System;
using System.IO;

using Godot;

using Vortex.Core;
using Vortex.Core.Localization;
using Vortex.Core.Runtime;
using Vortex.IID;

namespace Vortex.Habbo.Localization;

/// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as
public class HabboLocalizationManager : CoreLocalizationManager, IHabboLocalizationManager
{
    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as — flag constant 0x10000000
    public const uint SKIP_EXTERNAL_LOCALIZATIONS = 0x10000000;

    private static readonly string[] _romanNumerals =
    [
        "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X",
        "XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX",
        "XXI", "XXII", "XXIII", "XXIV", "XXV", "XXVI", "XXVII", "XXVIII", "XXIX", "XXX",
    ];

    private readonly Dictionary<string, int> _badgePointLimits = new(StringComparer.Ordinal);
    private bool _isLocalizationInitialized;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::HabboLocalizationManager
    public HabboLocalizationManager(IContext? param1 = null, uint param2 = 0, object? param3 = null)
        : base(param1, param2, param3)
    {
        // Godot/C# adaptation: _skipExternals is read from _flags in InitComponent() since
        // C# runs base constructor (which calls InitComponent) before this body executes.
        // Unlike AS3 where constructor body runs before super().
        RegisterInterface(new IIDHabboLocalizationManager(), this);
        RegisterInterface(new IIDCoreLocalizationManager(), this);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::dispose
    public override void Dispose()
    {
        base.Dispose();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::loadDefaultEmbedLocalizations
    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::loadDefaultEmbedLocalizations
    public bool LoadDefaultEmbedLocalizations(string param1, bool param2 = true)
    {
        string assetName = "default_localizations_" + param1;

        if (!TryLoadLocalizationAsset(assetName, out string? localizationContent) &&
            !string.Equals(param1, "en", StringComparison.Ordinal) && param2)
        {
            Logger.Warn("Could not load default localizations " + assetName + " : Trying with default_localizations_en...");
            return LoadDefaultEmbedLocalizations("en", false);
        }

        if (localizationContent != null)
        {
            ParseLocalizationData(localizationContent);
        }

        // Godot/C# adaptation: development overrides load LAST so they take priority during development.
        // In AS3, "default_localizations" (the generic base) loaded first and language-specific on top.
        // Here development_localizations acts as a dev-time override file.
        if (TryLoadLocalizationAsset("development_localizations", out string? devContent))
        {
            ParseLocalizationData(devContent);
        }

        if (localizationContent != null)
        {
            return true;
        }

        Logger.Warn("Could not load " + assetName);
        return false;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getLocalizationWithParams
    public string GetLocalizationWithParams(string param1, string param2 = "", params string[] rest)
    {
        if (rest.Length > 0)
        {
            for (int i = 0;
                 i < rest.Length / 2;
                 i++)
            {
                RegisterParameter(param1, rest[i * 2], rest[(i * 2) + 1]);
            }
        }

        return GetLocalization(param1, param2);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getExternalVariablesUrl
    public string GetExternalVariablesUrl()
    {
        return GetGameDataResources()?.GetExternalVariablesUrl() ?? string.Empty;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getExternalVariablesHash
    public string GetExternalVariablesHash()
    {
        return GetGameDataResources()?.GetExternalVariablesHash() ?? string.Empty;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getActiveEnvironmentId
    public override string GetActiveEnvironmentId()
    {
        return base.GetActiveEnvironmentId();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getLocalization
    public override string GetLocalization(string param1, string param2 = "")
    {
        string value = base.GetLocalization(param1, param2);
        return Interpolate(value);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getAchievementName
    public string GetAchievementName(string param1)
    {
        BadgeBaseAndLevel badgeInfo = new(param1);
        string key = GetExistingKey(
            [
                "badge_name_al_" + param1,
                "badge_name_al_" + badgeInfo.@base,
                "badge_name_" + param1,
                "badge_name_" + badgeInfo.@base,
            ]
        );
        RegisterParameter(key, "roman", GetRomanNumeral(badgeInfo.level));
        string? localization = GetLocalization(key);
        return localization ?? string.Empty;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getAchievementDesc
    public string GetAchievementDesc(string param1, int param2)
    {
        BadgeBaseAndLevel badgeInfo = new(param1);
        string key = GetExistingKey(
            [
                "badge_desc_al_" + param1,
                "badge_desc_al_" + badgeInfo.@base,
                "badge_desc_" + param1,
                "badge_desc_" + badgeInfo.@base,
            ]
        );
        RegisterParameter(key, "limit", param2.ToString());
        RegisterParameter(key, "roman", GetRomanNumeral(badgeInfo.level));
        return GetLocalization(key);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getAchievementInstruction
    public string GetAchievementInstruction(string param1)
    {
        BadgeBaseAndLevel badgeInfo = new(param1);
        string key = GetExistingKey(["badge_instruction_" + badgeInfo.@base]);
        RegisterParameter(key, "limit", GetBadgePointLimit(param1).ToString());
        string? localization = GetLocalization(key);
        return localization ?? string.Empty;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getBadgeBaseName
    public string GetBadgeBaseName(string param1)
    {
        BadgeBaseAndLevel badgeInfo = new(param1);
        return badgeInfo.@base;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getBadgeName
    public string GetBadgeName(string param1)
    {
        BadgeBaseAndLevel badgeInfo = new(param1);
        string key = FixBadLocalization(GetExistingKey(["badge_name_" + param1, "badge_name_" + badgeInfo.@base]));
        RegisterParameter(key, "roman", GetRomanNumeral(badgeInfo.level));
        return GetLocalization(key);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getBadgeDesc
    public string GetBadgeDesc(string param1)
    {
        BadgeBaseAndLevel badgeInfo = new(param1);
        string key = FixBadLocalization(GetExistingKey(["badge_desc_" + param1, "badge_desc_" + badgeInfo.@base]));
        RegisterParameter(key, "limit", GetBadgePointLimit(param1).ToString());
        RegisterParameter(key, "roman", GetRomanNumeral(badgeInfo.level));
        string localization = GetLocalization(key);
        return string.Equals(key, localization, StringComparison.Ordinal) ? string.Empty : localization;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getPreviousLevelBadgeId
    public string GetPreviousLevelBadgeId(string param1)
    {
        BadgeBaseAndLevel badgeInfo = new(param1);
        badgeInfo.level--;
        return badgeInfo.badgeId;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::setBadgePointLimit
    public void SetBadgePointLimit(string param1, int param2)
    {
        _badgePointLimits[param1] = param2;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::requestLocalizationInit
    public void RequestLocalizationInit()
    {
        if (_isLocalizationInitialized)
        {
            return;
        }

        _isLocalizationInitialized = true;
        events.AddEventListener("LOCALIZATION_EVENT_LOCALIZATION_LOADED", OnLocalizationLoaded);
        events.AddEventListener("LOCALIZATION_EVENT_LOCALIZATION_FAILED", OnManagerLocalizationFailed);
        LoadLocalizationFromURL(GetProperty("gamedata.hashes.url"), GetProperty("environment.id"));
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::initComponent
    protected override void InitComponent()
    {
        base.InitComponent();
        ConfigureLocalizationLocations();

        // Godot/C# adaptation: read flag from _flags directly since C# base constructor
        // runs InitComponent() before the derived constructor body sets _skipExternals.
        if ((_flags & SKIP_EXTERNAL_LOCALIZATIONS) > 0)
        {
            events.DispatchEvent("complete");
        }
        else
        {
            context.events.AddEventListener("HABBO_CONNECTION_EVENT_AUTHENTICATED", OnAuthenticated);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::onAuthenticated
    private void OnAuthenticated(object? param1)
    {
        _ = param1;
        RequestLocalizationInit();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::fixBadLocalization
    private static string FixBadLocalization(string param1)
    {
        string result = param1.Replace("${", "$", StringComparison.Ordinal);
        result = result.Replace("{", "$", StringComparison.Ordinal);
        result = result.Replace("}", "$", StringComparison.Ordinal);
        return result;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::configureLocalizationLocations
    private void ConfigureLocalizationLocations()
    {
        int index = 1;

        while (PropertyExists("localization." + index))
        {
            string name = GetProperty("localization." + index);
            string code = GetProperty("localization." + index + ".code");
            string displayName = GetProperty("localization." + index + ".name");
            string url = GetProperty("localization." + index + ".url");
            RegisterLocalizationDefinition(name, displayName, url, code);
            index++;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getBadgePointLimit
    private int GetBadgePointLimit(string param1)
    {
        return _badgePointLimits.GetValueOrDefault(param1, 0);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getExistingKey
    private string GetExistingKey(string[] param1)
    {
        foreach (string candidateKey in param1)
        {
            string value = base.GetLocalization(candidateKey);

            if (!string.IsNullOrEmpty(value))
            {
                return candidateKey;
            }
        }

        return param1[0];
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::getRomanNumeral
    private static string GetRomanNumeral(int param1)
    {
        int index = Math.Max(0, param1 - 1);
        return index < _romanNumerals.Length ? _romanNumerals[index] : param1.ToString();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::onManagerLocalizationFailed
    private void OnManagerLocalizationFailed(object? param1)
    {
        _ = param1;
        _isLocalizationInitialized = false;
        CoreEnvironment.Crash("Failed loading gamedata hashes", CoreEnvironment.ERROR_CATEGORY_DOWNLOAD_LOCALIZATION);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::onLocalizationLoaded
    private void OnLocalizationLoaded(object? param1)
    {
        _ = param1;
        _isLocalizationInitialized = false;
        events.RemoveEventListener("LOCALIZATION_EVENT_LOCALIZATION_LOADED", OnLocalizationLoaded);
        events.RemoveEventListener("LOCALIZATION_EVENT_LOCALIZATION_FAILED", OnManagerLocalizationFailed);
        LocalizationsReady();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/HabboLocalizationManager.as::localizationsReady
    private void LocalizationsReady()
    {
        events.DispatchEvent("complete");
    }

    private static bool TryLoadLocalizationAsset(string assetName, out string? content)
    {
        string[] candidatePaths =
        [
            Path.Combine("data", "localization", assetName + ".txt"),
            Path.Combine("data", "localization", assetName),
            Path.Combine("data", assetName + ".txt"),
            Path.Combine("data", assetName),
        ];

        foreach (string candidate in candidatePaths)
        {
            string resolved = ResolvePath(candidate);

            if (!File.Exists(resolved))
            {
                continue;
            }

            content = File.ReadAllText(resolved);
            return true;
        }

        content = null;
        return false;
    }

    private static string ResolvePath(string path)
    {
        if (path.StartsWith("res://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
        {
            return ProjectSettings.GlobalizePath(path);
        }

        if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase) && Uri.TryCreate(path, UriKind.Absolute, out Uri? fileUri))
        {
            return fileUri.LocalPath;
        }

        if (Path.IsPathRooted(path))
        {
            return path;
        }

        string normalizedPath = path.Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), normalizedPath));
    }
}
