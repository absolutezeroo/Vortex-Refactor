// @see com.sulake.habbo.session.HabboGroupInfoManager

using Vortex.Core.Communication.Messages;
using Vortex.Core.Runtime;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Session;
using Vortex.Habbo.Communication.Messages.Incoming.Users;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.HabboGroupInfoManager
public class HabboGroupInfoManager : IDisposable
{
    private readonly SessionDataManager _sessionDataManager;
    private readonly Dictionary<int, string> _groupBadges = new();
    private IMessageEvent? _roomReadyEvent;
    private IMessageEvent? _groupBadgesEvent;

    /// @see HabboGroupInfoManager.as::HabboGroupInfoManager
    public HabboGroupInfoManager(SessionDataManager sessionDataManager)
    {
        _sessionDataManager = sessionDataManager;

        var comm = _sessionDataManager.communication;
        if (comm != null)
        {
            _roomReadyEvent = comm.AddHabboConnectionMessageEvent(
                new RoomReadyMessageEvent(OnRoomReady));
            _groupBadgesEvent = comm.AddHabboConnectionMessageEvent(
                new HabboGroupBadgesMessageEvent(OnHabboGroupBadges));
        }
    }

    public bool disposed => _sessionDataManager == null!;

    /// @see HabboGroupInfoManager.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        var comm = _sessionDataManager.communication;
        if (comm != null)
        {
            if (_roomReadyEvent != null)
            {
                comm.RemoveHabboConnectionMessageEvent(_roomReadyEvent);
                _roomReadyEvent = null;
            }
            if (_groupBadgesEvent != null)
            {
                comm.RemoveHabboConnectionMessageEvent(_groupBadgesEvent);
                _groupBadgesEvent = null;
            }
        }
    }

    /// @see HabboGroupInfoManager.as::getBadgeId
    public string? GetBadgeId(int groupId)
    {
        return _groupBadges.TryGetValue(groupId, out string? badge) ? badge : null;
    }

    // --- Private handlers ---

    /// @see HabboGroupInfoManager.as::onRoomReady
    /// TODO(communication): Send GetHabboGroupBadgesMessageComposer once ported.
    private void OnRoomReady(IMessageEvent param1)
    {
        // TODO(communication): _sessionDataManager.Send(new GetHabboGroupBadgesMessageComposer());
    }

    /// @see HabboGroupInfoManager.as::onHabboGroupBadges
    private void OnHabboGroupBadges(IMessageEvent param1)
    {
        HabboGroupBadgesMessageEvent ev = (HabboGroupBadgesMessageEvent)param1;
        foreach (KeyValuePair<int, string> pair in ev.badges)
        {
            _groupBadges[pair.Key] = pair.Value;
        }
    }
}
