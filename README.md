# OutlineSystem

Drop-in outline highlighting for Unity. Zero dependencies, doesn't touch the object's own material.

## Overview

OutlineSystem adds a single `Outline` component you can drop on any GameObject to be able to show an
outline around it - a highlight for the currently focused/selected/hovered object, without changing
anything about how that object already renders. It never swaps or edits the object's real material, so
whatever shading setup a project already uses (PBR, toon, a custom shader) stays completely intact; the
outline is drawn as an extra inverted-hull pass appended to the same renderer.

It has no dependencies of its own, so it compiles and works in any project, standalone. Other systems
(e.g. InteractionSystem) can hook into it optionally to show the outline on focus and hide it on
lose-focus, but nothing about OutlineSystem itself requires them.

## Features

- `Outline` component: `Show()` / `Hide()`, plus `Color` and `Width` you can change at any time
- Works on `MeshRenderer` and `SkinnedMeshRenderer` alike (including child renderers), with no separate
  duplicated mesh or GameObject
- The original material(s) are never replaced - the outline is an extra material slot, so multi-submesh
  meshes are handled correctly and the object's own shader is untouched
- No renderer found on the object (or its children)? Logs a warning once and `Show()`/`Hide()` become a
  no-op - it never throws
- One shared outline material and a `MaterialPropertyBlock` per instance - no per-object material
  instancing, no per-frame allocation

## Setup

### Requirements

- Unity 6000.3 LTS or newer, Universal Render Pipeline (the outline shader targets URP)

### Installation

Clone or download this repository, then copy its contents into `Assets/Scripts/OutlineSystem/`. Systems
that use OutlineSystem (e.g. InteractionSystem) can also download it there for you, from their setup
dialog. Either way you get the same files, visible and editable in `Assets`.

It has no dependencies, so it compiles in any project.

## Quick Start

```csharp
var outline = target.GetComponent<Outline>();
outline.Color = Color.yellow;
outline.Width = 0.02f;

outline.Show(); // e.g. on focus / hover / selection
outline.Hide(); // e.g. on lose focus
```

If `target` (or any of its children) has no `MeshRenderer`/`SkinnedMeshRenderer`, `Show()` and `Hide()`
simply do nothing - safe to call unconditionally.

## License

[MIT License](LICENSE)
