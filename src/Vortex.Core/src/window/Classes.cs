// @see core/window/Classes.as

using System;

using Vortex.Core.Window.Components;

namespace Vortex.Core.Window;

/// @see core/window/Classes.as
public class Classes
{
    /// @see core/window/Classes.as::var_1636
    protected static Dictionary<uint, Type>? Var1636;

    /// @see core/window/Classes.as::Classes
    public Classes() { }

    /// @see core/window/Classes.as::init
    public static void Init()
    {
        if (Var1636 != null)
        {
            return;
        }

        Var1636 = new Dictionary<uint, Type>
        {
            [0] = typeof(WindowController),
            [40] = typeof(ActivatorController),
            [2] = typeof(BackgroundController),
            [30] = typeof(BorderController),
            [17] = typeof(BoxSizerController),
            [45] = typeof(BubbleController),
            [46] = typeof(WindowController),
            [47] = typeof(WindowController),
            [48] = typeof(WindowController),
            [49] = typeof(WindowController),
            [60] = typeof(ButtonController),
            [61] = typeof(ButtonController),
            [67] = typeof(SelectableButtonController),
            [68] = typeof(SelectableButtonController),
            [69] = typeof(SelectableButtonController),
            [21] = typeof(BitmapWrapperController),
            [70] = typeof(CheckBoxController),
            [4] = typeof(ContainerController),
            [41] = typeof(ContainerButtonController),
            [72] = typeof(CloseButtonController),
            [20] = typeof(DisplayObjectWrapperController),
            [76] = typeof(ScrollBarLiftController),
            [102] = typeof(DropMenuController),
            [103] = typeof(DropMenuItemController),
            [105] = typeof(DropListController),
            [106] = typeof(DropListItemController),
            [15] = typeof(FormattedTextController),
            [35] = typeof(FrameController),
            [6] = typeof(HeaderController),
            [11] = typeof(HtmlTextController),
            [1] = typeof(IconController),
            [79] = typeof(IconButtonController),
            [50] = typeof(ItemListController),
            [51] = typeof(ItemListController),
            [52] = typeof(ItemGridController),
            [54] = typeof(ItemGridController),
            [53] = typeof(ItemGridController),
            [12] = typeof(TextLabelController),
            [14] = typeof(TextLinkController),
            [78] = typeof(PasswordFieldController),
            [71] = typeof(RadioButtonController),
            [5] = typeof(RegionController),
            [120] = typeof(ScalerController),
            [130] = typeof(ScrollBarController),
            [131] = typeof(ScrollBarController),
            [139] = typeof(ButtonController),
            [137] = typeof(ButtonController),
            [138] = typeof(ButtonController),
            [136] = typeof(ButtonController),
            [132] = typeof(ScrollBarLiftController),
            [133] = typeof(ScrollBarLiftController),
            [134] = typeof(WindowController),
            [135] = typeof(WindowController),
            [56] = typeof(ScrollableItemListWindow),
            [140] = typeof(ScrollableItemGridWindow),
            [42] = typeof(SelectorController),
            [43] = typeof(SelectorListController),
            [23] = typeof(StaticBitmapWrapperController),
            [93] = typeof(TabButtonController),
            [94] = typeof(TabContainerButtonController),
            [90] = typeof(ContainerController),
            [91] = typeof(TabContextController),
            [92] = typeof(SelectorListController),
            [10] = typeof(TextController),
            [77] = typeof(TextFieldController),
            [8] = typeof(ToolTipController),
            [16] = typeof(WidgetWindowController),
        };
    }

    /// @see core/window/Classes.as::getWindowClassByType
    public static Type? GetWindowClassByType(uint param1)
    {
        Init();

        return Var1636 != null && Var1636.TryGetValue(param1, out Type? windowType) ? windowType : null;
    }

    /// @see core/window/Classes.as::init
    public virtual object? Init(params object?[] args)
    {
        Init();

        return null;
    }

    /// @see core/window/Classes.as::getWindowClassByType
    public virtual object? GetWindowClassByType(params object?[] args)
    {
        if (args.Length == 0 || args[0] == null)
        {
            return null;
        }

        return GetWindowClassByType(Convert.ToUInt32(args[0]));
    }
}
