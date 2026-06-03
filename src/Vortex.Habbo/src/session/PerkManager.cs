// @see com.sulake.habbo.session.PerkManager

using Vortex.Core.Communication.Messages;
using Vortex.Core.Runtime;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.PerkManager
public class PerkManager : IDisposable
{
    private readonly SessionDataManager _sessionDataManager;
    private readonly Dictionary<string, PerkEntry> _perks = new();

    // TODO(communication): Wire PerkAllowancesMessageEvent + Perk parser once ported.
    // private IMessageEvent? _perkAllowancesEvent;

    public PerkManager(SessionDataManager sessionDataManager)
    {
        _sessionDataManager = sessionDataManager;

        // TODO(communication): Register PerkAllowancesMessageEvent once ported:
        // var comm = _sessionDataManager.communication;
        // if (comm != null)
        //     _perkAllowancesEvent = comm.AddHabboConnectionMessageEvent(
        //         new PerkAllowancesMessageEvent(OnPerkAllowances));
    }

    public bool disposed => _sessionDataManager == null!;
    public bool isReady { get; private set; }

    /// @see PerkManager.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _perks.Clear();

        // TODO(communication): Remove PerkAllowancesMessageEvent once wired.
    }

    /// @see PerkManager.as::isPerkAllowed
    public bool IsPerkAllowed(string perk)
    {
        return _perks.TryGetValue(perk, out PerkEntry? entry) && entry.IsAllowed;
    }

    /// @see PerkManager.as::getPerkErrorMessage
    public string GetPerkErrorMessage(string perk)
    {
        return _perks.TryGetValue(perk, out PerkEntry? entry) ? entry.ErrorMessage : "";
    }

    /// @see PerkManager.as::onPerkAllowances
    /// TODO(communication): Uncomment once PerkAllowancesMessageEvent + Perk are ported.
    // private void OnPerkAllowances(IMessageEvent param1)
    // {
    //     var ev = (PerkAllowancesMessageEvent)param1;
    //     foreach (var perk in ev.GetParser().GetPerks())
    //         _perks[perk.code] = new PerkEntry(perk.isAllowed, perk.errorMessage);
    //     isReady = true;
    //     _sessionDataManager.events?.DispatchEvent(new PerksUpdatedEvent());
    // }

    /// Called externally once PerkAllowancesMessageEvent is ported.
    public void SetPerks(IEnumerable<(string code, bool isAllowed, string errorMessage)> perks)
    {
        foreach ((string code, bool isAllowed, string errorMessage) in perks)
        {
            _perks[code] = new PerkEntry(isAllowed, errorMessage);
        }
        isReady = true;
        _sessionDataManager.events?.DispatchEvent(new PerksUpdatedEvent());
    }

    private sealed record PerkEntry(bool IsAllowed, string ErrorMessage);
}
