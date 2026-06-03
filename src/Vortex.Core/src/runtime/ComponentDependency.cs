// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentDependency.as

using System;

using Vortex.Core.Runtime.Events;

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentDependency.as
public class ComponentDependency
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentDependency.as::ComponentDependency
    public ComponentDependency(IID param1, Action<object?>? param2, bool param3 = true, IList<DependencyEventListener>? param4 = null)
    {
        identifier = param1;
        dependencySetter = param2;
        isRequired = param3;
        eventListeners = param4;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentDependency.as::get identifier
    internal IID identifier { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentDependency.as::get dependencySetter
    internal Action<object?>? dependencySetter { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentDependency.as::get isRequired
    internal bool isRequired { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentDependency.as::get eventListeners
    internal IList<DependencyEventListener>? eventListeners { get; }
}
