// @see com.sulake.habbo.session.events.BadgeImageReadyEvent

using Godot;

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.BadgeImageReadyEvent
public class BadgeImageReadyEvent
{
    public const string BADGE_IMAGE_READY = "BadgeImageReadyEvent";

    public BadgeImageReadyEvent(string badgeId, Image image)
    {
        BadgeId = badgeId;
        Image = image;
        Type = BADGE_IMAGE_READY;
    }

    public string Type { get; }
    public string BadgeId { get; }
    public Image Image { get; }
}
