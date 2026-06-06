// @see com.sulake.habbo.toolbar.memenu.MeMenuIconLoader

using System;

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Avatar;
using Vortex.Habbo.Communication.Messages.Incoming.Handshake;

namespace Vortex.Habbo.Toolbar.MeMenu;

/// @see com.sulake.habbo.toolbar.memenu.MeMenuIconLoader (legacy toolbar system)
public class MeMenuIconLoader : IAvatarImageListener
{
    private const int MAX_ICON_HEIGHT = 50;
    private const int HEAD_MARGIN = 3;

    private HabboToolbar? _toolbar;
    private IConnection? _connection;
    private UserObjectEvent? _userObjectEvent;
    private IAvatarImage? _avatarImage;

    // TODO(as3-port): class_199 (FigureUpdateMessageEvent) — not ported yet

    /// @see MeMenuIconLoader.as::MeMenuIconLoader
    public MeMenuIconLoader(HabboToolbar toolbar)
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

    /// @see MeMenuIconLoader.as::dispose
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

    /// @see MeMenuIconLoader.as::get disposed
    public bool disposed => _toolbar == null;

    /// @see MeMenuIconLoader.as::avatarImageReady
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
        _avatarImage = avatarRenderManager.CreateAvatarImage(figure, "h", null, this);

        if (_avatarImage == null)
        {
            return;
        }

        Godot.Image? raw = _avatarImage.GetImage("h", false);

        if (raw == null)
        {
            return;
        }

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
