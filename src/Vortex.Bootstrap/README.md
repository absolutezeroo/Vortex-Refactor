# Vortex.Bootstrap

Component wiring layer that bridges AS3 bootstrap patterns with the Component-Context DI framework. Instantiates managers and feature modules, registering them as injectable IIDs.

## SDK

`Microsoft.NET.Sdk` — References `GodotSharp`/`Godot.SourceGenerators` 4.6.1.
Depends on: `Vortex.Core`, `Vortex.Habbo`, `Vortex.IID`

## Three Bootstrap Patterns

### Pattern A: Sealed Manager Wrapper
Minimal 1-line constructor passthrough for core managers.

```csharp
public sealed class CoreCommunicationManagerBootstrap : CoreCommunicationManager
{
    public CoreCommunicationManagerBootstrap(IContext p1, uint p2 = 0, object? p3 = null)
        : base(p1, p2, p3) { }
}
```

**Instances:** CoreCommunicationManager, HabboCommunicationManager, HabboConfigurationManager, HabboLocalizationManager, HabboWindowManagerComponent, AdManager, ModerationManager, AvatarRenderManager

### Pattern B: Feature Bootstrap
~15-line placeholder extending `HabboBootstrapComponentBase`. Declares provided/required IIDs.

```csharp
public sealed class HabboCatalogBootstrap : HabboBootstrapComponentBase
{
    public HabboCatalogBootstrap(IContext p1, uint p2 = 0, object? p3 = null) : base(p1, p2, p3)
    {
        // TODO(as3-port): Replace with HabboCatalog once migrated.
    }
}
```

**Instances:** 26 feature bootstraps (Catalog, Inventory, Navigator, Messenger, FriendList, Room*, etc.)

### Pattern C: Com/Lib Module
`SimpleApplication` subclass declaring `requiredClasses` metadata.

```csharp
public sealed partial class HabboCommunicationCom : SimpleApplication
{
    public static string[] requiredClasses =>
        ["HabboCommunicationManagerBootstrap", "IIDHabboCommunicationManager"];
}
```

**Instances:** 35 Com/Lib modules (metadata containers)

## Key Base Classes

- **`HabboBootstrapComponentBase`** — Abstract. Auto-registers `providedIIDs`, builds `ComponentDependency` list from `requiredIIDs`. Fires `"complete"` event on init.
- **`SimpleApplication`** — `Control` subclass. Caches manifest XML per class name. Lists `requiredClasses` for runtime discovery.

## Directory Structure

```
src/
  HabboBootstrapComponentBase.cs    Abstract base for feature modules
  SimpleApplication.cs              Base for manifest metadata
  *Bootstrap.cs                     34 bootstrap classes (8 sealed + 26 feature)
  *Com.cs / *Lib.cs                 35 manifest metadata modules
```

72 files total. All flat in `src/`.