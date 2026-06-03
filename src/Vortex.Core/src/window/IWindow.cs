// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/window/IWindow.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;

namespace Vortex.Core.Window;

/// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/window/IWindow.as
public interface IWindow
{
    int id { get; set; }

    string name { get; set; }

    string caption { get; set; }

    float x { get; set; }

    float y { get; set; }

    float width { get; set; }

    float height { get; set; }

    bool visible { get; set; }

    IList<string> tags { get; }

    IWindow? parent { get; }

    bool disposed { get; }

    int numChildren { get; }

    /// @see IWindow.as::type
    uint type { get; set; }

    /// @see IWindow.as::state
    uint state { get; set; }

    /// @see IWindow.as::style
    uint style { get; set; }

    /// @see IWindow.as::param
    uint param { get; set; }

    /// @see IWindow.as::alpha
    uint alpha { get; set; }

    /// @see IWindow.as::color
    uint color { get; set; }

    /// @see IWindow.as::blend
    float blend { get; set; }

    /// @see IWindow.as::dynamicStyleColor
    ColorTransform? dynamicStyleColor { get; set; }

    /// @see IWindow.as::background
    bool background { get; set; }

    /// @see IWindow.as::clipping
    bool clipping { get; set; }

    /// @see IWindow.as::mouseThreshold
    uint mouseThreshold { get; set; }

    /// @see IWindow.as::procedure
    Action<WindowEvent, IWindow>? procedure { get; set; }

    /// @see IWindow.as::left
    float left { get; }

    /// @see IWindow.as::top
    float top { get; }

    /// @see IWindow.as::right
    float right { get; }

    /// @see IWindow.as::bottom
    float bottom { get; }

    /// @see IWindow.as::rectangle
    Rect2 rectangle { get; set; }

    /// @see IWindow.as::position
    Vector2 position { get; set; }

    /// @see IWindow.as::renderingX
    float renderingX { get; }

    /// @see IWindow.as::renderingY
    float renderingY { get; }

    /// @see IWindow.as::renderingWidth
    float renderingWidth { get; }

    /// @see IWindow.as::renderingHeight
    float renderingHeight { get; }

    /// @see IWindow.as::renderingRectangle
    Rect2 renderingRectangle { get; }

    bool Destroy();

    IWindow? GetChildAt(int param1);

    IWindow? GetChildByName(string param1);

    IWindow? FindChildByName(string param1);

    bool AddChild(IWindow param1);

    bool RemoveChild(IWindow param1);

    /// @see IWindow.as::addChildAt
    IWindow? AddChildAt(IWindow param1, int param2);

    /// @see IWindow.as::getChildByID
    IWindow? GetChildByID(int param1);

    /// @see IWindow.as::getChildByTag
    IWindow? GetChildByTag(string param1);

    /// @see IWindow.as::findChildByTag
    IWindow? FindChildByTag(string param1);

    /// @see IWindow.as::getChildIndex
    int GetChildIndex(IWindow param1);

    /// @see IWindow.as::removeChildAt
    IWindow? RemoveChildAt(int param1);

    /// @see IWindow.as::setChildIndex
    void SetChildIndex(IWindow param1, int param2);

    /// @see IWindow.as::swapChildren
    void SwapChildren(IWindow param1, IWindow param2);

    /// @see IWindow.as::swapChildrenAt
    void SwapChildrenAt(int param1, int param2);

    /// @see IWindow.as::groupChildrenWithID
    uint GroupChildrenWithID(uint param1, IList<IWindow> param2, int param3 = 0);

    /// @see IWindow.as::groupChildrenWithTag
    uint GroupChildrenWithTag(string param1, IList<IWindow> param2, int param3 = 0);

    /// @see IWindow.as::center
    void Center();

    /// @see IWindow.as::offset
    void Offset(float param1, float param2);

    /// @see IWindow.as::invalidate
    void Invalidate(Rect2? param1 = null);

    /// @see IWindow.as::setStateFlag
    void SetStateFlag(uint param1, bool param2 = true);

    /// @see IWindow.as::getStateFlag
    bool GetStateFlag(uint param1);

    /// @see IWindow.as::testStateFlag
    bool TestStateFlag(uint param1, uint param2 = 0);

    /// @see IWindow.as::setStyleFlag
    void SetStyleFlag(uint param1, bool param2 = true);

    /// @see IWindow.as::getStyleFlag
    bool GetStyleFlag(uint param1);

    /// @see IWindow.as::testStyleFlag
    bool TestStyleFlag(uint param1, uint param2 = 0);

    /// @see IWindow.as::setParamFlag
    void SetParamFlag(uint param1, bool param2 = true);

    /// @see IWindow.as::getParamFlag
    bool GetParamFlag(uint param1);

    /// @see IWindow.as::testParamFlag
    bool TestParamFlag(uint param1, uint param2 = 0);

    /// @see IWindow.as::activate
    bool Activate();

    /// @see IWindow.as::deactivate
    bool Deactivate();

    /// @see IWindow.as::minimize
    bool Minimize();

    /// @see IWindow.as::maximize
    bool Maximize();

    /// @see IWindow.as::restore
    bool Restore();

    /// @see IWindow.as::lock
    bool Lock();

    /// @see IWindow.as::unlock
    bool Unlock();

    /// @see IWindow.as::enable
    bool Enable();

    /// @see IWindow.as::disable
    bool Disable();

    /// @see IWindow.as::addEventListener
    void AddEventListener(string param1, Action<WindowEvent, IWindow> param2, int param3 = 0);

    /// @see IWindow.as::removeEventListener
    void RemoveEventListener(string param1, Action<WindowEvent, IWindow> param2);

    /// @see IWindow.as::hasEventListener
    bool HasEventListener(string param1);

    /// @see IWindow.as::findParentByName
    IWindow? FindParentByName(string param1);

    /// @see IWindow.as::isEnabled
    bool IsEnabled();

    /// @see IWindow.as::iterator
    object? Iterator();

    /// @see IWindow.as::setRectangle
    void SetRectangle(float param1, float param2, float param3, float param4);

    /// @see IWindow.as::scale
    void Scale(float param1, float param2);

    /// @see IWindow.as::getGlobalPosition
    Vector2 GetGlobalPosition();

    /// @see IWindow.as::hitTestLocalPoint
    bool HitTestLocalPoint(Vector2 param1);

    /// @see IWindow.as::hitTestGlobalPoint
    bool HitTestGlobalPoint(Vector2 param1);

    /// @see IWindow.as::convertPointFromGlobalToLocalSpace
    Vector2 ConvertPointFromGlobalToLocalSpace(Vector2 param1);

    /// @see IWindow.as::convertPointFromLocalToGlobalSpace
    Vector2 ConvertPointFromLocalToGlobalSpace(Vector2 param1);
}
