# Project Gaps

All known TODOs and missing implementations, grouped by system.
Keeping this file up to date avoids re-scanning the codebase each session.

---

## 1. Systèmes non portés (Bootstrap stubs)

Ces bootstraps existent mais leur classe AS3 parent n'est pas encore portée.
Chacun contient `// TODO(as3-port): Parent AS3 class "X" is not ported yet.`

| Bootstrap | Classe AS3 manquante |
|-----------|----------------------|
| `HabboFreeFlowChatBootstrap` | `HabboFreeFlowChat` |
| `HabboGroupsManagerBootstrap` | `HabboGroupsManager` |
| `HabboFriendListBootstrap` | `HabboFriendList` |
| `HabboMessengerBootstrap` | `HabboMessenger` |
| `HabboInventoryBootstrap` | `HabboInventory` |
| `HabboHelpBootstrap` | `HabboHelp` |
| `HabboGameManagerBootstrap` | `HabboGameManager` |
| `HabboNewNavigatorBootstrap` | `HabboNewNavigator` |
| `HabboFriendBarBootstrap` | `HabboFriendBar` |
| `HabboNotificationsBootstrap` | `HabboNotifications` |
| `HabboQuestEngineBootstrap` | `HabboQuestEngine` |
| `HabboCatalogBootstrap` | `HabboCatalog` |
| `HabboNavigatorBootstrap` | `HabboNavigator` |
| `HabboAvatarEditorManagerBootstrap` | `HabboAvatarEditorManager` |
| `HabboSoundManagerFlash10Bootstrap` | `HabboSoundManagerFlash10` |
| `HabboToolbarBootstrap` | `HabboToolbar` |
| `HabboTrackingBootstrap` | `HabboTracking` |
| `ModerationManagerBootstrap` | `ModerationManager` |
| `AdManagerBootstrap` | `AdManager` |
| `HabboUserDefinedRoomEventsBootstrap` | `HabboUserDefinedRoomEvents` |
| `RoomSessionManagerBootstrap` | `RoomSessionManager` |
| `RoomUIBootstrap` | `RoomUI` |
| `SessionDataManagerBootstrap` | `SessionDataManager` |

**Libs sans manifest mappé** : `HabboRoomObjectLogicLib`, `HabboRoomObjectVisualizationLib`,
`HabboRoomSessionManagerLib`, `HabboAvatarRenderLib`, `HabboSessionDataManagerLib`,
`HabboTrackingLib`, `CoreCommunicationFrameworkLib`, `RoomSpriteRendererLib`, `RoomManagerLib`

---

## 2. Managers bloquants (IID non injectables)

Ces interfaces sont déclarées dans `IRoomEngineServices` / `IRoomCreator` mais les implémentations
n'existent pas encore. Elles bloquent en cascade les points 3–5 ci-dessous.

- `IRoomSessionManager` — bloque guide markers, special room effects, session-based room logic
- `ISessionDataManager` — bloque `OnIgnoreResult`, `RoomContentLoader.getFurniData`, user data lookups
- `IHabboWindowManager` (injection dans room engine) — bloque alerts et overlays
- `IHabboToolbar` — bloque toolbar updates depuis la room
- `IHabboCatalog` — bloque ouverture catalogue depuis la room
- `IHabboGameManager` — bloque `IRoomEngineServices.GameManager`
- `IHabboAdManager` — bloque `IRoomEngineServices.AdManager`
- `IRoomAreaSelectionManager` — déclaré dans `IRoomEngine.cs:30` mais non porté

---

## 3. Room Engine

### 3a. Handlers bloqués dans `RoomMessageHandler.cs`
- `OnSpecialRoomEvent` cases 0/1/2 — `RoomRotatingEffect`, `RoomShakingEffect`, `IRoomSessionManager.Events`
- `UpdateGuideMarker` / `SetUserGuideStatus` — `IRoomSessionManager` + `IUserDataManager`
- `OnIgnoreResult` — `sessionDataManager.GetSession().UserDataManager.GetUserDataByName()`

### 3b. Message event non porté
- `AreaHideMessageEvent` — référencé dans `RoomMessageHandler` (TODO), source AS3 disponible dans `WIN63-202407091256`

### 3c. `RoomObjectEventHandler.cs`
Nombreux composers et managers non portés (l.159, 321, 393, 602–638, 869, 947–949, 1060, 1307, 1486–1489, 1627, 1680, 1873, 1953).

### 3d. `RoomEngine.cs`
- ~45 TODOs pour managers non injectés (l.56–2813 en intermittent)
- `GetRenderRoomMessage()` (`IRoomEngine.cs:224`) — composer non porté

### 3e. Logiques furniture manquantes
- `FurnitureAreaHideLogic` — `RoomObjectFactory.cs:120` utilise `FurnitureMultiStateLogic` comme fallback
- `FurnitureWildwestWantedLogic` — idem (`RoomObjectFactory.cs:126`)

### 3f. `RoomObjectVisualizationFactory.cs`
- ~15 classes de visualisation non portées (l.87–237)

### 3g. `SpriteDataCollector.cs`
- 7 méthodes non implémentées (l.87–140) : collect sprites, background plane, z-sort, JSON serialization

### 3h. `RoomContentLoader.cs:829`
- `getFurniData(this)` — bloqué sur `ISessionDataManager`

---

## 4. Communication — Composers manquants

| Composer | Référencé depuis |
|----------|-----------------|
| `GetHabboGroupDetailsMessageComposer` | `BadgeImageWidget.cs:386`, `RoomObjectEventHandler` |
| `GetExtendedProfileMessageComposer` | `AvatarImageWidget.cs:391` |
| `ElementPointerMessageEvent` | `ElementPointerHandler.cs:21` (register + remove) |
| Plusieurs composers room action | `RoomObjectEventHandler.cs` (multiples lignes) |

---

## 5. Avatar

- **`GetFigureStringWithFigureIds()`** (`AvatarRenderManager.cs:503`) — stub retourne `param1` tel quel.
  Nécessite `FigureDataContainer` (`com.sulake.habbo.utils`).
- **`AvatarFurnitureVisualizationData`** (`l.14, 33`) — délégation à `AvatarVisualizationData.GetAvatar()` non implémentée (affichage avatar sur furniture mannequin).
- **`AvatarVisualization.cs:290`** — highlight/glow filters (équivalent shader Godot non fait).

---

## 6. Window / Widgets

| Fichier | Ligne | Description |
|---------|-------|-------------|
| `FurnitureImageWidget.cs` | 211 | `roomEngine.getFurnitureImage/getWallItemImage` — bloqué sur room engine |
| `PetImageWidget.cs` | 117 | Rendu image pet — bloqué sur room engine |
| `BadgeImageWidget.cs` | 386 | `GetHabboGroupDetailsMessageComposer` non porté |
| `AvatarImageWidget.cs` | 391 | `GetExtendedProfileMessageComposer` non porté |
| `IlluminaChatBubbleWidget.cs` | 138,151,198,225 | Clone templates, figure avatar widget, placeholder height |
| `LimitedItemPreviewOverlayWidget.cs` | 36,44 | `LimitedItemOverlayNumberBitmapGenerator` non porté |
| `LimitedItemGridOverlayWidget.cs` | 61 | Animation shine non implémentée |
| `RunningNumberWidget.cs` | 80 | Affichage visuel des chiffres non mis à jour |
| `RarityItemPreviewOverlayWidget.cs` | 36 | Bitmap rarity overlay non mis à jour |
| `RarityItemGridOverlayWidget.cs` | 36 | Idem |
| `UpdatingTimeStampWidget.cs` | 53,91 | Text alignment + format "time ago" (localization) |
| `CountdownWidget.cs` | 236,240 | Clone separator/counter templates |
| `HabboWindowManagerComponent.cs` | 2 | Runtime wiring : renderer/theme, profiler, floorplan/help viewers |

### Scaffolds (implémentation vide à faire)
- `HabboPagesViewer.cs`
- `ImportExportDialog.cs`
- `HeightMapEditor.cs`
- `FloorPlanPreviewer.cs`
- `FloorPlanCache.cs`
- `BCFloorPlanEditor.cs`

---

## 7. Texte / Font (`TextController.cs`)

- `l.601` — Intégration StyleSheet AS3
- `l.788` — `Clone()` retourne null silencieusement
- `l.1218` — Bounding box par glyphe (mesure de police)
- `l.1222` — Hit-testing au niveau du caractère
- `l.1303` — `TextLineMetrics` (ascent/descent/width)

---

## 8. Assets

- `LimitedItemOverlayNumberBitmapGenerator.cs:27` — chargement glyphes depuis `IAssetLibrary` non porté

---

## 9. Divers

- `Component.cs:337` — Trace sink pour unlock logging
- `CoreComponentContext.cs:112,360,705` — Profiler + ProfilerViewer non portés
- `ProfilerOutput.cs:61–135` — 8 méthodes profiler UI non câblées
- `HabboCommunicationDemo.cs:245` — `_windowManager.alert()` (utilise fallback null)
- `WindowToolTipAgent.cs:212` — `SceneTreeTimer` non annulable proprement

---

## Ordre de débloquage recommandé

```
SessionDataManager  ──┐
RoomSessionManager  ──┼──► déblocage room handlers (§3a) + ContentLoader (§3h)
                      │
IHabboWindowManager ──┘

AreaHideMessageEvent          ← actionnable maintenant (source AS3 dispo)
GetFigureStringWithFigureIds  ← actionnable maintenant (FigureDataContainer à vérifier)
ElementPointerMessageEvent    ← actionnable maintenant
GetExtendedProfileComposer    ← actionnable maintenant
GetHabboGroupDetailsComposer  ← actionnable maintenant
```
