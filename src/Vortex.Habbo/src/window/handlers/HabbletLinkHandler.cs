// @see habbo/window/handlers/HabbletLinkHandler.as

using System;

using Vortex.Habbo.Utils;

namespace Vortex.Habbo.Window.Handlers;

/// <summary>
/// Link handler for "habblet/" prefixed URLs. Dispatches to web tools
/// for page/habblet opening.
/// </summary>
/// @see habbo/window/handlers/HabbletLinkHandler.as
public class HabbletLinkHandler : IDisposable
{
    private HabboWindowManagerComponent? _windowManager;

    /// @see HabbletLinkHandler.as::HabbletLinkHandler
    public HabbletLinkHandler(HabboWindowManagerComponent windowManager)
    {
        _windowManager = windowManager;
    }

    /// @see HabbletLinkHandler.as::get linkPattern
    public static string LinkPattern => "habblet/";

    /// @see HabbletLinkHandler.as::get disposed
    public bool Disposed => _windowManager == null;

    /// @see HabbletLinkHandler.as::linkReceived
    public void LinkReceived(string link)
    {
        if (_windowManager == null)
        {
            return;
        }

        string[] parts = link.Split('/');

        if (parts.Length < 2)
        {
            return;
        }

        switch (parts[1])
        {
            case "open" when parts.Length > 2:
            {
                string target = parts[2];

                if (target == "credits")
                {
                    // @see HabbletLinkHandler.as — "credits" opens shop URL and minimizes client
                    string shopUrl = _windowManager.GetProperty("web.shop.relativeUrl");
                    HabboWebTools.OpenWebPageAndMinimizeClient(shopUrl);
                }
                else
                {
                    // @see HabbletLinkHandler.as — any other target opens the named habblet
                    HabboWebTools.OpenWebHabblet(target);
                }

                break;
            }
        }
    }

    /// @see HabbletLinkHandler.as::dispose
    public void Dispose()
    {
        _windowManager = null;
        GC.SuppressFinalize(this);
    }
}
