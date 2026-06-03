# Vortex.IID

~49 marker interfaces for type-safe dependency injection. Each IID is an empty sealed class extending `Core.Runtime.IID`, used as a typed key for component dependency resolution.

## SDK

`Microsoft.NET.Sdk` — References `GodotSharp`/`Godot.SourceGenerators` 4.6.1.
Depends on: `Vortex.Core` (for `IID` base interface only)

## Pattern

Every file follows the same structure:

```csharp
namespace Vortex.IID;

public class IIDCoreCommunicationManager : Core.Runtime.IID
{
    public IIDCoreCommunicationManager() { }
}
```

## All IID Types (49)

**Core System (3):**
IIDCoreCommunicationManager, IIDCoreLocalizationManager, IIDCoreWindowManager

**Avatar & Room (6):**
IIDAvatarRenderManager, IIDRoomEngine, IIDRoomManager, IIDRoomObjectFactory, IIDRoomObjectVisualizationFactory, IIDRoomRendererFactory

**Habbo Communication & Config (5):**
IIDHabboCommunicationManager, IIDHabboConfigurationManager, IIDHabboLocalizationManager, IIDSessionDataManager, IIDHabboWindowManager

**Habbo Features (35):**
IIDHabboAdManager, IIDHabboAvatarEditor, IIDHabboCampaigns, IIDHabboCatalog, IIDHabboClubCenter, IIDHabboEpicPopupView, IIDHabboFreeFlowChat, IIDHabboFriendBar, IIDHabboFriendBarData, IIDHabboFriendBarView, IIDHabboFriendList, IIDHabboGameManager, IIDHabboGroupForumController, IIDHabboGroupsManager, IIDHabboHelp, IIDHabboInventory, IIDHabboLandingView, IIDHabboMessenger, IIDHabboModeration, IIDHabboNavigator, IIDHabboNewNavigator, IIDHabboNotifications, IIDHabboNuxDialogs, IIDHabboPhoneNumber, IIDHabboQuestEngine, IIDHabboRoomSessionManager, IIDHabboRoomUI, IIDHabboSoundManager, IIDHabboTalent, IIDHabboToolbar, IIDHabboTracking, IIDHabboUserDefinedRoomEvents, IIDCollectiblesController, IIDVaultController, IIDWiredMenuController

## Usage in Components

```csharp
// Declaring a dependency (in a Component subclass)
new ComponentDependency(
    new IIDHabboConfigurationManager(),
    param => _config = param as IHabboConfigurationManager
)

// Registering as a provider
RegisterInterface(new IIDHabboConfigurationManager(), this);
```

## Structure

All 49 files are flat in `src/` — no subdirectories. Zero business logic.