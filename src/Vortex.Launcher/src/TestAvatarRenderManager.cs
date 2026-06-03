using System;
using System.Reflection;

using Vortex.Core.Runtime;
using Vortex.Habbo.Avatar;

namespace Vortex.Test;

/// <summary>
/// Test-only subclass of AvatarRenderManager that bypasses download manager dependencies.
/// Overrides <c>dependencies</c> to return empty (the base class's <c>_inNuxFlow</c> field
/// is set too late — after the Component base constructor already reads <c>dependencies</c>).
/// TriggerConfigurationComplete() invokes the real download pipeline via reflection.
/// </summary>
public class TestAvatarRenderManager(IContext context, uint param2 = 0, object? param3 = null) : AvatarRenderManager(context, param2, param3, true)
{
    /// Override to guarantee zero dependencies — the base class's inNuxFlow guard
    /// doesn't work because _inNuxFlow is set in AvatarRenderManager's constructor body,
    /// which runs AFTER Component's base constructor reads this property.
    protected override IList<ComponentDependency> dependencies => Array.Empty<ComponentDependency>();

    /// Triggers the real download pipeline by invoking the private OnConfigurationComplete method.
    /// This creates download managers, loads figuremap, downloads .vortex bundles, and sets
    /// readiness flags naturally through the normal event flow.
    public void TriggerConfigurationComplete()
    {
        MethodInfo? method = typeof(AvatarRenderManager).GetMethod(
            "OnConfigurationComplete", BindingFlags.NonPublic | BindingFlags.Instance
        );
        method?.Invoke(this, [null]);
    }

    private void SetPrivateField(string name, object value)
    {
        FieldInfo? field = typeof(AvatarRenderManager).GetField(
            name, BindingFlags.NonPublic | BindingFlags.Instance
        );
        field?.SetValue(this, value);
    }
}
