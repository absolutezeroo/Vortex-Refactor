// @see habbo/window/widgets/BadgeImageWidget.as

using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Communication.Messages;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;
using Vortex.Core.Window.Utils;
using Vortex.Habbo.Communication;
using Vortex.Habbo.Communication.Messages.Incoming.Users;
using Vortex.Habbo.Window.Enum;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/BadgeImageWidget.as
public class BadgeImageWidget : IBadgeImageWidget
{
    public const string TYPE = "badge_image";

    private const string TYPE_KEY = "badge_image:type";
    private const string BADGE_ID_KEY = "badge_image:badge_id";

    private static readonly PropertyStruct TYPE_DEFAULT =
        new(TYPE_KEY, Class3550.NORMAL, PropertyStruct.STRING, false, Class3550.ALL);

    private static readonly PropertyStruct ID_DEFAULT =
        new(BADGE_ID_KEY, "", PropertyStruct.STRING);

    private IWidgetWindow? _widgetWindow;
    private HabboWindowManagerComponent? _windowManager;
    private bool _inBulkUpdate;
    private IWindowContainer? _rootContainer;
    private IStaticBitmapWrapperWindow? _bitmap;
    private IWindow? _regionWindow;
    private string _type;
    private string _badgeId;
    private int _groupId;

    private GroupDetailsChangedMessageEvent? _groupDetailsEvent;
    private HabboGroupBadgesMessageEvent? _groupBadgesEvent;

    /// @see BadgeImageWidget.as::BadgeImageWidget
    public BadgeImageWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _type = (string)TYPE_DEFAULT.value!;
        _badgeId = (string)ID_DEFAULT.value!;

        _widgetWindow = widgetWindow;
        _windowManager = windowManager;

        object? xmlAsset = windowManager.FindAssetByName("badge_image_xml");
        if (xmlAsset is IAsset { Content: XElement xml })
        {
            _rootContainer = ((IWindowFactory)windowManager).BuildFromXml(xml) as IWindowContainer;
        }

        if (_rootContainer == null)
        {
            return;
        }

        _bitmap = _rootContainer?.GetChildByName("bitmap") as IStaticBitmapWrapperWindow;
        IRegionWindow? region = _rootContainer?.GetChildByName("region") as IRegionWindow;
        _regionWindow = region as IWindow;
        _regionWindow?.AddEventListener(WindowMouseEvent.CLICK, OnClick);

        _widgetWindow.RootWindow(_rootContainer);
        _rootContainer!.width = _widgetWindow.width;
        _rootContainer!.height = _widgetWindow.height;
    }

    /// @see BadgeImageWidget.as — helper to get IBitmapDataContainer from _bitmap
    private IBitmapDataContainer? BitmapContainer => _bitmap as IBitmapDataContainer;

    public bool disposed { get; private set; }

    /// @see BadgeImageWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see BadgeImageWidget.as::get type
    public string? Type
    {
        get => _type;
        set
        {
            _type = value ?? Class3550.NORMAL;
            Refresh();
        }
    }

    /// @see BadgeImageWidget.as::get badgeId
    public string? BadgeId
    {
        get => _badgeId;
        set
        {
            _badgeId = value ?? "";
            Refresh();
        }
    }

    /// @see BadgeImageWidget.as::get groupId
    public int GroupId
    {
        get => _groupId;
        set
        {
            _groupId = value;
            bool needsListeners = _type == Class3550.GROUP && _groupId > 0;

            if (_windowManager?.Communication() is IHabboCommunicationManager comm)
            {
                if (!needsListeners && _groupBadgesEvent != null)
                {
                    comm.RemoveHabboConnectionMessageEvent(_groupDetailsEvent!);
                    comm.RemoveHabboConnectionMessageEvent(_groupBadgesEvent);
                    _groupDetailsEvent = null;
                    _groupBadgesEvent = null;
                }
                else if (needsListeners && _groupBadgesEvent == null)
                {
                    _groupDetailsEvent = new GroupDetailsChangedMessageEvent(OnGroupDetailsChanged);
                    _groupBadgesEvent = new HabboGroupBadgesMessageEvent(OnHabboGroupBadges);
                    comm.AddHabboConnectionMessageEvent(_groupDetailsEvent);
                    comm.AddHabboConnectionMessageEvent(_groupBadgesEvent);
                }
            }
        }
    }

    /// @see BadgeImageWidget.as::get properties
    public PropertyStruct[] Properties
    {
        get
        {
            if (disposed)
            {
                return System.Array.Empty<PropertyStruct>();
            }

            List<PropertyStruct> result = new();
            result.Add(TYPE_DEFAULT.WithValue(_type));
            result.Add(ID_DEFAULT.WithValue(_badgeId));

            // @see BadgeImageWidget.as — forward bitmap properties with "badge_image" namespace,
            // skipping "asset_uri". AS3 calls _bitmap.properties which is StaticBitmapWrapperController.properties.
            // C# WindowController doesn't have a properties getter, so build manually from IBitmapDataContainer.
            IBitmapDataContainer? bdc = BitmapContainer;

            if (bdc == null)
            {
                return result.ToArray();
            }

            result.Add(new PropertyStruct("badge_image:pivot_point", bdc.PivotPoint, PropertyStruct.UINT));
            result.Add(new PropertyStruct("badge_image:stretched_x", bdc.StretchedX, PropertyStruct.BOOLEAN));
            result.Add(new PropertyStruct("badge_image:stretched_y", bdc.StretchedY, PropertyStruct.BOOLEAN));
            result.Add(new PropertyStruct("badge_image:zoom_x", bdc.ZoomX, PropertyStruct.NUMBER));
            result.Add(new PropertyStruct("badge_image:zoom_y", bdc.ZoomY, PropertyStruct.NUMBER));
            result.Add(new PropertyStruct("badge_image:greyscale", bdc.Greyscale, PropertyStruct.BOOLEAN));
            result.Add(new PropertyStruct("badge_image:etching_color", bdc.EtchingColor, PropertyStruct.UINT));
            result.Add(new PropertyStruct("badge_image:fit_size_to_contents", bdc.FitSizeToContents, PropertyStruct.BOOLEAN));
            result.Add(new PropertyStruct("badge_image:wrap_x", bdc.WrapX, PropertyStruct.BOOLEAN));
            result.Add(new PropertyStruct("badge_image:wrap_y", bdc.WrapY, PropertyStruct.BOOLEAN));
            result.Add(new PropertyStruct("badge_image:rotation", bdc.Rotation, PropertyStruct.NUMBER));

            return result.ToArray();
        }
        set
        {
            _inBulkUpdate = true;
            List<PropertyStruct> bitmapProps = new();

            foreach (PropertyStruct prop in value)
            {
                switch (prop.key)
                {
                    case TYPE_KEY:
                        _type = prop.value as string ?? Class3550.NORMAL;
                        break;
                    case BADGE_ID_KEY:
                        _badgeId = prop.value as string ?? "";
                        break;
                }

                if (prop.key != "badge_image:asset_uri")
                {
                    bitmapProps.Add(prop.WithoutNameSpace());
                }
            }

            // @see BadgeImageWidget.as — forward non-badge props to _bitmap via ApplyProperties
            if (_bitmap is BitmapDataController bdc)
            {
                bdc.ApplyProperties(bitmapProps.ToArray());
            }

            _inBulkUpdate = false;
            Refresh();
        }
    }

    // IBitmapDataContainer — delegates to BitmapContainer, with invalidation on set
    Image? IBitmapDataContainer.BitmapData => BitmapContainer?.BitmapData;

    uint IBitmapDataContainer.PivotPoint
    {
        get => BitmapContainer?.PivotPoint ?? 0;
        set
        {
            if (BitmapContainer != null) { BitmapContainer.PivotPoint = value; }
            (_bitmap as IWindow)?.Invalidate();
        }
    }

    bool IBitmapDataContainer.StretchedX
    {
        get => BitmapContainer?.StretchedX ?? false;
        set
        {
            if (BitmapContainer != null) { BitmapContainer.StretchedX = value; }
            (_bitmap as IWindow)?.Invalidate();
        }
    }

    bool IBitmapDataContainer.StretchedY
    {
        get => BitmapContainer?.StretchedY ?? false;
        set
        {
            if (BitmapContainer != null) { BitmapContainer.StretchedY = value; }
            (_bitmap as IWindow)?.Invalidate();
        }
    }

    float IBitmapDataContainer.ZoomX
    {
        get => BitmapContainer?.ZoomX ?? 1f;
        set
        {
            if (BitmapContainer != null) { BitmapContainer.ZoomX = value; }
            (_bitmap as IWindow)?.Invalidate();
        }
    }

    float IBitmapDataContainer.ZoomY
    {
        get => BitmapContainer?.ZoomY ?? 1f;
        set
        {
            if (BitmapContainer != null) { BitmapContainer.ZoomY = value; }
            (_bitmap as IWindow)?.Invalidate();
        }
    }

    bool IBitmapDataContainer.Greyscale
    {
        get => BitmapContainer?.Greyscale ?? false;
        set
        {
            if (BitmapContainer != null) { BitmapContainer.Greyscale = value; }
            (_bitmap as IWindow)?.Invalidate();
        }
    }

    uint IBitmapDataContainer.EtchingColor
    {
        get => BitmapContainer?.EtchingColor ?? 0;
        set
        {
            if (BitmapContainer != null) { BitmapContainer.EtchingColor = value; }
            (_bitmap as IWindow)?.Invalidate();
        }
    }

    /// @see BadgeImageWidget.as::get etchingPoint — fixed at (0, 1) per AS3
    Vector2 IBitmapDataContainer.EtchingPoint => new(0, 1);

    bool IBitmapDataContainer.FitSizeToContents
    {
        get => BitmapContainer?.FitSizeToContents ?? false;
        set
        {
            if (BitmapContainer != null) { BitmapContainer.FitSizeToContents = value; }
            (_bitmap as IWindow)?.Invalidate();
        }
    }

    /// @see BadgeImageWidget.as — wrapX/wrapY always false, rotation always 0 per AS3
    bool IBitmapDataContainer.WrapX { get => false; set { } }

    bool IBitmapDataContainer.WrapY { get => false; set { } }
    float IBitmapDataContainer.Rotation { get => 0f; set { } }

    /// @see BadgeImageWidget.as::get assetUri
    private string GetAssetUri()
    {
        if (string.IsNullOrEmpty(_badgeId))
        {
            return "";
        }

        switch (_type)
        {
            case Class3550.NORMAL:
                return "${image.library.url}album1584/" + _badgeId + ".png";

            case Class3550.GROUP:
                if (_windowManager == null)
                {
                    return "";
                }

                string url = _windowManager.GetProperty("group.badge.url");

                return url.Replace("%imagerdata%", _badgeId);

            case Class3550.PERK:
                return "${image.library.url}perk/" + _badgeId + ".png";

            default:
                return "";
        }
    }

    /// @see BadgeImageWidget.as::refresh
    private void Refresh()
    {
        if (_inBulkUpdate || disposed || _bitmap == null)
        {
            return;
        }

        _bitmap.AssetUri = GetAssetUri();
        (_bitmap as IWindow)?.Invalidate();
    }

    /// @see BadgeImageWidget.as::forceRefresh
    private void ForceRefresh(int groupId, string badgeId)
    {
        if (groupId != _groupId || _windowManager == null)
        {
            return;
        }

        _badgeId = badgeId;

        // @see BadgeImageWidget.as — removes cached asset before refresh
        ResourceManager? resourceManager = _windowManager.ResourceManager() as ResourceManager;
        resourceManager?.RemoveAsset(GetAssetUri());
        Refresh();
    }

    /// @see BadgeImageWidget.as::onGroupDetailsChanged
    private void OnGroupDetailsChanged(IMessageEvent param1)
    {
        GroupDetailsChangedMessageEvent ev = (GroupDetailsChangedMessageEvent)param1;
        ForceRefresh(ev.groupId, _badgeId);
    }

    /// @see BadgeImageWidget.as::onHabboGroupBadges
    private void OnHabboGroupBadges(IMessageEvent param1)
    {
        HabboGroupBadgesMessageEvent ev = (HabboGroupBadgesMessageEvent)param1;
        if (ev.badges.TryGetValue(_groupId, out string? badge))
        {
            ForceRefresh(_groupId, badge);
        }
    }

    /// @see BadgeImageWidget.as::onClick
    private void OnClick(WindowEvent param1, IWindow param2)
    {
        if (_groupId > 0 && _windowManager != null)
        {
            // TODO(communication): Send GetHabboGroupDetailsMessageComposer(_groupId, true) — composer not yet ported
        }
    }

    /// @see BadgeImageWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        // @see BadgeImageWidget.as — sets groupId=0 to clear message listeners
        GroupId = 0;

        if (_regionWindow != null)
        {
            _regionWindow.RemoveEventListener(WindowMouseEvent.CLICK, OnClick);
            _regionWindow.Destroy();
            _regionWindow = null;
        }

        _bitmap = null;

        if (_rootContainer != null)
        {
            (_rootContainer as IWindow)?.Destroy();
            _rootContainer = null;
        }

        if (_widgetWindow != null)
        {
            _widgetWindow.RootWindow(null);
            _widgetWindow = null;
        }

        _windowManager = null;
        disposed = true;
    }
}
