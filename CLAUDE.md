# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FairyGUI-godot is a C# runtime library for [FairyGUI](https://www.fairygui.com/) in Godot 4. It is adapted from the Unity version with reference to the Laya version. Developed and tested primarily on Godot 4.4/4.6, targeting .NET 8.

**Known limitations (by design):**
- No 3D UI support
- No grayscale filter (grayscale shader exists but is limited)
- No filter effects
- BlendMode support is limited

## Building

This is a Godot 4 C# project. Build via:
- **Godot Editor**: Open project and use Build → Build Solution, or press Ctrl+B
- **CLI**: `dotnet build FairyGUI-godot.csproj`

The project uses `Godot.NET.Sdk/4.6.0` targeting `net8.0`. Assembly name is `FairyGUI-godot`, root namespace is `FairyGUIgodot`.

Running the project requires opening it in the Godot editor (version 4.4+). The main scene is `Examples/demo.tscn`.

> Note: When importing for the first time on Godot 4.5+, the editor may crash. Use recovery mode to complete the import, then open normally.

## UI Asset Export

When exporting from the FairyGUI editor, select the **Laya** export type. This allows renaming the description file extension (e.g., `.fui` or `.res`), avoiding extra post-export steps. Assets live under `UIProject/assets/` with a corresponding `.fairy` project file.

## Architecture

### Two-Layer Design

The library uses a strict two-layer separation:

**Display Layer (`fgui/Core/`)** — Godot Node wrappers that bridge to the scene tree:
- `IDisplayObject` — interface for all display objects; exposes `node` (`CanvasItem`), transform, visibility, blend mode
- `NContainer : Node2D` — base display container (no clipping); each `GObject` owns one
- `NClipContainer : SubViewport` — display container with clipping/masking support
- `NImage`, `NShape` — leaf drawing nodes
- `Stage : CanvasLayer` — singleton; root of the entire scene graph, handles input routing

**UI Logic Layer (`fgui/UI/`)** — platform-agnostic FairyGUI objects:
- `GObject` — base class for all UI objects; holds a reference to an `IDisplayObject` (its `displayObject`)
- `GComponent : GObject` — container that manages children
- `GRoot : GComponent` — singleton top-level component, created automatically by `Stage.Instantiate()`
- Concrete widgets: `GButton`, `GLabel`, `GTextField`, `GTextInput`, `GRichTextField`, `GImage`, `GMovieClip`, `GLoader`, `GGraph`, `GList`, `GTree`, `GProgressBar`, `GSlider`, `GScrollBar`, `GComboBox`
- `Window` — modal/non-modal window base class
- `UIPackage` — loads `.res`/`.bytes` package files and instantiates UI objects
- `UIObjectFactory` — maps package component types to C# classes for customization
- `ScrollPane` — handles scrolling within `GComponent`
- `Transition` — timeline-based animation system
- `Controller` — state machine for component variants (Gears)

### Event System (`fgui/Event/`)
- `EventDispatcher` — base class for all objects that can send/receive events
- Events bubble up the `GObject.parent` chain
- Standard event names: `"onClick"`, `"onRollOver"`, `"onRollOut"`, `"onTouchBegin"`, `"onTouchEnd"`, `"onTouchMove"`, `"onMouseWheel"`, `"onKeyDown"`, `"onKeyUp"`, `"onFocusIn"`, `"onFocusOut"`

### Gear System (`fgui/UI/Gears/`)
Gears bind controller states to object properties (position, size, color, display, text, icon, animation, look, font size). Each `GObject` can have up to one gear per property type, set by the FairyGUI editor.

### Tween System (`fgui/Tween/`)
- `GTween` — static factory: `GTween.To(...)`, `GTween.Shake(...)`, etc.
- `GTweener` — active tween instance with chaining API
- `TweenManager : Node` — singleton that drives all tweens each frame

### Text Rendering (`fgui/Core/Text/`)
Text is **self-rendered** (not using Godot's Label/RichTextLabel nodes). Godot fonts supply only glyph data; all layout and mesh generation is custom. This enables full FairyGUI text features (UBB markup, inline images, emoji). Key config in `UIConfig`:
- `glyphCacheTexSize` — glyph cache texture size
- `textOutlineType` — outline style (`Godot` shadow vs. custom)
- `minFontSize` / `maxFontSize` / `fontSizeLevels` — font size normalization

### Shaders (`fgui/Resources/`)
Three built-in GDShaders:
- `ui_standard.gdshader` — standard UI rendering with blend mode support
- `ui_grayscale.gdshader` — grayscale effect
- `ui_blur_screen.gdshader` — screen-space blur

### Initialization Pattern
```csharp
// Stage and GRoot are created lazily on first access, or call explicitly:
Stage.Instantiate(); // creates Stage (CanvasLayer) + GRoot

// Load a package
UIPackage.AddPackage("res://path/to/Package.res");

// Create a UI object
GComponent view = UIPackage.CreateObject("PackageName", "ComponentName").asCom;
GRoot.inst.AddChild(view);
```

### Custom GLoader
To load textures from custom sources (e.g., runtime icon URLs), subclass `GLoader` and override `LoadItem()`, then register:
```csharp
UIObjectFactory.SetLoaderExtension(() => new MyGLoader());
```

## Code Conventions

- All library code is in the `FairyGUI` namespace (`fgui/` directory)
- Example/demo code lives in `Examples/` with no namespace
- `GObject` subclasses use the `G` prefix; display node wrappers use the `N` prefix
- `partial class` is required on Godot node subclasses (`Stage`, `NContainer`, `UIConfig`, etc.)
- `UIConfig` static fields must be set before any UI is constructed
