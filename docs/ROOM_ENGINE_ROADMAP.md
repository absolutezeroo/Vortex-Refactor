# Room Engine Porting Roadmap

Phased plan for porting the Habbo Room Engine from AS3 to Godot 4.6 + C#.

## Overview

| Metric | Value |
|--------|-------|
| AS3 scope | ~394 room files + ~106 communication files |
| AS3 LOC | ~36,500 (room) + ~4,400 (comm) = ~41,000 |
| Target C# files | ~470 |
| Estimated C# LOC | ~52,000 |
| Projects touched | Vortex.Room, Vortex.Habbo, Vortex.Bootstrap |
| Visual milestones | Phase 6, Phase 10, Phase 15 |

### Progress

| Phase | Name | Status |
|-------|------|--------|
| 1 | Math, Data, Pure Utilities | ✅ Completed |
| 2 | Interfaces, Events, Exceptions | ✅ Completed |
| 3 | Object Graph Core | ✅ Completed |
| 4 | Graphic Asset System | ✅ Completed |
| 5 | Room Instance + Room Manager | ✅ Completed |
| 6 | Renderer Infrastructure | ✅ Completed |
| 7 | Habbo Enums, StuffData, Visualization Data | ✅ Completed |
| 8 | Communication Messages | ✅ Completed |
| 9a | Logic — Core, Room, Avatar, Pet | ✅ Completed |
| 9b | Furniture Logic (Bulk) | ✅ Completed |
| 10 | Room Visualization + Rasterizer | ✅ Completed |
| 11a | Furniture Visualization (Bulk) | ✅ Completed |
| 11b | Avatar + Pet Visualization | ✅ Completed |
| 12 | RoomEngine Facade — Part 1 | ✅ Completed |
| 13 | RoomEngine Facade — Part 2 | ✅ Completed |
| 14 | Factories | ✅ Completed |
| 15 | Integration + Polish | ✅ Completed |

### AS3 Source Packages

| Package | Target Project | Files | LOC |
|---------|---------------|-------|-----|
| `com.sulake.room` (generic engine) | `src/Vortex.Room/` | 81 | ~9,400 |
| `com.sulake.habbo.room` (Habbo-specific) | `src/Vortex.Habbo/src/room/` | 313 | ~27,000 |
| `com.sulake.habbo.communication/messages/…/room/` | `src/Vortex.Habbo/src/communication/messages/` | ~106 | ~4,400 |

### Design Decisions

- **Rendering:** Software rendering via `Image` pixel manipulation (faithful BitmapData port, same pattern as WindowRenderer)
- **Missing dependencies:** Null guards — unported systems (SessionDataManager, Catalog, Toolbar) are null-checked, no stub implementations
- **Source hierarchy:** WIN63-202407 (primary) → WIN63-202111 (secondary, room-specific deobfuscation) → PRODUCTION-201611 (deobfuscation fallback)

---

## Dependency Graph

```
Phase 1 ──→ Phase 2 ──┬──→ Phase 3 ──→ Phase 5 ──→ Phase 12 ──→ Phase 13 ──→ Phase 15
					   │       │                         ↑              ↑           ↑
					   │       └──→ Phase 4 ──→ Phase 6 ─┘              │           │
					   │                │                               │           │
					   │                └──→ Phase 10 ──────────────────────────→ Phase 15
					   │                       ↑                                    ↑
					   └──→ Phase 7 ──┬──→ Phase 8 ──→ Phase 9a → 9b → Phase 14 ──┘
							   │      │                                    ↑
							   │      └──→ Phase 11a ─────────────────────┘
							   └──→ Phase 11b
```

**Critical path:** 1 → 2 → 3 → 5 → 12 → 13 → 15

**Parallelizable clusters after Phase 2:**
- Cluster A: {Phase 3, Phase 4, Phase 7} — object core, assets, enums/data
- Cluster B (after 3+4): {Phase 5, Phase 6} — manager + renderer
- Cluster C (after 7): {Phase 8, Phase 10, Phase 11a, Phase 11b} — comm, rasterizer, vis

---

## Phase 1: Math, Data, Pure Utilities ✅ COMPLETED

**Project:** `Vortex.Room/src/utils/`, `src/data/`, `src/object/enum/`
**Deps:** None
**Files:** 16 | **LOC:** ~1,550
**Testable:** Yes (pure math, unit-testable)

| AS3 File | C# File | LOC |
|----------|---------|-----|
| `room/utils/IVector3d.as` | `src/utils/IVector3d.cs` | 11 |
| `room/utils/Vector3d.as` | `src/utils/Vector3d.cs` | 215 |
| `room/utils/IRoomGeometry.as` | `src/utils/IRoomGeometry.cs` | 24 |
| `room/utils/RoomGeometry.as` | `src/utils/RoomGeometry.cs` | 436 |
| `room/utils/ColorConverter.as` | `src/utils/ColorConverter.cs` | 287 |
| `room/utils/PointMath.as` | `src/utils/PointMath.cs` | 25 |
| `room/utils/NumberBank.as` | `src/utils/NumberBank.cs` | 58 |
| `room/utils/RoomId.as` | `src/utils/RoomId.cs` | 20 |
| `room/utils/RoomEnterEffect.as` | `src/utils/RoomEnterEffect.cs` | 84 |
| `room/utils/RoomRotatingEffect.as` | `src/utils/RoomRotatingEffect.cs` | 86 |
| `room/utils/RoomShakingEffect.as` | `src/utils/RoomShakingEffect.cs` | 86 |
| `room/utils/_SafeStr_93.as` | `src/utils/ColorTransitioner.cs` | 55 |
| `room/utils/_SafeStr_217.as` (class_1781) | `src/utils/TileUtil.cs` | 117 |
| `room/data/RoomObjectSpriteData.as` | `src/data/RoomObjectSpriteData.cs` | 23 |
| `room/object/enum/RoomObjectSpriteType.as` | `src/object/enum/RoomObjectSpriteType.cs` | 14 |
| `room/object/enum/_SafeStr_181.as` (class_3641) | `src/object/enum/AlphaTolerance.cs` | 13 |

**Milestone:** `Vector3d`, `RoomGeometry`, `ColorConverter` compile and can be unit-tested standalone.

---

## Phase 2: Interfaces, Events, Exceptions ✅ COMPLETED

**Project:** `Vortex.Room/src/` (interfaces across subdirs), `src/events/`, `src/messages/`, `src/exceptions/`
**Deps:** Phase 1
**Files:** 36 | **LOC:** ~900
**Testable:** Compile check

### Interfaces (26+)

| AS3 File | C# File |
|----------|---------|
| `room/IRoomInstance.as` | `src/IRoomInstance.cs` |
| `room/IRoomInstanceContainer.as` | `src/IRoomInstanceContainer.cs` |
| `room/IRoomManager.as` | `src/IRoomManager.cs` |
| `room/IRoomManagerListener.as` | `src/IRoomManagerListener.cs` |
| `room/IRoomObjectFactory.as` | `src/IRoomObjectFactory.cs` |
| `room/IRoomObjectManager.as` | `src/IRoomObjectManager.cs` |
| `room/IRoomContentLoader.as` | `src/IRoomContentLoader.cs` |
| `room/object/IRoomObject.as` | `src/object/IRoomObject.cs` |
| `room/object/IRoomObjectController.as` | `src/object/IRoomObjectController.cs` |
| `room/object/IRoomObjectModel.as` | `src/object/IRoomObjectModel.cs` |
| `room/object/IRoomObjectModelController.as` | `src/object/IRoomObjectModelController.cs` |
| `room/object/IRoomObjectVisualizationFactory.as` | `src/object/IRoomObjectVisualizationFactory.cs` |
| `room/object/logic/IRoomObjectEventHandler.as` | `src/object/logic/IRoomObjectEventHandler.cs` |
| `room/object/logic/IRoomObjectMouseHandler.as` | `src/object/logic/IRoomObjectMouseHandler.cs` |
| `room/object/visualization/IRoomObjectVisualization.as` | `src/object/visualization/IRoomObjectVisualization.cs` |
| `room/object/visualization/IRoomObjectVisualizationData.as` | `src/object/visualization/IRoomObjectVisualizationData.cs` |
| `room/object/visualization/IRoomObjectSprite.as` | `src/object/visualization/IRoomObjectSprite.cs` |
| `room/object/visualization/IRoomObjectSpriteVisualization.as` | `src/object/visualization/IRoomObjectSpriteVisualization.cs` |
| `room/object/visualization/IRoomObjectGraphicVisualization.as` | `src/object/visualization/IRoomObjectGraphicVisualization.cs` |
| `room/object/visualization/ISortableSprite.as` | `src/object/visualization/ISortableSprite.cs` |
| `room/object/visualization/IRoomPlane.as` | `src/object/visualization/IRoomPlane.cs` |
| `room/object/visualization/IPlaneVisualization.as` | `src/object/visualization/IPlaneVisualization.cs` |
| `room/object/visualization/IPlaneDrawingData.as` | `src/object/visualization/IPlaneDrawingData.cs` |
| `room/renderer/IRoomRenderer.as` | `src/renderer/IRoomRenderer.cs` |
| `room/renderer/IRoomRendererBase.as` | `src/renderer/IRoomRendererBase.cs` |
| `room/renderer/IRoomRendererFactory.as` | `src/renderer/IRoomRendererFactory.cs` |
| `room/renderer/IRoomRenderingCanvas.as` | `src/renderer/IRoomRenderingCanvas.cs` |
| `room/renderer/IRoomRenderingCanvasMouseListener.as` | `src/renderer/IRoomRenderingCanvasMouseListener.cs` |
| `room/renderer/IRoomSpriteCanvasContainer.as` | `src/renderer/IRoomSpriteCanvasContainer.cs` |

### Events + Messages + Exceptions (7)

| AS3 File | C# File |
|----------|---------|
| `room/events/RoomObjectEvent.as` | `src/events/RoomObjectEvent.cs` |
| `room/events/RoomObjectMouseEvent.as` | `src/events/RoomObjectMouseEvent.cs` |
| `room/events/RoomSpriteMouseEvent.as` | `src/events/RoomSpriteMouseEvent.cs` |
| `room/events/RoomToObjectEvent.as` | `src/events/RoomToObjectEvent.cs` |
| `room/events/RoomContentLoadedEvent.as` | `src/events/RoomContentLoadedEvent.cs` |
| `room/messages/RoomObjectUpdateMessage.as` | `src/messages/RoomObjectUpdateMessage.cs` |
| `room/exceptions/RoomManagerException.as` | `src/exceptions/RoomManagerException.cs` |

**Milestone:** All contracts defined. Event hierarchy established.

---

## Phase 3: Object Graph Core ✅ COMPLETED

**Project:** `Vortex.Room/src/object/`, `src/`
**Deps:** Phase 1, 2
**Files:** 8 | **LOC:** ~1,620
**Testable:** Yes (create objects, set model properties)

| AS3 File | C# File | LOC |
|----------|---------|-----|
| `room/object/RoomObjectModel.as` | `src/object/RoomObjectModel.cs` | 300 |
| `room/object/RoomObject.as` | `src/object/RoomObject.cs` | 244 |
| `room/RoomObjectManager.as` | `src/RoomObjectManager.cs` | 165 |
| `room/object/logic/ObjectLogicBase.as` | `src/object/logic/ObjectLogicBase.cs` | 119 |
| `room/object/visualization/RoomObjectSprite.as` | `src/object/visualization/RoomObjectSprite.cs` | 391 |
| `room/object/visualization/RoomObjectSpriteVisualization.as` | `src/object/visualization/RoomObjectSpriteVisualization.cs` | 356 |
| `room/object/visualization/utils/IGraphicAsset.as` | `src/object/visualization/utils/IGraphicAsset.cs` | 21 |
| `room/object/visualization/utils/IGraphicAssetCollection.as` | `src/object/visualization/utils/IGraphicAssetCollection.cs` | 24 |

**Milestone:** Can create `RoomObject` with model, assign sprites, store in `RoomObjectManager`.

---

## Phase 4: Graphic Asset System ✅ COMPLETED

**Project:** `Vortex.Room/src/object/visualization/utils/`
**Deps:** Phase 2, core asset system (already ported)
**Files:** 3 | **LOC:** ~750
**Testable:** Yes (asset loading)

| AS3 File | C# File | LOC |
|----------|---------|-----|
| `room/object/visualization/utils/GraphicAsset.as` | `GraphicAsset.cs` | 145 |
| `room/object/visualization/utils/GraphicAssetPalette.as` | `GraphicAssetPalette.cs` | 70 |
| `room/object/visualization/utils/GraphicAssetCollection.as` | `GraphicAssetCollection.cs` | 534 |

**Milestone:** Can load an asset library and retrieve `GraphicAsset` objects with offset metadata.

---

## Phase 5: Room Instance + Room Manager ✅ COMPLETED

**Project:** `Vortex.Room/src/`, `Vortex.Bootstrap/src/`
**Deps:** Phase 1-3
**Files:** 2 new + 1 modified | **LOC:** ~1,050
**Testable:** Yes (room lifecycle)

| AS3 File | C# File | LOC |
|----------|---------|-----|
| `room/RoomInstance.as` | `src/RoomInstance.cs` | 429 |
| `room/RoomManager.as` | `src/RoomManager.cs` | 604 |
| — | `RoomManagerBootstrap.cs` (update to extend RoomManager) | ~15 |

**Milestone:** Can create/destroy rooms, add objects, trigger content loading lifecycle. Bootstrap wired.

---

## Phase 6: Renderer Infrastructure ★ FIRST VISIBLE MILESTONE ✅ COMPLETED

**Project:** `Vortex.Room/src/renderer/`, `Vortex.Bootstrap/src/`
**Deps:** Phase 1-4
**Files:** 14 new + 1 modified | **LOC:** ~2,770
**Testable:** Yes — visible render output

| AS3 File | C# File | LOC |
|----------|---------|-----|
| `renderer/cache/class_3686.as` | `cache/BitmapDataCacheItem.cs` | 76 |
| `renderer/cache/class_3726.as` | `cache/BitmapDataCache.cs` | 152 |
| `renderer/cache/class_3730.as` | `cache/RoomObjectCacheItem.cs` | 51 |
| `renderer/cache/class_3807.as` | `cache/RoomObjectLocationCacheItem.cs` | 95 |
| `renderer/cache/class_3826.as` | `cache/RoomObjectSortableSpriteCacheItem.cs` | 84 |
| `renderer/cache/class_3846.as` | `cache/RoomObjectCache.cs` | 150 |
| `renderer/utils/class_3707.as` | `utils/ExtendedBitmapData.cs` | 64 |
| `renderer/utils/class_3727.as` | `utils/ExtendedSprite.cs` | 192 |
| `renderer/utils/class_3741.as` | `utils/SortableSprite.cs` | 67 |
| `renderer/utils/class_3815.as` | `utils/ObjectMouseData.cs` | 31 |
| `renderer/class_3650.as` | `RoomSpriteCanvas.cs` | 1,233 |
| `renderer/class_3656.as` | `RotatingRoomSpriteCanvas.cs` | 323 |
| `renderer/class_3447.as` | `RoomRenderer.cs` | 215 |
| `renderer/class_2015.as` | `RoomRendererFactory.cs` | 20 |
| — | `RoomRendererFactoryBootstrap.cs` (update) | ~15 |

**Key adaptation:** `RoomSpriteCanvas` (1,233 LOC) — Flash `BitmapData.copyPixels()`/`draw()` → Godot `Image` per-pixel compositing. Follow the same `BlitWithAlpha` pattern from `AvatarImageCache`.

**Milestone:** Renderer accepts RoomObjects, sorts sprites by depth, composites into Image buffer.

---

## Phase 7: Habbo Enums, StuffData, Visualization Data ✅ COMPLETED

**Project:** `Vortex.Habbo/src/room/`
**Deps:** Phase 2
**Files:** ~42 | **LOC:** ~4,100
**Testable:** Yes (StuffData parsing, enum validation)

### 7A: Enums + Type Constants (9 files)

| AS3 File | C# File |
|----------|---------|
| `habbo/room/object/RoomObjectCategoryEnum.as` | `enum/RoomObjectCategoryEnum.cs` |
| `habbo/room/object/RoomObjectLogicEnum.as` | `enum/RoomObjectLogicEnum.cs` |
| `habbo/room/object/RoomObjectTypeEnum.as` | `enum/RoomObjectTypeEnum.cs` |
| `habbo/room/object/RoomObjectVisualizationEnum.as` | `enum/RoomObjectVisualizationEnum.cs` |
| `habbo/room/object/RoomObjectUserTypes.as` | `enum/RoomObjectUserTypes.cs` |
| `habbo/room/object/RoomObjectOperationEnum.as` | `enum/RoomObjectOperationEnum.cs` |
| `habbo/room/object/RoomObjectVariableEnum.as` | `enum/RoomObjectVariableEnum.cs` |
| `habbo/room/RoomVariableEnum.as` | `RoomVariableEnum.cs` |
| `habbo/room/enum/RoomObjectPlacementSource.as` | `enum/RoomObjectPlacementSource.cs` |

### 7B: StuffData (12 files)

| AS3 File | C# File |
|----------|---------|
| `habbo/room/IStuffData.as` | `IStuffData.cs` |
| `habbo/room/object/data/StuffDataBase.as` | `object/data/StuffDataBase.cs` |
| `habbo/room/object/data/LegacyStuffData.as` | `object/data/LegacyStuffData.cs` |
| `habbo/room/object/data/MapStuffData.as` | `object/data/MapStuffData.cs` |
| `habbo/room/object/data/StringArrayStuffData.as` | `object/data/StringArrayStuffData.cs` |
| `habbo/room/object/data/IntArrayStuffData.as` | `object/data/IntArrayStuffData.cs` |
| `habbo/room/object/data/VoteResultStuffData.as` | `object/data/VoteResultStuffData.cs` |
| `habbo/room/object/data/HighScoreStuffData.as` | `object/data/HighScoreStuffData.cs` |
| `habbo/room/object/data/CrackableStuffData.as` | `object/data/CrackableStuffData.cs` |
| `habbo/room/object/data/EmptyStuffData.as` | `object/data/EmptyStuffData.cs` |
| `habbo/room/object/data/_SafeStr_80.as` | `object/data/StuffDataFactory.cs` |
| `habbo/room/object/data/_SafeStr_91.as` + `_SafeStr_92.as` | `object/data/` (2 helpers) |

### 7C: Room Plane Data + Visualization Data (21 files)

| AS3 File | C# File | LOC |
|----------|---------|-----|
| `habbo/room/object/RoomPlaneParser.as` | `object/RoomPlaneParser.cs` | 1,688 |
| `habbo/room/object/RoomPlaneData.as` | `object/RoomPlaneData.cs` | 150 |
| `habbo/room/object/RoomWallData.as` | `object/RoomWallData.cs` | 100 |
| `habbo/room/object/RoomPlaneBitmapMaskData.as` | `object/RoomPlaneBitmapMaskData.cs` | 30 |
| `habbo/room/object/RoomPlaneMaskData.as` | `object/RoomPlaneMaskData.cs` | 20 |
| `habbo/room/object/RoomPlaneBitmapMaskParser.as` | `object/RoomPlaneBitmapMaskParser.cs` | 80 |
| `habbo/room/object/RoomFloorHole.as` | `object/RoomFloorHole.cs` | 20 |
| `visualization/data/LayerData.as` | `object/visualization/data/LayerData.cs` | 80 |
| `visualization/data/DirectionData.as` | `…/DirectionData.cs` | 60 |
| `visualization/data/ColorData.as` | `…/ColorData.cs` | 40 |
| `visualization/data/SizeData.as` | `…/SizeData.cs` | 416 |
| `visualization/data/DirectionalOffsetData.as` | `…/DirectionalOffsetData.cs` | 40 |
| `visualization/data/AnimationData.as` | `…/AnimationData.cs` | 265 |
| `visualization/data/AnimationFrame.as` | `…/AnimationFrame.cs` | 60 |
| `visualization/data/AnimationFrameData.as` | `…/AnimationFrameData.cs` | 50 |
| `visualization/data/AnimationFrameDirectionalData.as` | `…/AnimationFrameDirectionalData.cs` | 40 |
| `visualization/data/AnimationFrameSequenceData.as` | `…/AnimationFrameSequenceData.cs` | 60 |
| `visualization/data/AnimationLayerData.as` | `…/AnimationLayerData.cs` | 40 |
| `visualization/data/AnimationSizeData.as` | `…/AnimationSizeData.cs` | 60 |
| `visualization/data/AnimationStateData.as` | `…/AnimationStateData.cs` | 40 |
| `visualization/data/ExtraDataManager.as` | `…/ExtraDataManager.cs` | 50 |

**Milestone:** All enums defined. StuffData parses from wire. Visualization data parses from XML. RoomPlaneParser converts heightmap strings to geometry.

---

## Phase 8: Communication Messages ✅ COMPLETED

**Project:** `Vortex.Habbo/src/room/messages/`, `src/communication/messages/`
**Deps:** Phase 2, 7
**Files:** ~130 | **LOC:** ~4,400
**Testable:** Yes (round-trip parse/compose)

### 8A: Habbo Room Update Messages (40 files in `src/room/messages/`)

Internal messages between RoomEngine subsystems. Most are simple data carriers extending `RoomObjectUpdateMessage`.

Examples: `RoomObjectAvatarUpdateMessage`, `RoomObjectAvatarFigureUpdateMessage`, `RoomObjectAvatarPostureUpdateMessage`, `RoomObjectAvatarDanceUpdateMessage`, `RoomObjectAvatarEffectUpdateMessage`, `RoomObjectAvatarCarryUpdateMessage`, `RoomObjectAvatarChatUpdateMessage`, `RoomObjectAvatarTypingUpdateMessage`, `RoomObjectAvatarExpressionUpdateMessage`, `RoomObjectMoveUpdateMessage`, `RoomObjectDataUpdateMessage`, `RoomObjectHeightUpdateMessage`, `RoomObjectModelDataUpdateMessage`, `RoomObjectItemDataUpdateMessage`, `RoomObjectGroupBadgeUpdateMessage`, `RoomObjectSelectedMessage`, `RoomObjectTileCursorUpdateMessage`, `RoomObjectUpdateStateMessage`, `RoomObjectVisibilityUpdateMessage`, `RoomObjectRoomFloorUpdateMessage`, `RoomObjectRoomWallUpdateMessage`, `RoomObjectRoomLandscapeUpdateMessage`, `RoomObjectRoomColorUpdateMessage`, `RoomObjectRoomFloorHoleUpdateMessage`, `RoomObjectRoomMaskUpdateMessage`, `RoomObjectRoomPlaneVisibilityUpdateMessage`, `RoomObjectRoomDoorUpdateMessage`, `RoomObjectRoomAdUpdateMessage`, etc.

### 8B: Communication Parsers + Events + Composers (90 files in `src/communication/messages/`)

| Type | Count | Avg LOC |
|------|-------|---------|
| Incoming Events (MessageEvent wrappers) | ~31 | ~15 |
| Parsers (IMessageParser impls) | ~31 | ~80 |
| Outgoing Composers (IMessageComposer impls) | ~28 | ~30 |

Key parsers: `ObjectsMessageParser` (all floor furniture), `ItemsMessageParser` (wall items), `UsersMessageParser` (avatars), `HeightMapMessageParser`, `FloorHeightMapMessageParser`, `RoomReadyMessageParser`, `ObjectUpdateMessageParser`, `UserUpdateMessageParser`, `RoomEntryTileMessageParser`

### 8C: HabboMessages.cs Registration

Add ~60 new entries mapping message IDs to events/composers.

**Milestone:** All room wire protocol messages can be parsed and composed.

---

## Phase 9a: Logic — Core, Room, Avatar, Pet ✅ COMPLETED

**Project:** `Vortex.Habbo/src/room/object/logic/`
**Deps:** Phase 3, 7, 8
**Files:** 8 | **LOC:** ~1,730
**Testable:** Compile check

| AS3 File | C# File | LOC |
|----------|---------|-----|
| `habbo/room/object/logic/MovingObjectLogic.as` | `logic/MovingObjectLogic.cs` | 200 |
| `habbo/room/object/logic/AvatarLogic.as` | `logic/AvatarLogic.cs` | 548 |
| `habbo/room/object/logic/PetLogic.as` | `logic/PetLogic.cs` | 313 |
| `habbo/room/object/logic/room/RoomLogic.as` | `logic/room/RoomLogic.cs` | 461 |
| `habbo/room/object/logic/room/RoomTileCursorLogic.as` | `logic/room/RoomTileCursorLogic.cs` | 80 |
| `habbo/room/object/logic/room/SelectionArrowLogic.as` | `logic/room/SelectionArrowLogic.cs` | 60 |
| `habbo/room/object/logic/game/SnowballLogic.as` | `logic/game/SnowballLogic.cs` | 40 |
| `habbo/room/object/logic/game/SnowSplashLogic.as` | `logic/game/SnowSplashLogic.cs` | 30 |

**Milestone:** Room logic processes room events. MovingObjectLogic handles pathfinding. AvatarLogic handles avatar state.

---

## Phase 9b: Furniture Logic (Bulk) ✅ COMPLETED

**Project:** `Vortex.Habbo/src/room/object/logic/furniture/`
**Deps:** Phase 9a
**Files:** 65 (64 furniture + 1 SnowSplashLogic) + 10 event classes | **LOC:** ~3,400
**Testable:** Compile check

`FurnitureLogic` base (436 LOC) + `FurnitureMultiStateLogic` + ~62 specialized logics.

### Deobfuscation Map (`_SafeStr_*` → real names, via PRODUCTION source)

| Obfuscated | Real Name |
|------------|-----------|
| `_SafeStr_103` | FurniturePurchasableClothingLogic |
| `_SafeStr_105` | FurnitureEffectboxLogic |
| `_SafeStr_106` | FurnitureMysterboxLogic |
| `_SafeStr_109` | FurnitureCrackableLogic |
| `_SafeStr_110` | FurnitureGroupForumTerminalLogic |
| `_SafeStr_111` | FurnitureBadgeDisplayLogic |
| `_SafeStr_113` | FurnitureWildwestWantedLogic |
| `_SafeStr_121` | FurnitureCustomStackHeightLogic |
| `_SafeStr_122` | FurnitureWindowLogic |
| `_SafeStr_123` | FurnitureMonsterplantSeedLogic |
| `_SafeStr_130` | FurnitureRandomTeleportLogic |
| `_SafeStr_133` | FurnitureVimeoLogic |
| `_SafeStr_135` | FurnitureLovelockLogic |
| `_SafeStr_136` | FurnitureVoteMajorityLogic |
| `_SafeStr_138` | FurnitureCraftingGizmoLogic |
| `_SafeStr_146` | FurnitureCounterClockLogic |
| `_SafeStr_149` | FurnitureRentableSpaceLogic |
| `_SafeStr_151` | FurnitureYoutubeLogic |
| `_SafeStr_152` | FurnitureHweenLovelockLogic |

**Milestone:** All ~55 logic factory cases have concrete implementations.

---

## Phase 10: Room Visualization + Rasterizer Pipeline ★ SECOND VISIBLE MILESTONE ✅ COMPLETED

**Project:** `Vortex.Habbo/src/room/object/visualization/room/`
**Deps:** Phase 4, 7
**Files:** 33 | **LOC:** ~6,400
**Testable:** Yes — visible render of floor tiles and walls

### 10A: Rasterizer Base + Materials (16 files)

PlaneTextureBitmap, PlaneTexture, PlaneMaterialCell (230 LOC), PlaneMaterialCellColumn (511 LOC), PlaneMaterialCellMatrix (729 LOC), PlaneMaterial, Plane, PlaneVisualizationLayer, PlaneVisualization (264 LOC), PlaneRasterizer base (746 LOC), FloorPlane, FloorRasterizer, WallPlane, WallRasterizer, WallAdRasterizer, IPlaneRasterizer

### 10B: Animated Landscape (4 files)

AnimationItem, PlaneVisualizationAnimationLayer, LandscapePlane, LandscapeRasterizer (298 LOC)

### 10C: Masks + Room Plane + Room Visualization (13 files)

PlaneMaskBitmap, PlaneMask, PlaneMaskVisualization, PlaneMaskManager (236 LOC), PlaneDrawingData, RoomPlaneBitmapMask, RoomPlaneRectangleMask, **RoomPlane (1,123 LOC)**, TileCursorVisualization, RoomVisualizationData, **RoomVisualization (1,009 LOC)**, PlaneBitmapData, Randomizer

**Key adaptation:** Rasterizers render isometric floor diamonds and wall segments into Image buffers using pixel manipulation. `RoomPlane` composites layers with material textures. `RoomVisualization` coordinates the full room render.

**Milestone:** Can feed a heightmap to RoomPlaneParser → RoomVisualization and get a rendered floor/wall Image.

---

## Phase 11a: Furniture Visualization (Bulk) ✅ COMPLETED

**Project:** `Vortex.Habbo/src/room/object/visualization/furniture/`
**Deps:** Phase 7, 10
**Files:** ~43 | **LOC:** ~7,000
**Testable:** Compile check

Key files:
- `FurnitureVisualizationData.as` (358 LOC) — XML-driven layer/direction/color/animation data
- `AnimatedFurnitureVisualizationData.as` (120 LOC)
- `FurnitureVisualization.as` (618 LOC) — base render: layer sprites from asset collection
- `AnimatedFurnitureVisualization.as` (424 LOC) — frame-based animation
- `FurniturePlane.as` (498 LOC) — isometric plane for furniture
- `ShoreMaskCreatorUtility.as` (315 LOC) — water shore mask generator
- `FurnitureParticleSystem*.as` (3 files, 600 LOC total) — particle effects
- ~25 specialized furniture visualizations

**Milestone:** All ~35 visualization factory cases have concrete implementations.

---

## Phase 11b: Avatar + Pet Visualization

**Project:** `Vortex.Habbo/src/room/object/visualization/avatar/`, `pet/`
**Deps:** Phase 7, Avatar Render System (already ported)
**Files:** 17 | **LOC:** ~2,700
**Testable:** Compile check

Key files:
- **`AvatarVisualization.as` (1,243 LOC)** — largest file after RoomEngine. Bridges `IAvatarRenderManager` to room sprite system.
- 9 avatar additions (TypingBubble, FloatingIdleZ, MutedBubble, NumberBubble, GuideStatusBubble, GameClickTarget, ExpressionAddition, FloatingHeart, etc.)
- `AnimatedPetVisualization.as` (644 LOC) — pet-specific animation with experience bubbles
- Pet visualization data (3 files)

**Milestone:** Avatars and pets can be visually rendered in room context.

---

## Phase 12: RoomEngine Facade — Part 1 (Setup + Content) ✅ COMPLETED

**Project:** `Vortex.Habbo/src/room/`, `Vortex.Bootstrap/src/`
**Deps:** Phase 5, 6, 8
**Files:** ~43 new + 1 modified | **LOC:** ~5,900
**Testable:** Yes (room creation lifecycle)

### 12A: Habbo Room Events (31 files in `src/room/events/`)

`RoomEngineEvent`, `RoomEngineObjectEvent`, `RoomEngineZoomEvent`, `RoomEngineRoomColorEvent`, `RoomEngineDimmerStateEvent`, `RoomEngineHSLColorEnableEvent`, `RoomEngineObjectPlacedEvent`, `RoomEngineObjectPlacedOnUserEvent`, `RoomEngineObjectPlaySoundEvent`, `RoomEngineObjectSamplePlaybackEvent`, `RoomEngineSoundMachineEvent`, `RoomEngineRoomAdEvent`, `RoomEngineDragWithMouseEvent`, `RoomEngineAreaHideStateWidgetEvent`, `RoomEngineToWidgetEvent`, `RoomEngineUseProductEvent`, `RoomObjectBadgeAssetEvent`, `RoomObjectDataRequestEvent`, `RoomObjectDimmerStateUpdateEvent`, `RoomObjectFloorHoleEvent`, `RoomObjectFurnitureActionEvent`, `RoomObjectHSLColorEnableEvent`, `RoomObjectMoveEvent`, `RoomObjectPlaySoundIdEvent`, `RoomObjectRoomAdEvent`, `RoomObjectSamplePlaybackEvent`, `RoomObjectStateChangeEvent`, `RoomObjectTileMouseEvent`, `RoomObjectWallMouseEvent`, `RoomObjectWidgetRequestEvent`, `RoomToObjectOwnAvatarMoveEvent`

### 12B: Utility Classes (10 files in `src/room/utils/`)

| AS3 File | C# File | LOC |
|----------|---------|-----|
| `utils/class_3344.as` | `utils/RoomInstanceData.cs` | 228 |
| `utils/class_3419.as` | `utils/FurniStackingHeightMap.cs` | 150 |
| `utils/class_3373.as` | `utils/LegacyWallGeometry.cs` | 330 |
| `utils/class_3355.as` | `utils/RoomCamera.cs` | 291 |
| `utils/class_1769.as` | `utils/RoomData.cs` | 40 |
| `utils/class_3413.as` | `utils/SelectedRoomObjectData.cs` | 80 |
| `utils/class_3513.as` | `utils/TileObjectMap.cs` | 100 |
| `utils/class_3498.as` | `utils/SpriteDataCollector.cs` | 441 |
| `utils/class_3500.as` | `utils/FurnitureData.cs` | 80 |
| `utils/class_3467.as` | `utils/RoomObjectBadgeImageAssetListener.cs` | 40 |

### 12C: Interfaces + Content Loader + RoomEngine Part 1

| AS3 File | C# File | LOC |
|----------|---------|-----|
| `IRoomEngine.as` | `IRoomEngine.cs` | 60 |
| `IRoomEngineServices.as` | `IRoomEngineServices.cs` | 30 |
| `IRoomCreator.as` | `IRoomCreator.cs` | 20 |
| `IRoomObjectCreator.as` | `IRoomObjectCreator.cs` | 15 |
| `IRoomContentListener.as` | `IRoomContentListener.cs` | 10 |
| `IGetImageListener.as` | `IGetImageListener.cs` | 10 |
| `ISelectedRoomObjectData.as` | `ISelectedRoomObjectData.cs` | 15 |
| `PetColorResult.as` | `PetColorResult.cs` | 15 |
| `RoomContentLoader.as` | `RoomContentLoader.cs` | 1,477 |
| `RoomEngine.as` (Part 1) | `RoomEngine.cs` (partial) | ~2,000 |
| — | `RoomEngineBootstrap.cs` (update) | ~15 |

**Part 1 scope in RoomEngine:** Constructor, 13 ComponentDependencies, `InitComponent()`, room creation/disposal methods, content loading, camera management.

**Milestone:** RoomEngine resolves dependencies, creates rooms via RoomManager, loads content. Bootstrap wired.

---

## Phase 13: RoomEngine Facade — Part 2 (Events + Message Handling)

**Project:** `Vortex.Habbo/src/room/`
**Deps:** Phase 12
**Files:** 2 new + 1 completed | **LOC:** ~6,400
**Testable:** Yes (room loads from server messages)

| AS3 File | C# File | LOC |
|----------|---------|-----|
| `RoomEngine.as` (Part 2) | Complete `RoomEngine.cs` | ~2,500 |
| `RoomMessageHandler.as` | `RoomMessageHandler.cs` | 1,391 |
| `RoomObjectEventHandler.as` | `RoomObjectEventHandler.cs` | 2,499 |

**Part 2 scope in RoomEngine:** Update loop (`IUpdateReceiver`), render dispatch, mouse event processing, avatar/furniture/wall-item creation from server data, image generation (getImage/getGenericImage).

**RoomMessageHandler:** Registers ~50 incoming message types, dispatches to RoomEngine via `IRoomCreator`. This is the network→room-state bridge.

**RoomObjectEventHandler:** Mouse hit-testing on room sprites, dispatches room events to widgets/UI.

**Milestone:** Full message flow: server → RoomMessageHandler → RoomEngine → render.

---

## Phase 14: Factories

**Project:** `Vortex.Habbo/src/room/`, `Vortex.Bootstrap/src/`
**Deps:** Phase 9b, 11a
**Files:** 2 new + 2 modified | **LOC:** ~780
**Testable:** Yes (factory instantiation)

| AS3 File | C# File | LOC |
|----------|---------|-----|
| `RoomObjectFactory.as` | `src/room/RoomObjectFactory.cs` | 387 |
| `RoomObjectVisualizationFactory.as` | `src/room/object/RoomObjectVisualizationFactory.cs` | 363 |
| — | `RoomObjectFactoryBootstrap.cs` (update) | ~15 |
| — | `RoomObjectVisualizationFactoryBootstrap.cs` (update) | ~15 |

**Milestone:** All ~55 logic types and ~35 visualization types registered in factory switch statements.

---

## Phase 15: Integration + Polish ★ FULL VISIBLE MILESTONE ✅ COMPLETED

**Project:** All
**Deps:** All phases
**Files:** ~6 | **LOC:** ~760
**Testable:** Yes — full room rendering in Godot viewport

| Task | Detail | Status |
|------|--------|--------|
| `RoomPreviewer.as` → `src/Vortex.Habbo/src/room/preview/RoomPreviewer.cs` | ~540 LOC — furniture/avatar preview for catalog | ✅ |
| `RoomPreviewerWidget` wiring | Widget now creates and exposes RoomPreviewer instance | ✅ |
| Bootstrap verification | All 8 bootstraps verified; RoomRendererFactoryBootstrap param3 fix | ✅ |
| Null guard audit | All unported dependency paths confirmed guarded | ✅ |

**Milestone:** Full room rendering pipeline works end-to-end: server message → room creation → object placement → isometric rendering in Godot.

---

## Risks and Mitigations

| Risk | Severity | Mitigation |
|------|----------|------------|
| `RoomSpriteCanvas` (1,233 LOC) Flash→Godot pixel rendering | High | Follow WindowRenderer/AvatarImageCache `BlitWithAlpha` pattern |
| `RoomPlaneParser` (1,688 LOC) heightmap parsing | Medium | Test with known Habbo room layout strings |
| `RoomEngine` 13 ComponentDependencies to unported systems | Medium | Null guards per design decision |
| ~26 `_SafeStr_*` obfuscated files | Low | Deobfuscation map provided above; PRODUCTION source as fallback |
| Communication message IDs | Low | Cross-reference WIN63-202407 (parsers) with WIN63-202111 (registration) |

---

## Key Reference Files

| File | Purpose |
|------|---------|
| `sources/WIN63-202111…/habbo/room/RoomEngine.as` | Primary facade (4,502 LOC) |
| `sources/WIN63-202111…/room/renderer/RoomSpriteCanvas.as` | Core canvas (1,233 LOC) |
| `sources/WIN63-202111…/habbo/room/RoomObjectFactory.as` | Logic type registry |
| `sources/PRODUCTION…/habbo/room/object/logic/furniture/` | Deobfuscation reference |
| `src/Vortex.Habbo/src/avatar/AvatarRenderManager.cs` | Component pattern to follow |
| `src/Vortex.Core/runtime/window/renderer/WindowRenderer.cs` | BitmapData→Image rendering pattern |
| `docs/COMMUNICATION_EXAMPLES.md` | Parser/composer pattern guide |
