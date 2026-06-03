// @see habbo/window/widgets/IlluminaChatBubbleWidget.as

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/IlluminaChatBubbleWidget.as
public class IlluminaChatBubbleWidget : IIlluminaChatBubbleWidget, IWidget
{
    private static readonly int RESIZING_OFFSETS = 10;

    private IWidgetWindow? _widgetWindow;
    private HabboWindowManagerComponent? _windowManager;
    private IWindowContainer? _container;
    private bool _resizing;
    private IItemListWindow? _messageContainer;
    private readonly ITextWindow? _messageTemplate;
    private readonly IWindow? _messageTemplateWindow;
    private IItemListWindow? _spacedMessageContainer;
    private readonly IWindow? _userNameLabel;
    private readonly IWindow? _avatarParent;
    private readonly IWindow? _bubbleWrapper;
    private readonly IStaticBitmapWrapperWindow? _arrowPoint;
    private readonly IWindow? _arrowPointWindow;
    private bool _flipped;
    private string _figure = "";
    private int _lastWidthFactor;
    private readonly List<int> _confirmationIds = new();

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::IlluminaChatBubbleWidget
    public IlluminaChatBubbleWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;

        // @see IlluminaChatBubbleWidget.as — build root from illumina_chat_bubble_xml asset
        object? xmlAsset = windowManager.FindAssetByName("illumina_chat_bubble_xml");
        if (xmlAsset is Core.Assets.IAsset { Content: System.Xml.Linq.XElement xml })
        {
            _container = ((IWindowFactory)windowManager).BuildFromXml(xml) as IWindowContainer;
        }

        if (_container == null)
        {
            return;
        }

        IWindow? containerWin = _container;
        _messageContainer = containerWin.FindChildByName("message_container") as IItemListWindow;
        _spacedMessageContainer = containerWin.FindChildByName("spaced_message_container") as IItemListWindow;
        _userNameLabel = containerWin.FindChildByName("user_name");
        _bubbleWrapper = containerWin.FindChildByName("bubble_wrapper");
        _arrowPointWindow = containerWin.FindChildByName("arrow_point");
        _arrowPoint = _arrowPointWindow as IStaticBitmapWrapperWindow;

        // @see IlluminaChatBubbleWidget.as — extract message template from list
        if (_messageContainer != null)
        {
            IWindow? templateObj = (_messageContainer as IWindow)?.FindChildByName("message_template");
            _messageTemplateWindow = templateObj;
            _messageTemplate = templateObj as ITextWindow;

            if (templateObj != null)
            {
                _messageContainer.RemoveListItem(templateObj);
            }
        }

        // @see IlluminaChatBubbleWidget.as — extract avatar parent
        IWindow? avatarWidget = containerWin.FindChildByName("user_avatar");

        if (avatarWidget != null)
        {
            _avatarParent = avatarWidget.parent;
        }

        // @see IlluminaChatBubbleWidget.as — initialize defaults
        if (_userNameLabel != null)
        {
            _userNameLabel.caption = ":";
        }

        containerWin.procedure = RootProcedure;
        _widgetWindow.RootWindow(_container);
        _widgetWindow.SetParamFlag(147456);
        containerWin.width = _widgetWindow.width;
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::flipped
    bool IIlluminaChatBubbleWidget.Flipped
    {
        get => _flipped;
        set
        {
            _flipped = value;

            Refresh();
        }
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::userName
    string? IIlluminaChatBubbleWidget.UserName
    {
        get => _userNameLabel?.caption is { Length: > 1 } s ? s[..^1] : "";
        set
        {
            if (value != null && _userNameLabel != null)
            {
                _userNameLabel.caption = value + ":";
            }
        }
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::userId
    int IIlluminaChatBubbleWidget.UserId { get; set; }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::figure
    string? IIlluminaChatBubbleWidget.Figure
    {
        get => _figure;
        set
        {
            if (value != null)
            {
                _figure = value;
                // TODO(window-port): Set figure on avatar widget when available
            }
        }
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::timeStamp
    double IIlluminaChatBubbleWidget.TimeStamp { get; set; }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::friendOnlineStatus
    bool IIlluminaChatBubbleWidget.FriendOnlineStatus
    {
        set
        {
            // TODO(window-port): Update offline placeholder height
        }
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::numMessages
    int IIlluminaChatBubbleWidget.NumMessages
    {
        get
        {
            if (_messageContainer == null)
            {
                return 0;
            }

            return _messageContainer.NumListItems;
        }
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::getMessage
    string? IIlluminaChatBubbleWidget.GetMessage(int index)
    {
        if (_messageContainer == null)
        {
            return null;
        }

        if (_messageContainer.GetListItemAt(index) is IWindow item)
        {
            return item.caption;
        }

        return null;
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::setMessage
    void IIlluminaChatBubbleWidget.SetMessage(int index, string message)
    {
        if (_messageContainer == null)
        {
            return;
        }

        // @see IlluminaChatBubbleWidget.as — grow list if needed
        int numMessages = _messageContainer.NumListItems;

        while (index >= numMessages)
        {
            // TODO(window-port): Clone message template and add to list
            _confirmationIds.Add(0);
            numMessages++;
        }

        if (_messageContainer.GetListItemAt(index) is IWindow item)
        {
            item.caption = message;
        }

        Refresh();
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::appendMessage
    void IIlluminaChatBubbleWidget.AppendMessage(string message, bool newLine, int confirmationId)
    {
        if (_messageContainer == null)
        {
            return;
        }

        int numMessages = _messageContainer.NumListItems;
        int targetIndex;

        if (newLine)
        {
            targetIndex = 0;
            // TODO(window-port): Clone message template and add at index 0
            _confirmationIds.Insert(targetIndex, 0);
        }
        else
        {
            targetIndex = numMessages;
        }

        // @see IlluminaChatBubbleWidget.as — set message and confirmation
        ((IIlluminaChatBubbleWidget)this).SetMessage(targetIndex, message);
        SetAwaitingConfirmationIdInternal(targetIndex, confirmationId);
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::setAwaitingConfirmationId
    void IIlluminaChatBubbleWidget.SetAwaitingConfirmationId(int messageIndex, int confirmationId)
    {
        SetAwaitingConfirmationIdInternal(messageIndex, confirmationId);
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::clearAwaitingConfirmationId
    void IIlluminaChatBubbleWidget.ClearAwaitingConfirmationId(int messageIndex)
    {
        if (_messageContainer == null)
        {
            return;
        }

        if (_messageContainer.GetListItemAt(messageIndex) is ITextWindow textWin)
        {
            textWin.TextColor = 0;
        }

        if (messageIndex >= 0 && messageIndex < _confirmationIds.Count)
        {
            _confirmationIds[messageIndex] = 0;
        }
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::getAwaitingConfirmationId
    int IIlluminaChatBubbleWidget.GetAwaitingConfirmationId(int messageIndex)
    {
        if (messageIndex >= 0 && messageIndex < _confirmationIds.Count)
        {
            return _confirmationIds[messageIndex];
        }

        return 0;
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (_container is IWindow containerWin)
        {
            containerWin.Destroy();
            _container = null;
        }

        if (_widgetWindow != null)
        {
            _widgetWindow.RootWindow(null);
            _widgetWindow = null;
        }

        _windowManager = null;
        _messageContainer = null;
        _spacedMessageContainer = null;
        _confirmationIds.Clear();
        disposed = true;
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::refresh
    private void Refresh()
    {
        if (_resizing || _container == null || _widgetWindow == null || _bubbleWrapper == null || _avatarParent == null)
        {
            return;
        }

        _resizing = true;

        IWindow? containerWin = (IWindow)_container;

        // @see IlluminaChatBubbleWidget.as — set height from bubble bottom
        containerWin.height = _bubbleWrapper.bottom;

        // @see IlluminaChatBubbleWidget.as — size bubble wrapper excluding avatar
        _bubbleWrapper.width = containerWin.width - _avatarParent.width;

        // @see IlluminaChatBubbleWidget.as — resize message widths if container width changed
        int widthFactor = (int)(_bubbleWrapper.width / RESIZING_OFFSETS);
        if (widthFactor != _lastWidthFactor && _messageContainer != null)
        {
            int numItems = _messageContainer.NumListItems;
            for (int i = 0;
                 i < numItems;
                 i++)
            {
                if (_messageContainer.GetListItemAt(i) is IWindow messageItem)
                {
                    messageItem.width = _bubbleWrapper.width - 5;
                }
            }
            _lastWidthFactor = widthFactor;
        }

        // @see IlluminaChatBubbleWidget.as — set list widths
        if (_messageContainer is IWindow msgWin)
        {
            msgWin.width = _bubbleWrapper.width;
        }
        if (_spacedMessageContainer is IWindow spacedWin)
        {
            spacedWin.width = _bubbleWrapper.width;
        }

        // @see IlluminaChatBubbleWidget.as — position avatar and arrow based on flip
        if (_flipped)
        {
            _avatarParent.x = containerWin.width - _avatarParent.width;
            if (_arrowPointWindow != null)
            {
                _arrowPointWindow.x = _avatarParent.x;
            }
            _bubbleWrapper.x = 0;
        }
        else
        {
            _avatarParent.x = 0;
            if (_arrowPointWindow != null)
            {
                _arrowPointWindow.x = _avatarParent.right - (_arrowPointWindow.width);
            }
            _bubbleWrapper.x = _avatarParent.right;
        }

        _arrowPointWindow?.Invalidate();

        _resizing = false;
    }

    /// @see habbo/window/widgets/IlluminaChatBubbleWidget.as::rootProcedure
    private void RootProcedure(WindowEvent param1, IWindow param2)
    {
        switch (param1.type)
        {
            case "WE_RESIZED":
            case "WE_CHILD_RESIZED":
                Refresh();
                break;
        }
    }

    private void SetAwaitingConfirmationIdInternal(int index, int confirmationId)
    {
        if (_messageContainer == null)
        {
            return;
        }

        if (_messageContainer.GetListItemAt(index) is ITextWindow textWin)
        {
            textWin.TextColor = confirmationId > 0 ? (uint)9079434 : (uint)0;
        }

        while (index >= _confirmationIds.Count)
        {
            _confirmationIds.Add(0);
        }

        _confirmationIds[index] = confirmationId;
    }
}
