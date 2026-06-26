# KSP2 `Assembly-CSharp.dll` Reference

Deep reference for `Packages/KSP2_x64/Assembly-CSharp.dll` — the assembly that contains
**almost the entire KSP2 codebase**. This document is a navigation map: it tells you which
namespaces hold what, and how to pull exact signatures on demand. Architectural idioms are in
`architectural_patterns_ksp2.md`; this file is the "where does X live" index.

## Facts

- **File:** `Packages/KSP2_x64/Assembly-CSharp.dll` (~13 MB)
- **Type count:** **7,194 types** (incl. compiler-generated closures `<>c`, iterators `d__`).
- **Engine:** Unity `6000.4.1f1`. Targets the game's own runtime, references many other
  DLLs in `Packages/KSP2_x64/` (`SpaceWarp2.*`, `ReduxLib`, `UitkForKsp2`, `0Harmony`,
  Unity modules, `Newtonsoft.Json`, etc.).
- **Publicization:** the shipped copy keeps original access modifiers. Run `publicize.bat`
  (NStrip) to produce an all-public copy for compiling against internals. **Currently NOT
  publicized** in this repo.
- **This is the Redux community build** — it contains both stock `KSP.*` code and Redux
  additions (`Redux.*`, `Redux.Ksp1Import.*`, `Redux.Modules.*`, `VSwift.*`).

## How to inspect it

```bash
export PATH="$PATH:$HOME/.dotnet/tools"     # ilspycmd installed as a global dotnet tool

ilspycmd -l c  Packages/KSP2_x64/Assembly-CSharp.dll          # list all types
ilspycmd -t KSP.Sim.impl.VesselComponent  Packages/KSP2_x64/Assembly-CSharp.dll   # one type
ilspycmd       Packages/KSP2_x64/Assembly-CSharp.dll -o ./out  # full decompile to a folder (huge; avoid)
```
Filter the type list with grep, e.g. find every part module:
```bash
ilspycmd -l c Packages/KSP2_x64/Assembly-CSharp.dll | sed 's/^Class //' \
  | grep -E '^KSP\.Modules\.Module_' | grep -v '<'
```
> Tip: decompiling a closed type may pull in nested compiler-generated classes (`<>c`,
> `<Method>d__NN`) — ignore those, they're lambdas/iterators, not real API.

---

## Namespace map (by type count — the parts that matter for modding)

| Namespace | ~Types | What's in it |
|-----------|-------:|--------------|
| `KSP.Messages` | 453 | **Event/message types** for the `MessageCenter` pub/sub bus. The hook surface for reacting to game events. |
| `RTG` | 332 | Runtime gizmos / transform-handle tooling (editor-style manipulation). Rarely needed by gameplay mods. |
| `KSP.UI` | 202 | Stock UI controllers/widgets (IMGUI + legacy UI). |
| `KSP.Sim.impl` | 200 | **The simulation model** — `VesselComponent`, `PartComponent`, `CelestialBodyComponent`, `OrbiterComponent`, `PatchedConicsOrbit`, `UniverseModel`, `SpaceSimulation`, all `PartComponentModule_*`. The heart of gameplay logic. |
| `KSP.OAB` | 170 | Orbital Assembly Building (the VAB/editor) — parts placement, assembly. |
| `KSP.Game` | 160 | `GameInstance`, `GameManager`, managers, session/save, science, missions glue. |
| `KSP.Messages.PropertyWatchers` | 154 | Messages that fire when a watched property changes (orbit params, speeds…). |
| `KSP.ScriptInterop.impl.moonsharp` | 136 | Lua scripting interop (MoonSharp). |
| `VehiclePhysics` | 126 | Vehicle physics solver internals. |
| `KSP.Modules` | 119 | **`Module_*` part behaviours** (view side) + module data registries. |
| `KSP.UI.Binding` (+`.Core`/`.Widget`) | ~170 | Data-binding layer for stock UI. |
| `KSP.Sim` | 77 | Sim-layer interfaces/value types (`VesselSituations`, orbit/position types, `IPatchedOrbit`). |
| `KSP.Game.Load` | 69 | Load sequence flow actions. |
| `KSP.Rendering` (+`.Planets`/`.impl`) | ~109 | Rendering systems incl. planet/PQS rendering. |
| `KSP.VFX` | 61 | Visual effects. |
| `KSP.Map` | 44 | **Map view** model/controllers — relevant for map overlays. |
| `KSP.Game.Missions` | 40 | Missions/contracts. |
| `KSP.UI.Flight` | 38 | Flight HUD UI. |
| `KSP.Game.Science` | 25 | Science system. |
| `KSP.Sim.ResourceSystem` | 25 | **Resource flow** — `ResourceFlowRequestBroker`, `ResourceContainerGroup`, definitions. |
| `KSP.Sim.Definitions` | 24 | **`ModuleData`, `PartBehaviourModule`** base types + serialized part defs. |
| `KSP.Navigation` | 27 | Waypoints / nav. |
| `KSP.Rendering.Planets` | 32 | PQS / planet surface rendering (relevant for surface overlays). |
| `KSP.DebugTools` | 29 | In-game debug UI. |
| `KSP.Networking.*` | ~50 | Multiplayer scaffolding. |
| `Redux.*` | many | **Community-build additions** (see below). |
| `I2.Loc` | 60 | Localization (I2 Localization). |
| `LibNoise(.Modifiers)` | ~32 | Procedural noise (terrain/resource maps). |
| `SpaceGraphicsToolkit`, `EdyCommonTools`, `AwesomeTechnologies.*`, `iT`(iTween), `PA.ParticleField`, `UnityEngine.PostProcessing` | — | Third-party libraries embedded into the assembly. |

> The single-word top-level types (no namespace) include many MonoBehaviours like
> `AeroUtilities`, `AdvancedRagdoll`, `ReduxAudioBehavior`, etc. Search by exact name when needed.

---

## Redux community additions (`Redux.*`)

The Redux build layers extra systems on top of stock KSP2. Notable for this project:

- **`Redux.Modules`** — new part modules following the dual pattern:
  - `PartComponentModule_ResourceScanner` (+ `Module_ResourceScanner`, `Data_ResourceScanner`)
    — **closest existing analog to an orbital survey scanner**; study it first.
  - `PartComponentModule_Mine`, `PartComponentModule_Camera`,
    `PartComponentModule_UniversalContainer`, `PartComponentModule_PartAudioPreset`.
- **`Redux.WorldVis`** — `PqsOverlay`, `PqsVisSetup` — **planet-surface overlay rendering**;
  precedent for drawing a discovered-resource map onto a body.
- **`Redux.Ksp1Import.*`** — importer that maps KSP1-style part configs/modules into KSP2
  (`Ksp1PartModuleImporterRegistry`, `Ksp1PartComponentModule_*`). Useful context if the
  original Orbital Survey mod (a KSP1/SpaceWarp-1.x era mod) is being ported.
- **`Redux.ReduxPlugin`, `Redux.ReduxHooks`, `Redux.ReduxAssets`, `Redux.Constants`,
  `Redux.UIUtil`, `Redux.ReduxResourceAdapter`** — loader/glue/util surface.
- **`Redux.VFX.Plume`, `Redux.VFX.ReentryMeshGeneration`** — VFX systems.
- **`VSwift.Modules.*`** — engine/part-switch modules (V-Swift, the Redux engine module set).

---

## The types you'll touch most for Orbital Survey

| Type | Namespace | Why |
|------|-----------|-----|
| `GameInstance` | `KSP.Game` | Service locator (`GameManager.Instance.Game`). |
| `GameManager` | `KSP.Game` | Static singleton entry. |
| `MessageCenter` | `KSP.Messages` | Subscribe to game events. |
| `VesselComponent` | `KSP.Sim.impl` | Orbit, body, lat/long, altitude, situation. |
| `CelestialBodyComponent` | `KSP.Sim.impl` | Body being scanned (radius, name, …). |
| `PatchedConicsOrbit` / `OrbiterComponent` | `KSP.Sim.impl` | Orbit geometry. |
| `PartComponent` / `PartOwnerComponent` | `KSP.Sim.impl` | Part + vessel part container. |
| `PartComponentModule` | `KSP.Sim.impl` | Base for sim-side scanner logic. |
| `PartBehaviourModule` / `ModuleData` | `KSP.Sim.Definitions` | Base for view-side + serialized data. |
| `ResourceFlowRequestBroker`, `ResourceContainerGroup`, `ResourceDefinitionDatabase` | `KSP.Sim.ResourceSystem` / `KSP.Game` | EC consumption while scanning. |
| `MapProvider`, `KSP.Map.*` | `KSP.Game` / `KSP.Map` | Map view + overlay placement. |
| `Redux.Modules.PartComponentModule_ResourceScanner` | `Redux.Modules` | Reference implementation. |
| `Redux.WorldVis.PqsOverlay` | `Redux.WorldVis` | Surface overlay precedent. |
| Messages: `GameStateChangedMessage`, `VesselChangedMessage`, `MapViewEnteredMessage`, `ObserverSOIChangedMessage` | `KSP.Messages` | Lifecycle hooks. |

---

## Sibling assemblies in `Packages/KSP2_x64/` worth decompiling

| DLL | Contains |
|-----|----------|
| `SpaceWarp2.dll` | Mod API: `GeneralMod`, `ISpaceWarpMod`, asset/localization APIs. |
| `SpaceWarp2.Game.dll` | Game-side SW2 helpers (waypoints, etc.). `.UI`, `.Sound`, `.VersionChecking` split out. |
| `ReduxLib.dll` | Loader, `ReduxLib.Logging.*`, `ReduxLib.Messaging.MessageBus`, config. |
| `UitkForKsp2.dll` | UI Toolkit MVVM framework (`API.Dialog`, `PanelFactory`, `ViewModelBase`, bindings/converters). |
| `Assembly-CSharp-firstpass.dll` | Plugins compiled in Unity's first pass (third-party that other code depends on). |
| `0Harmony.dll` | Harmony patching library. |
| `KSPLogging.dll`, `Newtonsoft.Json*.dll` | Logging + JSON. |

---

## Quick recipes

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
A="Packages/KSP2_x64/Assembly-CSharp.dll"

# Find a type by partial name
ilspycmd -l c "$A" | sed 's/^Class //' | grep -i resourcescanner | grep -v '<'

# Public surface of a type only
ilspycmd -t KSP.Sim.impl.VesselComponent "$A" | grep -E '^\s*public '

# All message types
ilspycmd -l c "$A" | sed 's/^Class //' | grep '^KSP.Messages\.' | grep -v '<'

# All stock part-behaviour modules
ilspycmd -l c "$A" | sed 's/^Class //' | grep '^KSP.Modules.Module_'
```
