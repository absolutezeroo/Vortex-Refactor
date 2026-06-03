// @see habbo/window/IHabboWindowManager.as

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Window;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Utils;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Window;

/// @see habbo/window/IHabboWindowManager.as
public interface IHabboWindowManager : IWindowFactory
{
    /// @see habbo/window/IHabboWindowManager.as::get resourceManager
    object? ResourceManager();

    /// @see habbo/window/IHabboWindowManager.as::get assets
    object? Assets();

    /// @see habbo/window/IHabboWindowManager.as::get habboPagesStyleSheet
    object? HabboPagesStyleSheet();

    /// @see habbo/window/IHabboWindowManager.as::createWindow
    IWindow? CreateWindow
    (
        string param1,
        string param2 = "",
        uint param3 = 0,
        uint param4 = 0,
        uint param5 = 0,
        Rect2? param6 = null,
        Action<WindowEvent, IWindow>? param7 = null,
        uint param8 = 0,
        uint param9 = 1,
        string param10 = ""
    );

    /// @see habbo/window/IHabboWindowManager.as::removeWindow
    void RemoveWindow(string param1, uint param2 = 1);

    /// @see habbo/window/IHabboWindowManager.as::getWindowByName
    IWindow? GetWindowByName(string param1, uint param2 = 1);

    /// @see habbo/window/IHabboWindowManager.as::getActiveWindow
    IWindow? GetActiveWindow(uint param1 = 1);

    /// @see habbo/window/IHabboWindowManager.as::toggleFullScreen
    void ToggleFullScreen();

    /// @see habbo/window/IHabboWindowManager.as::getWindowContext
    IWindowContext? GetWindowContext(uint param1);

    /// @see habbo/window/IHabboWindowManager.as::alert
    IClass3348? Alert(string param1, string param2, uint param3, Action? param4);

    /// @see habbo/window/IHabboWindowManager.as::alertWithModal
    IClass3348? AlertWithModal(string param1, string param2, uint param3, Action? param4);

    /// @see habbo/window/IHabboWindowManager.as::alertWithLink
    IAlertDialogWithLink? AlertWithLink(string param1, string param2, string param3, string param4, uint param5, Action? param6);

    /// @see habbo/window/IHabboWindowManager.as::registerLocalizationParameter
    void RegisterLocalizationParameter(string param1, string param2, string param3, string param4 = "%");

    /// @see habbo/window/IHabboWindowManager.as::addMouseEventTracker
    void AddMouseEventTracker(IInputEventTracker param1);

    /// @see habbo/window/IHabboWindowManager.as::removeMouseEventTracker
    void RemoveMouseEventTracker(IInputEventTracker param1);

    /// @see habbo/window/IHabboWindowManager.as::createUnseenItemCounter
    IWindowContainer? CreateUnseenItemCounter();

    /// @see habbo/window/IHabboWindowManager.as::buildModalDialogFromXML
    IModalDialog? BuildModalDialogFromXml(XElement param1);

    /// @see habbo/window/IHabboWindowManager.as::simpleAlert
    void SimpleAlert
    (
        string param1,
        string param2,
        string param3,
        string? param4 = null,
        string? param5 = null,
        Dictionary<string, object?>? param6 = null,
        string? param7 = null,
        Action? param8 = null,
        Action? param9 = null
    );

    /// @see habbo/window/IHabboWindowManager.as::registerHintWindow
    void RegisterHintWindow(string param1, IWindow param2, int param3 = 1);

    /// @see habbo/window/IHabboWindowManager.as::unregisterHintWindow
    void UnregisterHintWindow(string param1);

    /// @see habbo/window/IHabboWindowManager.as::showHint
    void ShowHint(string param1, Rect2? param2 = null);

    /// @see habbo/window/IHabboWindowManager.as::hideHint
    void HideHint();

    /// @see habbo/window/IHabboWindowManager.as::displayFloorPlanEditor
    void DisplayFloorPlanEditor();

    /// @see habbo/window/IHabboWindowManager.as::openHelpPage
    void OpenHelpPage(string param1);

    /// @see habbo/window/IHabboWindowManager.as::hideMatchingHint
    void HideMatchingHint(string param1);
}
