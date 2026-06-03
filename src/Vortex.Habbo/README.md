# Vortex.Habbo

Habbo Hotel-specific managers, communication protocols, window theming, and avatar rendering. Extends Vortex.Core with all Habbo business logic ported from AS3 source dumps.

## SDK

`Microsoft.NET.Sdk` — References `GodotSharp`/`Godot.SourceGenerators` 4.6.1.
Depends on: `Vortex.Core`, `Vortex.IID`

## Directory Structure

```
src/
  communication/            HabboCommunicationManager, encryption, message system
	demo/                   Test/demo communication screens
	encryption/             ArcFour (RC4), DiffieHellman key exchange
	handshake/              Protocol handshake events
	login/                  WebApiLoginProvider, ILoginProvider
	messages/
	  incoming/             10 parser types (AuthenticationOK, Ping, DiffieHandshake, etc.)
	  outgoing/             8 composer types (ClientHello, SSO, Disconnect, etc.)
	HabboMessages.cs        Registry: message ID → Parser/Composer type mapping
  configuration/            HabboConfigurationManager (~776 lines)
	HabboConfigurationEvent/Flags/Property enums
  localization/             HabboLocalizationManager (badge system, external downloads)
  window/                   HabboWindowManagerComponent (~1118 lines), theme/skin management
	components/             Controller implementations (inherited from Core)
	enum/                   Window type/state constants
	handlers/               HabbletLinkHandler, ElementPointerHandler
	services/               HintManager, ServiceManager
	utils/                  Alert dialogs, modal system, profiler
	widgets/                20 widget implementations (countdown, progress, balloon, illumina, etc.)
  avatar/                   Avatar render system (Phase 1 data layer — 40+ files)
	(math, interfaces, data types, actions, animation, geometry)
  session/                  Session data management (scaffold)
  advertisement/            Ad system (scaffold)
  catalog/                  Catalog/shop (scaffold)
  ... (20+ scaffold dirs for unported features)
```

## Key Components

### HabboCommunicationManager
Extends `Component`. Manages TCP socket lifecycle, port rotation, encryption initialization. Registers message parsers/composers via `HabboMessages.cs`.

### HabboConfigurationManager (~776 lines)
Loads configuration from: embedded assets → environment variables → HTTP downloads. Property interpolation with `${key}` syntax. Supports environment-specific overrides.

### HabboLocalizationManager
Extends `CoreLocalizationManager`. Badge parsing (base+level), roman numerals, external HTTP downloads with gamedata hashes. Reactive listener system for `${key}` text substitutions.

### HabboWindowManagerComponent (~1118 lines)
Manages 4 window context layers. Creates windows via XML parsing. Alert/confirm/modal dialogs. Theme manager + SkinContainer + TextStyleManager integration. Implements `IWindowFactory`, `IUpdateReceiver`.

### Avatar Render System (Phase 1 Complete)
Data layer: AvatarVector3D, AvatarMatrix4x4, FigureSetData, AvatarFigureContainer, ActionDefinition, AnimationData, AvatarModelGeometry. Pending: alias system, download infrastructure, AvatarStructure bridge.

## Message Registration Pattern

```csharp
// HabboMessages.cs
events[1417] = typeof(IdentityAccountsEvent);    // incoming
composers[1113] = typeof(DisconnectMessageComposer); // outgoing
```

Four required types per message: Composer, Event, Parser, Registration (see `docs/COMMUNICATION_EXAMPLES.md`).

## Configuration Property Access

```csharp
manager.GetProperty("connection.info.host"); // returns interpolated value
// Supports: ${key} recursive substitution, environment-specific overrides
```
