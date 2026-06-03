using System;
using System.Linq;

using Vortex.Core.Runtime;

namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::prepareCore
public abstract class HabboBootstrapComponentBase : Component
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::prepareCore
    protected HabboBootstrapComponentBase(IContext param1, uint param2 = 0, object? param3 = null)
        : base(param1, param2, param3)
    {
        foreach (Core.Runtime.IID iid in providedIIDs)
        {
            RegisterInterface(iid, this);
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::prepareCore
    public bool isInitialized { get; private set; }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::prepareCore
    protected virtual IList<Core.Runtime.IID> providedIIDs => Array.Empty<Core.Runtime.IID>();

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::prepareCore
    protected virtual IList<Core.Runtime.IID> requiredIIDs => Array.Empty<Core.Runtime.IID>();

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::prepareCore
    protected virtual bool dependencyIsRequired => false;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::prepareCore
    protected override IList<ComponentDependency> dependencies => CreateDependencies(requiredIIDs, dependencyIsRequired);

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::prepareCore
    protected override void InitComponent()
    {
        isInitialized = true;

        events.DispatchEvent("complete");
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::prepareCore
    private static IList<ComponentDependency> CreateDependencies(IList<Core.Runtime.IID> param1, bool param2)
    {
        List<ComponentDependency> result = new(param1.Count);

        result.AddRange(param1.Select(iid => new ComponentDependency(iid, AssignDependency, param2)));

        return result;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::prepareCore
    private static void AssignDependency(object? param1)
    {
        _ = param1;
    }
}
