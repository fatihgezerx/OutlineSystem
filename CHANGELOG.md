# Changelog

## [1.0.0] - 2026-09-27

### Added
- `Outline` component: `Show()`, `Hide()`, `Color`, `Width`.
- Inverted-hull outline shader (`Hidden/OutlineSystem/Outline`), URP-targeted, appended as an extra
  material slot so the object's own material is never touched.
- Works on `MeshRenderer` and `SkinnedMeshRenderer`, found on the object itself or its children.
- Graceful no-op (with a single warning) when no renderer is found.
