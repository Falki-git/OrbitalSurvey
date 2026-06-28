# Orbital Survey Mod — Architecture Analysis (Redux port)

Analysis of the **Orbital Survey mod** as it lives in this repo at `Assets/OrbitalSurvey/`.
The mod is **fully migrated** from the old SpaceWarp 1.x / BepInEx stack to the Redux /
SpaceWarp2 stack and from the pre-Unity 6 project into `OrbitalSurveyRedux`. The remaining
work is the **Unity 6 surface-level porting** (asset bundles, shader API, UITK API — see §6).

> Mod by **Falki** — `mod_id: OrbitalSurvey`, version `0.9.5`, depends on `SpaceWarp2 >= 2.0.0`,
> `ksp2_version` min `0.2.3`. Source: https://github.com/Falki-git/OrbitalSurvey
> Read alongside `architectural_patterns_ksp2.md` (game idioms) and
> `ksp2_assembly_csharp_reference.md` (where game types live).
> For PatchManager / Lua patch details, see `.claude/KSP2_Redux_docs/5.1 Patch Manager.md`.

---

## 1. What the mod does (one paragraph)

Adds a part module that, while a vessel is in orbit within a valid altitude band, gradually
"scans" the celestial body below it. Scanning reveals pixels of a per-body **Visual** and
**Biome** (Region) map texture; revealed pixels are painted from a hidden→scanned texture and
shown both in a custom UI window and as a **planet-surface overlay** (PQS in Flight, scaled-space
mesh in Map view). Scan coverage drives **science experiments** at 25/50/75/100% milestones.
Players can place **waypoints** on the map. Progress is saved per-save-game. Scanner altitude
bands scale with body size ("category"), and high time-warp is handled by **retroactive
("analytics") scanning** that back-fills coverage along the orbit path.

---

## 2. Project shape & toolchain

- **ThunderKit / SpaceWarp2 layout**: the mod lives under `Assets/OrbitalSurvey/` with:
  - `Code/` — all C# (compiled to `OrbitalSurvey.dll` via the `OrbitalSurvey.asmdef`).
  - `swinfo.asset` — mod metadata (`mod_id: OrbitalSurvey`, `main_assembly: OrbitalSurvey.dll`, SpaceWarp2 dep).
  - `Copied/assets/bundles/*.bundle` + `Copied/assets/images/` — runtime assets (see §4.5).
  - `Copied/patches/` — **PatchManager Lua patches** (Redux6 style — see §4.10 and §4.16).
  - `UI/` — UXML/USS source + icons.
  - `Pipelines/` — ThunderKit build pipelines (`Build for Editor`, `Build for Player`, `Deploy to Zip File`).
- **`OrbitalSurvey.asmdef`**: `overrideReferences: true` with an explicit `precompiledReferences`
  list pointing at the KSP2 game DLLs (`Assembly-CSharp.dll`, `SpaceWarp2*.dll`, `ReduxLib.dll`,
  `UitkForKsp2.dll`, `uitkforksp2.controls.Runtime.dll`, `UniTask*.dll`,
  `UnityMvvmToolkit.*.dll`, `Redux.UI.Component.dll`, …) plus a `Unity.InputSystem` reference.
  `allowUnsafeCode: true`. **No NuGet csproj** — this builds inside the Unity editor via ThunderKit.
- **Folder taxonomy** (under `Code/`): `Managers/` (singletons/orchestration), `Models/` (data),
  `Modules/` (the part-module trio), `UI/` (UITK controllers + `UI/Controls/` custom controls),
  `Utilities/` (math, save, patches, settings, helpers), `De_bug/` (debug UI + save inspection),
  `Notes/` (developer scratch — contains stale SW1.x snippets; **not compiled logic**).

### How this differs from the old SW1.x/BepInEx version (already done)
| Concern | Old (SpaceWarp 1.x / BepInEx) | This Redux port |
|---|---|---|
| Mod base | `BaseSpaceWarpPlugin` + `[BepInPlugin]` | `Redux.ExtraModTypes.KerbalMod` |
| Lifecycle | `OnInitialized()` | `OnPreInitialized()` + `OnInitialized()` |
| Logging | `BepInEx.Logging.ManualLogSource` | `ReduxLib.Logging.ILogger` via `ReduxLib.ReduxLib.GetLogger(name)` |
| Config | `BepInEx.Configuration.ConfigEntry<>` | `ReduxLib.Configuration.ConfigValue<>` wrapping `SWConfiguration.Bind(... RangeConstraint<>)` |
| AppBar | `SpaceWarp.API.UI.Appbar` | `SpaceWarp2.UI.API.Appbar` |
| Save | `SpaceWarp.API.SaveGameManager.ModSaves` | `SpaceWarp2.API.SaveGameManager.ModSaves` |
| Background EC | `SpaceWarp.API.Parts.PartComponentModuleOverride` | `SpaceWarp2.API.Parts.PartComponentModuleOverride` |
| Harmony | `Harmony.CreateAndPatchAll(typeof(Patches))` | `CreateHarmonyAndPatchAll()` (from `KerbalMod`) |
| Metadata | `MyPluginInfo` (SpaceWarp.PluginInfoProps) | `swinfo.json` + `SWMetadata.Folder` |
| UI assembly | runtime `Assembly.LoadFrom("OrbitalSurvey.Unity.dll")` | ThunderKit-bundled assets; no runtime load-from |
| Assets | `AssetManager.GetAsset<T>(addressable)` | `AssetBundle.LoadFromFile(...).LoadAsset<T>(...)` (see §4.5) |

The **gameplay, simulation, rendering, save-format, and science layers are unchanged** between
the two — only the framework plumbing above was rewritten.

---

## 3. Entry point & initialization order

`OrbitalSurveyPlugin : KerbalMod`. Split across two lifecycle hooks:

**`OnPreInitialized()`** (before assets/addressables load):
1. `Instance = this`.
2. Load the AppBar icon from disk: `SWMetadata.Folder/assets/images/icon.png` → `Texture2D`.
3. `Settings.Initialize()` — bind config via `SWConfiguration.Bind`.

**`OnInitialized()`** (game code safe to touch):
4. Register **three AppBar buttons** (Flight / OAB / KSC) → all toggle `SceneController.ToggleUI`.
5. `MessageListener.Instance.SubscribeToMessages()`.
6. Create `GameObject("OrbitalSurvey_Providers")` parented to the mod; attach MonoBehaviours
   `AssetUtility` (loads bundles in its `Start()`) and `VesselManager`.
7. `SaveManager.Instance.Register()`.
8. `PartComponentModuleOverride.RegisterModuleForBackgroundResourceProcessing<PartComponentModule_OrbitalSurvey>()`.
9. `CreateHarmonyAndPatchAll()`.

> **Note:** `CelestialCategoryManager.InitializeConfigs()` is **not** called in `OnInitialized()`.
> It is deferred to `MessageListener.OnGameLoadFinishedMessage` so that PatchManager has already
> finished its Lua patch pass and bound all config values before the C# reads them from
> `SWConfiguration`. `InitializeCelestialBodyCategories()` is also called there.

`Update()` toggles the debug window (Ctrl+Alt+O); `OnGUI()` drives the IMGUI debug window —
i.e. `KerbalMod` is itself a MonoBehaviour (this is why `KerbalMod` is used over `GeneralMod`).

---

## 4. Dominant patterns

### 4.1 Singleton managers
Nearly every manager is a singleton: `Core`, `MessageListener`, `OverlayManager`,
`CelestialCategoryManager`, `SaveManager`, `ScienceManager`, `SceneController`, `DebugUI`,
`NotificationUtility`. Flavors: eager static (`= new()`), lazy null-checked, and **MonoBehaviour
singletons** set in `Start()` (`VesselManager`, `AssetUtility`) — the latter need the Unity loop
and are attached to the providers GameObject.

### 4.2 `Core` as central orchestrator + the data model
`Core` owns `CelestialDataDictionary` (`body name → CelestialData`). `CelestialData` holds a
`Dictionary<MapType, MapData>` (Visual + Biome) plus the `CelestialBodyComponent`.
`Core.DoScan(...)` is the single funnel for all scanning; `Core` also exposes
`GetBodiesContainingData()`, `ClearMap`, and the `OnMapHasDataValueChanged` event. Maps are
initialized lazily on `GameLoadFinishedMessage` (`InitializeCelestialData`), pulling textures
from `AssetUtility.GetTextureAsset(AssetType, key)`.

### 4.3 The part-module trio (KSP2 standard, see ksp2 patterns doc §3)
| Class | Base | Responsibility |
|-------|------|----------------|
| `Module_OrbitalSurvey` | `PartBehaviourModule` | PAM UI (status/mode/altitudes/percent), AppBar action, OAB part-info, deploy gating, PAM visibility per `PartBackingMode` (Flight vs OAB). |
| `PartComponentModule_OrbitalSurvey` | `PartComponentModule` | The sim loop: `OnUpdate` → EC consumption, status/state update, `DoScan`, retroactive scanning. Registers itself with `VesselManager`. |
| `Data_OrbitalSurvey` | `ModuleData` | Serialized `ModuleProperty<>` fields (PAM-bound + `[KSPState]` saved), `[KSPDefinition]` config values set by PatchManager, EC `ResourceFlowRequest` setup, `GetPartInfoEntries` (OAB tooltip). |

Data-flow detail: `[KSPDefinition]` "*Value" fields (`ModeValue`, `ScanningFieldOfViewValue`) are
set by PatchManager and copied into the display `ModuleProperty<>` in `OnPartBehaviourModuleInit`
— a workaround for a stock bug where read-only `[KSPDefinition]` properties spawn as editable
sliders in Flight. The component module gates scanning on `Data_Deployable.IsExtended` and a
`PartComponentModule_ScienceExperiment` (looked up via `Part.TryGetModule`).

### 4.4 The scanning algorithm (`ScanUtility` + `MapData`)
- **Map texture model** (`MapData`): three textures per map — `ScannedMap` (revealed art),
  `HiddenMap` (unrevealed fill), `CurrentMap` (displayed, painted progressively) — plus
  `ScannedMapHiRes` for the completed reveal, and a `bool[,] DiscoveredPixels` coverage grid at
  `Settings.ActiveResolution` (1024). `PercentDiscovered = DiscoveredPixelsCount / TotalPixelCount`;
  **≥95% counts as fully scanned** (`SetAsFullyScanned` swaps in the 2048 hi-res texture).
- **Scan geometry** (`ScanUtility.GetScanRadius`): treats the scanner as a cone with `FieldOfView`;
  Law of Sines gives the surface arc seen from `altitude` above `bodyRadius`, clamped to 90°,
  scaled by `GetMinMaxReductionFactor` (full at ideal altitude, fading to 0 at min/max).
- **Geo → texture**: equirectangular — `x = W·(lon+180)/360`, `y = H·(lat+90)/180`.
- **Mercator/aspect correction**: textures are 1:1 but the map is 2:1, and longitude pixels
  compress toward the poles; `MarkAsScanned` halves the horizontal radius and divides by
  `cos(latitude)`, wrapping x at the antimeridian.
- **Painting**: per newly-discovered pixel, copy color `ScannedMap → CurrentMap`, flip
  `DiscoveredPixels`, then `CurrentMap.Apply()` once per scan.

### 4.5 Asset loading via raw `AssetBundle` (Unity-6-sensitive — see §6)
`AssetUtility` (MonoBehaviour) loads **four asset bundles** from `SWMetadata.Folder/assets/bundles/`
in `Start()`: `orbitalsurvey_maps_1024.bundle`, `orbitalsurvey_maps_2048.bundle`,
`orbitalsurvey_ui.bundle`, `swconsoleui.bundle`. `GetTextureAsset(AssetType, key)` resolves a
logical key (e.g. `"Kerbin_1024"`) to a bundle address (e.g.
`assets/orbitalsurvey/images/visualmaps/kerbin_scaled_d_1024.png`) and calls
`bundle.LoadAsset<Texture2D>(address)`. Visual/Biome/Other address dictionaries are hardcoded.
`GenerateHiddenMap()` copies the hidden-map texture; `GetGUISkin()` loads a `GUISkin` from the
console bundle. (The old `GameManager.Instance.Assets.Load<>`/`AssetManager` Addressables path is
commented out / only in `Notes/`.)

### 4.6 Retroactive / "analytics" scanning (time-warp catch-up)
At high warp, `OnUpdate` ticks are sparse, so `PerformRetroactiveScanningIfNeeded` walks the
orbit from `LastScanTime` to now in steps of `GetRetroactiveTimeBetweenScans` (2/5/10 s buckets),
using `OrbitUtility.GetOrbitalParametersAtUT` to sample past lat/lon/alt. Retroactive pixels are
**buffered** in `MapData` and applied in one batch (perf).

### 4.7 Overlay rendering (two render paths) — `OverlayManager`
- **Flight (PQS)**: finds the body transform under `#PhysicsSpace/#Celestial`, clones the PQS
  surface material onto shader `KSP2/Environment/CelestialBody/CelestialBody_Local_Old`, sets
  `_AlbedoScaledTex` = `CurrentMap`, and adds a custom `OrbitalSurveyOverlay` via
  `PQSRenderer.AddOverlay` (removed by scanning `PQSRenderer._overlays`). Oceans blacked out via
  `_ShorelineSDFTexture`.
- **Map view (scaled space)**: on `MapCelestialBodyAddedMessage`, swaps the body mesh's `_MainTex`
  to `CurrentMap` (backing up the original) and disables cloud/atmosphere child meshes; re-applies
  after zoom-out/in recreates scaled-space objects. Optional "always show overlay" hides unscanned
  bodies in Map view.

### 4.8 Save/Load via `SpaceWarp2.API.SaveGameManager.ModSaves`
`SaveManager.Register()` wires `OnSave`/`OnLoad` with a `SaveDataAdapter` DTO (per-body per-maptype
`DiscoveredPixels` compressed string + `IsFullyScanned` + `ExperimentLevel`, plus serialized
waypoints, window position, session GUID). `DiscoveredPixels` is compressed by
`SaveUtility.CompressData/DecompressData`. Load is **buffered** (`HasBufferedLoadData`) because save
callbacks can fire before textures initialize; `Core.InitializeCelestialData` flushes the buffer.
Waypoint load waits until referenced bodies exist in `UniverseModel`.

### 4.9 Science integration — `ScienceManager`
Maps `(MapType, ExperimentLevel)` → stock experiment IDs (e.g.
`orbital_survey_visual_mapping_high_25`). When `MapData.CheckIfExperimentNeedsToBeTriggered`
crosses a milestone, `TriggerExperiment` builds a `ResearchReport` (pulling
`CelestialBodyScalar`/`HighOrbitScalar` from `ScienceRegionsDataProvider._cbToScienceRegions`),
forces body/situation/region, stores it on every participating vessel's
`PartComponentModule_ScienceExperiment`, and publishes `ResearchReportAcquiredMessage`.

### 4.10 Config-driven definitions via PatchManager (Lua patches)
`CelestialCategoryManager` reads the celestial-body category definitions (max-radii,
per-category per-maptype scanning altitudes, category localization strings) from the mod's
own **`SWConfiguration` (`IConfigFile`)**, which is populated by the Lua patch
`Copied/patches/orbital_survey_definitions.lua` at game load via PatchManager's `Config:`
API:
```lua
Config:Integer("orbital-survey-category-max-radius", "Small",  150000, "…")
Config:String("orbital-survey-category-localization", "Small", "PartModules/…", "…")
Config:Integer("orbital-survey-category-altitudes", "Small.Visual.Min", 60000, "…")
```
The three config sections are `orbital-survey-category-max-radius`,
`orbital-survey-category-localization`, and `orbital-survey-category-altitudes`. Because these
values only exist after PatchManager has run, `InitializeConfigs()` is called from
`MessageListener.OnGameLoadFinishedMessage`, not `OnInitialized()`. Other mods can override
individual values with `Config:Mod("OrbitalSurvey")` in their own patches. Bodies are binned
into size categories by radius at game load.

The four active Lua patches (under `Copied/patches/`):
| File | What it does |
|------|-------------|
| `orbital_survey_definitions.lua` | Writes category/altitude config into `SWConfiguration` |
| `orbital_survey_module.lua` | Adds `Module_OrbitalSurvey` to antenna parts |
| `orbital_survey_experiments.lua` | Creates the 8 mapping-milestone science experiments |
| `orbital_survey_add_experiments_to_parts.lua` | Attaches those experiments to the relevant parts |

### 4.11 Messaging — game `MessageCenter` (`MessageListener`)
`PersistentSubscribe` to: `GameLoadFinishedMessage` (init maps/categories/debug),
`GameStateChangedMessage` (close GUI + remove overlay on scene changes except Flight↔Map),
`MapCelestialBodyAddedMessage` (apply Map-view overlay when scaled space loads).

### 4.12 UI — UitkForKsp2 (UXML + MonoBehaviour controllers + custom controls)
`SceneController` builds/destroys the main window via `Window.Create(WindowOptions, VisualTreeAsset)`
(from `UitkForKsp2.API`) and attaches a controller component (`MainGuiController`), toggling AppBar
button state. Sub-controllers: `VesselController`, `WaypointController`, `ZoomAndPanController`,
`ResizeController`, `MouseOverController`. Custom UITK controls live in `UI/Controls/`
(`LegendKeyControl`, `MapMarkerControl`, `SideToggleControl`). `Uxmls` caches the loaded
`VisualTreeAsset`s. The asmdef references `UnityMvvmToolkit.*` and `uitkforksp2.controls.Runtime`.

### 4.13 Event-driven vessel tracking — `VesselManager` (MonoBehaviour)
Tracks all module-bearing vessels for the UI. Modules self-register in
`PartComponentModule_OrbitalSurvey.OnStart`. A throttled `Update` (every `GuiRefreshInterval`)
detects destroyed/recovered vessels, SOI changes, decouple/dock re-parenting, and enabled/status/
state changes, firing fine-grained C# events (`VesselStats`/`ModuleStats` with per-field change
events) that the UI subscribes to.

### 4.14 Localization & Settings
`LocalizationStrings` centralizes I2.Loc `LocalizedString` lookups and `MODE_TYPE_TO_MAP_TYPE`
(config `ModeValue` string → `MapType` enum). `Settings` binds typed `ConfigValue<>` (scan
interval, UI sounds, always-show overlay, region legend, UI refresh, zoom factor/max, retroactive
buckets) through `SWConfiguration.Bind` with `RangeConstraint<>`.

### 4.15 Harmony patches — `Patches` / `DebugPatches`
Used to *suppress* stock behavior: hide auto-experiment PAM entries
(`Module_ScienceExperiment.InitializePAMItems`/`UpdatePAM`), de-duplicate OAB experiment
descriptions (`Data_ScienceExperiment.GetPartInfoEntries`), and a full reimplementation of
`PartComponentModule_ScienceExperiment.RefreshLocationsValidity` to silence log spam.

---

## 5. Key types map

| File (`Code/…`) | Type | Role |
|------|------|------|
| `OrbitalSurveyPlugin.cs` | `OrbitalSurveyPlugin : KerbalMod` | Entry point, AppBar, wiring. |
| `Managers/Core.cs` | `Core` | Orchestrator + `CelestialDataDictionary`, `DoScan`. |
| `Managers/MessageListener.cs` | `MessageListener` | Game MessageCenter subscriptions. |
| `Managers/OverlayManager.cs` | `OverlayManager` | PQS + scaled-space overlays. |
| `Managers/SaveManager.cs` | `SaveManager` | ModSaves save/load + buffering. |
| `Managers/ScienceManager.cs` | `ScienceManager` | Milestone experiments. |
| `Managers/CelestialCategoryManager.cs` | `CelestialCategoryManager` | PatchManager config → altitudes/categories. |
| `Managers/RegionsManager.cs` | `RegionsManager` | Biome/region legend data. |
| `Managers/VesselManager.cs` | `VesselManager` (MonoBehaviour) | Tracks module-bearing vessels for UI. |
| `Modules/Module_OrbitalSurvey.cs` | `: PartBehaviourModule` | View / PAM / OAB. |
| `Modules/PartComponentModule_OrbitalSurvey.cs` | `: PartComponentModule` | Sim loop + scanning. |
| `Modules/Data_OrbitalSurvey.cs` | `: ModuleData` | Serialized config/state + EC. |
| `Models/CelestialData.cs` / `MapData.cs` | data | Per-body / per-map texture + coverage model. |
| `Models/Enums.cs` | enums | `MapType`, `Status`, `State`, `ExperimentLevel`, … |
| `Models/SaveDataAdapter.cs` | DTO | Serialized save payload. |
| `Utilities/AssetUtility.cs` | `AssetUtility` (MonoBehaviour) | Loads asset bundles; resolves texture assets. |
| `Utilities/ScanUtility.cs` | static | Scan geometry + projection math. |
| `Utilities/OrbitUtility.cs` | static | Sample lat/lon/alt at a past UT (retroactive). |
| `Utilities/SaveUtility.cs` | static | Coverage-grid (de)compression. |
| `Utilities/Patches.cs` | Harmony | Suppress/adjust stock science UI/log. |
| `Utilities/Settings.cs` | static | ReduxLib config values. |
| `UI/SceneController.cs` + `UI/*Controller.cs` + `UI/Controls/*` | UITK | Window lifecycle, sub-controllers, custom controls. |

---

## 6. What the Unity 6 port still needs

The Orbital Survey source code has been imported into this repo and the PatchManager patches
have been migrated to Lua. The C# **gameplay logic is current** for the Redux/SpaceWarp2 stack.
Focus the remaining work on the Unity-version-sensitive surfaces:

1. **Rebuild the asset bundles under Unity 6.** The four bundles in `Copied/assets/bundles/`
   (`orbitalsurvey_maps_1024`, `orbitalsurvey_maps_2048`, `orbitalsurvey_ui`, `swconsoleui`)
   were built with the pre-Unity 6 editor. Unity 6 bundles use a different serialization version
   and **old bundles may fail to load** (`AssetBundle.LoadFromFile` returns null / type-load
   errors). Re-export via the ThunderKit pipelines in this repo. Verify
   `bundle.GetAllAssetNames()` still matches the hardcoded addresses in `AssetUtility` (e.g.
   `assets/orbitalsurvey/images/visualmaps/kerbin_scaled_d_1024.png`).
2. **Regenerate `OrbitalSurvey.asmdef precompiledReferences`** against this repo's Unity 6
   game DLLs in `Packages/KSP2_x64/`. The reference list may differ from the pre-Unity 6 build
   — reconcile against the `MyTest11` asmdef.
3. **Shader / material APIs** (`OverlayManager`): confirm `KSP2/Environment/CelestialBody/
   CelestialBody_Local_Old` still exists in the Unity 6 build and that `_AlbedoScaledTex` /
   `_ShorelineSDFTexture` property names and the `PQSRenderer._overlays` / `AddOverlay` /
   `_oceanMaterial` internals are unchanged. These are the highest-risk reach-ins.
4. **UI Toolkit (UITK) API changes** in Unity 6: `Window.Create` and custom control
   `UxmlFactory`/`UxmlTraits` registration in `UI/Controls/` may hit renamed members.
5. **Texture2D / Graphics APIs**: `Texture2D.SetPixel/Apply`, `Graphics.CopyTexture` usage in
   `MapData`/`ScanUtility` are stable but worth a compile-time check against Unity 6 signatures.
6. **Deprecated Unity APIs / Input**: `Input.GetKey*` (legacy input) in the debug hotkey may
   warn; the asmdef already references `Unity.InputSystem`. `OnGUI`/`GUISkin` (IMGUI debug
   window) remains supported.

Build and deploy via the ThunderKit pipelines (`Build for Editor` / `Build for Player` /
`Deploy to Zip File`) under `Assets/OrbitalSurvey/Pipelines/`. Pipelines run in the Unity
editor, not from a terminal — see `CLAUDE.md` for the build workflow.

---

## 7. Notable design decisions & gotchas (preserve through the port)

- **95% = 100%**: scanning completes at 95% coverage to avoid the frustrating last-few-pixels near
  the poles (Mercator distortion). Don't regress this.
- **Buffered load**: save data can arrive before textures exist — keep the buffer/flush handshake.
- **Two scan paths**: real-time vs retroactive (buffered) — both must paint identically.
- **Ocean blackout** during overlay so water doesn't bleed through the map texture.
- **Resolution fixed at 1024** active / 2048 hi-res; coverage grid is `bool[1024,1024]`.
- **EC handled in background** so unloaded vessels keep scanning (background resource processing).
- **`VesselManager` reparenting logic** (decouple/dock) is intricate and event-heavy — port carefully.
- `Notes/` contains **stale SpaceWarp 1.x snippets** (e.g. `AssetManager.GetAsset`) — reference only,
  do not resurrect; `De_bug/` is dev tooling, not core gameplay.
