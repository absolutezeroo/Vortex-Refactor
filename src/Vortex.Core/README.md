# Vortex.Core

Runtime foundation for the Vortex client. Provides component-context DI, communication, window rendering, assets, localization, and logging.

## SDK

`Microsoft.NET.Sdk` — References `GodotSharp`/`Godot.SourceGenerators` 4.6.1 as PackageReference.
Depends on: `Vortex.Bundle`

## Directory Structure

```
src/
  runtime/              Component-Context DI framework, lifecycle, interface queuing
    events/             Runtime event types
    exceptions/         Runtime exceptions
  communication/        CoreCommunicationManager (TCP socket layer)
    connection/         IConnection, SocketConnection
    wireformat/         EvaWireFormat (binary protocol encoding)
    messages/           IMessageComposer, IMessageDataWrapper, message infrastructure
    handshake/          Protocol initialization
    encryption/         Cipher handling
    util/               Message utilities
  window/               Window system core
    components/         39 controller implementations (text, buttons, containers, scrollbars, lists)
    graphics/           GraphicContext (Node2D-based drawing surface)
      renderer/         Skin renderers (bitmap 9-slice, text, fill, shape)
    services/           Mouse ops, focus, tooltip, gesture, drag scaling
    motion/             Animation system (17 files: Motion, Ease, MoveTo, ResizeTo, MotionManager)
    utils/              Text rendering, event processing, cursors, iterators, layout limits
    events/             Window event types
    enum/               Window enums (types, flags, states, styles, directions)
    iterators/          ItemList/ItemGrid iterators
    theme/              ThemeManager, DynamicStyle, style registry
    tools/              Window diagnostic tools
  assets/               Asset library system
    loaders/            BinaryFileLoader, BitmapFileLoader, TextFileLoader
    bundle/             VortexBundleLoader, VortexSpritesheet
  localization/         CoreLocalizationManager, Localization (reactive listeners, interpolation)
    enum/               Language IDs
  logging/              Logger (static, file + Godot output)
  utils/                Display utils, error reporting, profiler
    images/             Image processing
    debug/              Profiler output
```

## Key Concepts

### Component-Context DI (`runtime/`)

- **`Component`** — Base class for all managed entities. Lifecycle: lock → inject dependencies → unlock → `InitComponent()` → emit `INTERNAL_EVENT_UNLOCKED`. Has asset storage, event dispatching, reference counting.
- **`ComponentContext`** — Container for Components. Manages interface queuing (async dependency resolution) and update receivers.
- **`CoreComponentContext`** — Root context with Godot integration (scene tree, frame updates, hibernation). Parses configuration XML. 3 priority levels of update receivers.
- **`IID`** — Empty marker interface. Subclassed in `Vortex.IID` for type-safe DI keys.
- **`ComponentDependency`** — Declares: IID type, setter callback, required flag, event listeners.

### Window System (`window/`)

- **`IWindow`** — Full window interface: position, size, children, events, state/style/param flags, hit testing.
- **`WindowContext`** — Window manager: creates windows from XML, render queue, input event processing.
- **`WindowController`** — Base controller (64 KB). Layout XML injection, property cascading, skin rendering, invalidation.
- **`WindowRenderer`** / **`WindowRendererItem`** — Queue-based render pipeline with dirty rect tracking, clipping, z-order.
- **`GraphicContext`** — Node2D wrapping Image + SubViewport for off-screen rendering.

### Communication (`communication/`)

- **`CoreCommunicationManager`** — Component managing `IConnection` instances, calls `ProcessReceivedData()` per frame.
- **`SocketConnection`** — TCP client with handshake, message queuing, encryption support.
- **`EvaWireFormat`** — Binary message format (length-prefixed, big-endian message IDs).
- **`EvaMessageDataWrapper`** — Sequential typed reads (ReadString, ReadInt, ReadBoolean).

### Assets (`assets/`)

- **`AssetLibrary`** — Manifest XML parsing, 11 MIME types, asset aliases, lazy loading.
- Asset types: `BitmapDataAsset`, `TextAsset`, `XmlAsset`, `SoundAsset`, `DisplayAsset`, `TypeFaceAsset`, `VortexBundleAsset`, `UnknownAsset`.

### Localization (`localization/`)

- **`CoreLocalizationManager`** — Component with reactive listener system.
- **`Localization`** — Key-value store with parameter interpolation (`%{param}%`), plural forms (`%{key|singular|dual|plural}%`), nested keys (`%%%subkey%%%`).
- **`ILocalizable`** — Interface for UI elements that receive localized text updates.