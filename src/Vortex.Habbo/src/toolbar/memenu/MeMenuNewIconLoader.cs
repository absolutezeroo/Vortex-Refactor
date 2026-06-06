// @see com.sulake.habbo.toolbar.memenu.MeMenuNewIconLoader

using System;

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Avatar;
using Vortex.Habbo.Communication.Messages.Incoming.Handshake;

namespace Vortex.Habbo.Toolbar.MeMenu;

/// @see com.sulake.habbo.toolbar.memenu.MeMenuNewIconLoader
public class MeMenuNewIconLoader : IAvatarImageListener
{
    private const int MAX_ICON_HEIGHT = 50;
    private const int HEAD_MARGIN = 3;

    private HabboToolbar? _toolbar;
    private IConnection? _connection;
    private UserObjectEvent? _userObjectEvent;
    private IAvatarImage? _avatarImage;

    // TODO(as3-port): class_199 (FigureUpdateMessageEvent) — not ported yet

    /// @see MeMenuNewIconLoader.as::MeMenuNewIconLoader
    public MeMenuNewIconLoader(HabboToolbar toolbar)
    {
        _toolbar = toolbar;
        _connection = toolbar.communication?.connection;

        if (_connection != null)
        {
            _userObjectEvent = new UserObjectEvent(OnUserObject);
            _connection.AddMessageEvent(_userObjectEvent);
            // TODO(as3-port): register class_199 (FigureUpdateMessageEvent) when ported
        }
    }

    /// @see MeMenuNewIconLoader.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (_connection != null && _userObjectEvent != null)
        {
            _connection.RemoveMessageEvent(_userObjectEvent);
            _userObjectEvent = null;
        }

        _avatarImage?.Dispose();
        _avatarImage = null;
        _connection = null;
        _toolbar = null;
    }

    /// @see MeMenuNewIconLoader.as::get disposed
    public bool disposed => _toolbar == null;

    /// @see MeMenuNewIconLoader.as::avatarImageReady — called when IAvatarRenderManager finishes rendering
    public void AvatarImageReady(string figure)
    {
        SetMeMenuToolbarIcon(figure);
    }

    private void OnUserObject(IMessageEvent ev)
    {
        if (ev is not UserObjectEvent userObj)
        {
            return;
        }

        SetMeMenuToolbarIcon(userObj.figure);
    }

    // @see MeMenuNewIconLoader.as::setMeMenuToolbarIcon
    private void SetMeMenuToolbarIcon(string? figure = null)
    {
        if (_toolbar == null || string.IsNullOrEmpty(figure))
        {
            return;
        }

        IAvatarRenderManager? avatarRenderManager = _toolbar.avatarRenderManager;

        if (avatarRenderManager == null)
        {
            return;
        }

        _avatarImage?.Dispose();

        // @see MeMenuNewIconLoader.as — create avatar image with "h" type and "s" direction
        _avatarImage = avatarRenderManager.CreateAvatarImage(figure, "h", null, this);

        if (_avatarImage == null)
        {
            return;
        }

        // @see MeMenuNewIconLoader.as — crop to MAX_ICON_HEIGHT with HEAD_MARGIN offset
        Godot.Image? raw = _avatarImage.GetImage("h", false);

        if (raw == null)
        {
            return;
        }

        // Crop to head region: top MAX_ICON_HEIGHT rows starting at HEAD_MARGIN
        int cropY = HEAD_MARGIN;
        int cropH = Math.Min(MAX_ICON_HEIGHT, raw.GetHeight() - cropY);

        if (cropH <= 0)
        {
            return;
        }

        Godot.Image cropped = Godot.Image.CreateEmpty(raw.GetWidth(), cropH, false, raw.GetFormat());
        cropped.BlitRect(raw, new Godot.Rect2I(0, cropY, raw.GetWidth(), cropH), Godot.Vector2I.Zero);

        _toolbar.SetIconBitmap(HabboToolbarIconEnum.ICON_MEMENU, cropped);
    }
}
