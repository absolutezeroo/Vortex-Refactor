// @see com.sulake.habbo.session.IgnoredUsersManager

using Vortex.Core.Communication.Messages;
using Vortex.Core.Runtime;
using Vortex.Habbo.Communication.Messages.Incoming.Users;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.IgnoredUsersManager
public class IgnoredUsersManager : IDisposable
{
    private readonly SessionDataManager _sessionDataManager;
    private IMessageEvent? _ignoreResultEvent;

    // TODO(communication): Wire IgnoredUsersMessageEvent once ported
    // (parser reads a string array of ignored usernames).
    // private IMessageEvent? _ignoredUsersEvent;

    private readonly List<string> _ignoredUsers = new();

    /// @see IgnoredUsersManager.as::IgnoredUsersManager
    public IgnoredUsersManager(SessionDataManager sessionDataManager)
    {
        _sessionDataManager = sessionDataManager;

        var comm = _sessionDataManager.communication;
        if (comm != null)
        {
            _ignoreResultEvent = comm.AddHabboConnectionMessageEvent(
                new IgnoreResultMessageEvent(OnIgnoreResult));

            // TODO(communication): Register IgnoredUsersMessageEvent once ported:
            // _ignoredUsersEvent = comm.AddHabboConnectionMessageEvent(
            //     new IgnoredUsersMessageEvent(OnIgnoreList));
        }
    }

    public bool disposed => _sessionDataManager == null!;

    /// @see IgnoredUsersManager.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        var comm = _sessionDataManager.communication;
        if (comm != null)
        {
            if (_ignoreResultEvent != null)
            {
                comm.RemoveHabboConnectionMessageEvent(_ignoreResultEvent);
                _ignoreResultEvent = null;
            }

            // TODO(communication): Remove IgnoredUsersMessageEvent once wired.
        }
    }

    /// @see IgnoredUsersManager.as::initIgnoreList
    /// TODO(communication): Send GetIgnoredUsersMessageComposer once ported.
    public void InitIgnoreList()
    {
        // TODO(communication): _sessionDataManager.Send(new GetIgnoredUsersMessageComposer(_sessionDataManager.userName));
    }

    /// @see IgnoredUsersManager.as::isIgnored
    public bool IsIgnored(string userName)
    {
        return _ignoredUsers.Contains(userName);
    }

    /// @see IgnoredUsersManager.as::ignoreUser
    /// TODO(communication): Send IgnoreUserMessageComposer once ported.
    public void IgnoreUser(string userName)
    {
        // TODO(communication): _sessionDataManager.Send(new IgnoreUserMessageComposer(userName));
    }

    /// @see IgnoredUsersManager.as::unignoreUser
    /// TODO(communication): Send UnignoreUserMessageComposer once ported.
    public void UnignoreUser(string userName)
    {
        // TODO(communication): _sessionDataManager.Send(new UnignoreUserMessageComposer(userName));
    }

    /// @see IgnoredUsersManager.as::ignoreUserId
    /// TODO(communication): Send IgnoreUserIdMessageComposer once ported.
    public void IgnoreUserId(int userId)
    {
        // TODO(communication): _sessionDataManager.Send(new IgnoreUserIdMessageComposer(userId));
    }

    // --- Private handlers ---

    /// @see IgnoredUsersManager.as::onIgnoreList
    /// TODO(communication): Uncomment once IgnoredUsersMessageEvent is ported.
    // private void OnIgnoreList(IMessageEvent param1)
    // {
    //     var ev = (IgnoredUsersMessageEvent)param1;
    //     _ignoredUsers.Clear();
    //     _ignoredUsers.AddRange(ev.ignoredUsers);
    // }

    /// @see IgnoredUsersManager.as::onIgnoreResult
    private void OnIgnoreResult(IMessageEvent param1)
    {
        IgnoreResultMessageEvent ev = (IgnoreResultMessageEvent)param1;
        string name = ev.name;

        switch (ev.result)
        {
            case 0:
                return;
            case 1:
                AddToIgnoreList(name);
                return;
            case 2:
                AddToIgnoreList(name);
                if (_ignoredUsers.Count > 0)
                {
                    _ignoredUsers.RemoveAt(0);
                }

                return;
            case 3:
                RemoveFromIgnoreList(name);
                return;
        }
    }

    private void AddToIgnoreList(string userName)
    {
        if (!_ignoredUsers.Contains(userName))
        {
            _ignoredUsers.Add(userName);
        }
    }

    private void RemoveFromIgnoreList(string userName)
    {
        _ignoredUsers.Remove(userName);
    }
}
