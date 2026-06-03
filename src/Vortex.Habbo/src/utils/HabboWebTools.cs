// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as

using System;

using Godot;

namespace Vortex.Habbo.Utils;

/// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as
public static class HabboWebTools
{
    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::const_222
    public const string WINDOW_ADVERTISEMENT = "advertisement";

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::const_338
    public const string TARGET_SELF = "_self";

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::WINDOW_HABBO_MAIN
    public const string WINDOW_HABBO_MAIN = "habboMain";

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::OPEN_INTERNAL_LINK_FROM_WEB_CALLBACK
    public const string OPEN_INTERNAL_LINK_FROM_WEB_CALLBACK = "openlink";

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::GOTO_ROOM_FROM_WEB_CALLBACK
    public const string GOTO_ROOM_FROM_WEB_CALLBACK = "openroom";

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::HABBLET_AVATARS
    public const string HABBLET_AVATARS = "avatars";

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::HABBLET_MINI_MAIL
    public const string HABBLET_MINI_MAIL = "minimail";

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::HABBLET_ROOM_ENTER_AD
    public const string HABBLET_ROOM_ENTER_AD = "roomenterad";

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::HABBLET_NEWS
    public const string HABBLET_NEWS = "news";

    private static bool _isSpaWeb;
    private static string? _baseUrl;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::set isSpaWeb
    public static bool IsSpaWeb
    {
        set => _isSpaWeb = value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::set baseUrl
    public static string? BaseUrl
    {
        set => _baseUrl = value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::logEventLog
    public static void LogEventLog(string param1)
    {
        try
        {
            Logger.Info($"FlashExternalInterface.logEventLog: {param1}");
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working, failed to log event log.", e);
        }
    }

    /// Desktop bridge for FlashExternalInterface.logLoginStep calls scattered in AS3 source.
    public static void LogLoginStep(string param1, string? param2 = null)
    {
        try
        {
            if (param2 != null)
            {
                Logger.Info($"FlashExternalInterface.logLoginStep: {param1}, {param2}");
            }
            else
            {
                Logger.Info($"FlashExternalInterface.logLoginStep: {param1}");
            }
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working, failed to log login step.", e);
        }
    }

    /// Desktop bridge for FlashExternalInterface.logWarn calls scattered in AS3 source.
    public static void LogWarn(string param1)
    {
        try
        {
            Logger.Warn($"FlashExternalInterface.logWarn: {param1}");
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working, failed to log warning.", e);
        }
    }

    /// Desktop bridge for FlashExternalInterface.logDebug calls scattered in AS3 source.
    public static void LogDebug(string param1)
    {
        try
        {
            Logger.Debug($"FlashExternalInterface.logDebug: {param1}");
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working, failed to log debug.", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::openWebPage
    public static void OpenWebPage(string param1, string param2 = "")
    {
        if (string.IsNullOrEmpty(param2))
        {
        }

        if (string.IsNullOrEmpty(param1))
        {
            return;
        }

        try
        {
            NavigateToUrl(param1);
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working, failed to open web page.", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::openPage
    public static void OpenPage(string param1)
    {
        try
        {
            if (param1.StartsWith("http", StringComparison.Ordinal))
            {
                NavigateToUrl(param1);
            }
            else
            {
                NavigateToUrl(_baseUrl + "/" + param1);
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to open web page {param1}", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::sendHeartBeat
    public static void SendHeartBeat()
    {
        try
        {
            Logger.Debug("FlashExternalInterface.heartBeat");
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working, failed to send heartbeat.", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::openWebPageAndMinimizeClient
    public static void OpenWebPageAndMinimizeClient(string param1)
    {
        try
        {
            if (_isSpaWeb)
            {
                OpenPage(param1);
                return;
            }

            if (param1.StartsWith("http", StringComparison.Ordinal))
            {
                NavigateToUrl(param1);
            }
            else
            {
                NavigateToUrl(_baseUrl + "/" + param1);
            }

            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Minimized);
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to open web page {param1}", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::closeWebPageAndRestoreClient
    public static void CloseWebPageAndRestoreClient()
    {
        try
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
        catch (Exception e)
        {
            Logger.Error("Failed to close web page and restore client!", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::openWebHabblet
    public static void OpenWebHabblet(string param1, string? param2 = null)
    {
        try
        {
            Logger.Debug($"FlashExternalInterface.openHabblet: {param1}, {param2}");
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to open Habblet {param1}", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::closeWebHabblet
    public static void CloseWebHabblet(string param1, string? param2 = null)
    {
        try
        {
            Logger.Debug($"FlashExternalInterface.closeHabblet: {param1}, {param2}");
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to close Habblet {param1}", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::sendDisconnectToWeb
    public static void SendDisconnectToWeb(int param1, string param2)
    {
        try
        {
            Logger.Info($"FlashExternalInterface.disconnect: {param1}, {param2}");
        }
        catch (Exception e)
        {
            Logger.Error("Failed to send disconnect.", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::showGame
    public static void ShowGame(string param1)
    {
        try
        {
            Logger.Debug($"FlashExternalGameInterface.showGame: {param1}");
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to open game: {e.Message}", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::hideGame
    public static void HideGame()
    {
        try
        {
            Logger.Debug("FlashExternalGameInterface.hideGame");
        }
        catch (Exception e)
        {
            Logger.Error("Failed to hide game", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::navigateToURL
    public static void NavigateToUrl(string param1)
    {
        if (string.IsNullOrEmpty(param1))
        {
            Logger.Warn("Can not navigate to empty url");
            return;
        }

        OS.ShellOpen(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::openExternalLinkWarning
    public static void OpenExternalLinkWarning(string param1)
    {
        try
        {
            NavigateToUrl(param1);
        }
        catch (Exception e)
        {
            Logger.Error($"External interface not working. Could not request to open: {param1}", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::roomVisited
    public static void RoomVisited(int param1)
    {
        try
        {
            Logger.Debug($"FlashExternalInterface.roomVisited: {param1}");
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working. Could not store last room visit.", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::openMinimail
    public static void OpenMinimail(string param1)
    {
        try
        {
            if (_isSpaWeb)
            {
                Logger.Debug($"FlashExternalInterface.openMinimail: {param1}");
            }
            else
            {
                OpenWebHabblet(HABBLET_MINI_MAIL, param1);
            }
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working. Could not open minimail.", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::openNews
    public static void OpenNews()
    {
        try
        {
            if (_isSpaWeb)
            {
                Logger.Debug("FlashExternalInterface.openNews");
            }
            else
            {
                OpenWebHabblet(HABBLET_NEWS);
            }
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working. Could not open news.", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::closeNews
    public static void CloseNews()
    {
        try
        {
            if (_isSpaWeb)
            {
                Logger.Debug("FlashExternalInterface.closeNews");
            }
            else
            {
                CloseWebHabblet(HABBLET_NEWS);
            }
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working. Could not close news.", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::openAvatars
    public static void OpenAvatars()
    {
        try
        {
            if (_isSpaWeb)
            {
                Logger.Debug("FlashExternalInterface.openAvatars");
            }
            else
            {
                OpenWebHabblet(HABBLET_AVATARS);
            }
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working. Could not open avatars.", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::openRoomEnterAd
    public static void OpenRoomEnterAd()
    {
        try
        {
            if (_isSpaWeb)
            {
                Logger.Debug("FlashExternalInterface.openRoomEnterAd");
            }
            else
            {
                OpenWebHabblet(HABBLET_ROOM_ENTER_AD, "");
            }
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working. Could not open roomenterad.", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::updateFigure
    public static void UpdateFigure(string param1)
    {
        try
        {
            if (_isSpaWeb)
            {
                Logger.Debug($"FlashExternalInterface.updateFigure: {param1}");
            }
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working. Could not update figure.", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/HabboWebTools.as::logOut
    public static void LogOut()
    {
        try
        {
            Logger.Info("FlashExternalInterface.logout");
        }
        catch (Exception e)
        {
            Logger.Error("External interface not working. Could not logout.", e);
        }
    }
}
