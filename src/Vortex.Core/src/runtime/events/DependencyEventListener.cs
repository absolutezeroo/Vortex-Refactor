// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentDependency.as

using System;

namespace Vortex.Core.Runtime.Events;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentDependency.as::eventListeners
public sealed class DependencyEventListener
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentDependency.as::eventListeners
    public DependencyEventListener(string type, Action<object?> callback)
    {
        this.type = type;
        this.callback = callback;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentDependency.as::eventListeners
    public string type { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentDependency.as::eventListeners
    public Action<object?> callback { get; }
}
