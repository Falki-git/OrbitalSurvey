# KSP2 Architectural Patterns

Reference for the recurring architectural patterns in KSP2 (`Assembly-CSharp.dll`) and the
Redux/SpaceWarp2 modding stack. Use this when designing or migrating mod code so it matches
the game's idioms. All type names verified by decompiling the shipped assemblies.

---

## 1. Service-locator via `GameInstance`

KSP2 centralizes nearly every subsystem on a single object, `KSP.Game.GameInstance`,
reached through the static singleton `KSP.Game.GameManager.Instance.Game`.

```csharp
var game = GameManager.Instance.Game;            // KSP.Game.GameInstance
var messages = game.Messages;                    // KSP.Messages.MessageCenter
var bodies   = game.CelestialBodies;             // CelestialBodyProvider
var map      = game.Map;                          // MapProvider
var resDb    = game.ResourceDefinitionDatabase;   // ResourceDefinitionDatabase
```

`GameInstance` exposes (non-exhaustive): `Messages`, `GlobalGameState` (GameStateMachine),
`Input`/`InputManager`, `Parts` (PartProvider), `Map`, `OAB`, `CelestialBodies`,
`PartsManager`, `ActionGroupManager`, `ResourceManager` (UI), `ResearchManager`,
`Notifications`, `SessionManager`, `SaveLoadManager`, `UniverseModel`, `SpaceSimulation`,
`ResourceDefinitionDatabase`, `Units`, `PhysicsSettings`.

**Pattern for mods:** don't cache `GameInstance` across scene loads; fetch it when needed,
and null-check (`GameManager.Instance.Game == null`) — it doesn't exist on early boot.
It also defines a large set of `DEFAULT_*_PRIORITY` int constants used to order update
drivers / behaviours.

---

## 2. Messaging: the `MessageCenter` pub/sub bus

`KSP.Game.GameInstance.Messages` is a `KSP.Messages.MessageCenter`. All cross-system events
flow through it. There are **453 message types** in `KSP.Messages`.

```csharp
SubscriptionHandle h = game.Messages.Subscribe<VesselChangedMessage>(OnVesselChanged);
// ... later:
game.Messages.Unsubscribe(ref h);

void OnVesselChanged(MessageCenterMessage msg) {
    var m = (VesselChangedMessage)msg;
    // ...
}
```

API surface:
- `Subscribe<T>(Action<MessageCenterMessage>)` → returns `SubscriptionHandle`.
- `PersistentSubscribe<T>(...)` — survives across game-state transitions (use for mod-global listeners).
- `FiniteSubscribe<T>(...)` — auto-unsubscribes after first delivery.
- `Publish<T>()` / `Publish<T>(T msg)` / `Publish(Type, msg)`.
- `Unsubscribe(ref handle)` or `Unsubscribe<T>(callback)`.
- `CreateMessage<T>()` / `TryCreateMessage<T>(out T)` to build messages from a pool.

**Messages relevant to an orbital-survey mod:** `GameStateChangedMessage`,
`GameStateEnteredMessage`/`GameStateLeftMessage`, `GameLoadFinishedMessage`,
`VesselChangedMessage`, `MapViewEnteredMessage`/`MapViewLeftMessage`,
`FlightViewEnteredMessage`/`FlightViewLeftMessage`, `ObserverSOIChangedMessage`,
`MapOrbitAddedMessage`/`MapOrbitRemovedMessage`.

`KSP.Messages.PropertyWatchers.*` are a related family that fire on a watched value change
(e.g. `OrbitEccentricity`, `OrbitInclination`, `VesselOrbitSpeedPropertyWatcher`).

> ReduxLib also has its own lower-level `ReduxLib.Messaging.MessageBus` used by the loader.
> For gameplay events use the game's `MessageCenter`, not the ReduxLib bus.

---

## 3. The dual part-module pattern (Behaviour + Component + Data)

This is **the** core pattern for adding part functionality in KSP2. A single part feature
is split across three classes:

| Class | Base | Lives in | Role |
|-------|------|----------|------|
| `Module_X`           | `KSP.Sim.Definitions.PartBehaviourModule` | View / Unity (`KSP.Modules`) | MonoBehaviour-side: visuals, PAM (Part Action Menu) UI, animations, in-editor + active-vessel presentation. |
| `PartComponentModule_X` | `KSP.Sim.impl.PartComponentModule`     | Simulation (`KSP.Sim.impl`)   | Headless simulation logic; runs even when the part has no loaded GameObject (e.g. background/unloaded vessels). |
| `Data_X`             | `KSP.Sim.Definitions.ModuleData`          | Definitions                   | Serialized configuration/state shared between the two (resource costs, rates, flags). |

`PartComponentModule` (abstract) key surface:
```csharp
public abstract Type PartBehaviourModuleType { get; }   // points at the Module_X view type
public ModuleDataList DataModules;                       // holds the Data_X instances
public PartComponent Part { get; }                       // owning part
public GameInstance Game { get; }
public bool IsEnabled { get; }
public virtual void OnStart(double universalTime);
public virtual void OnUpdate(double universalTime, double deltaUniversalTime);
public virtual void OnShutdown();
public virtual void OnFinalizeCreation(double universalTime);
public virtual void ThermalUpdate(double deltaTime);
```

Typical component lifecycle (mirrors the shipped `Redux.Modules.PartComponentModule_ResourceScanner`):
```csharp
public override void OnStart(double universalTime) {
    if (!DataModules.TryGetByType<Data_ResourceScanner>(out _data)) { /* error */ return; }
    _resourceDB = Game.ResourceDefinitionDatabase;
    _containerGroup = Part.PartOwner.ContainerGroup;     // resource storage
    Part.RegisterModuleForUpdates(this);                 // opt into OnUpdate ticks
}
public override void OnShutdown() => Part.UnregisterModuleForUpdates(this);
```

**Existing Redux modules to study before writing new ones** (in `KSP.Sim.impl` / `KSP.Modules`
/ `Redux.Modules`): `PartComponentModule_ResourceScanner`, `PartComponentModule_Mine`,
`PartComponentModule_Camera`, `PartComponentModule_UniversalContainer`. The stock module
catalog (`Module_Engine`, `Module_SolarPanel`, `Module_DataTransmitter`,
`Module_ResourceConverter`, `Module_Generator`, `Module_ScienceExperiment`, …) shows the
canonical patterns to copy.

**Resources** flow through `KSP.Sim.ResourceSystem` — `ResourceFlowRequestBroker`,
`ResourceContainerGroup`, `ResourceDefinitionDatabase`. A scanner that consumes EC requests
it through `Part.PartResourceFlowRequestBroker` (exposed as `resourceFlowRequestBroker`).

---

## 4. Simulation vs. view split (`KSP.Sim.impl`)

KSP2 separates the *simulated universe* from its *rendered representation*:

- **Simulation** (`KSP.Sim.impl`, 200 types): `UniverseModel`, `SpaceSimulation`,
  `VesselComponent`, `PartComponent`, `PartOwnerComponent`, `CelestialBodyComponent`,
  `OrbiterComponent`, `PatchedConicsOrbit`. These run for *all* objects, loaded or not.
- **View / behaviour**: `*Behavior`/`*ViewController` MonoBehaviours that exist only for
  loaded objects and present the sim state.

`VesselComponent` is the most important sim type for an orbital-survey mod:
```csharp
vessel.Orbit;                 // PatchedConicsOrbit (via Orbiter)
vessel.mainBody;              // CelestialBodyComponent currently orbited
vessel.Latitude / Longitude;  // sub-vessel ground coordinates
vessel.AltitudeFromRadius / AltitudeFromTerrain / AltitudeFromSeaLevel;
vessel.Situation;             // VesselSituations enum (Orbiting, SubOrbital, Landed, …)
```
Use these to decide whether a vessel is in a valid scanning orbit and which body/lat-long it
is currently passing over.

---

## 5. Flow actions & state machine

Long/async game operations use `KSP.Game.Flow.FlowAction` — a command object with
`DoAction(Action resolve, Action<string> reject)`, often chained and driven by coroutines
(e.g. `GameManager`'s shutdown/scene-load actions). Game phases are governed by
`GameStateMachine` (`GlobalGameState`); subscribe to `GameStateChangedMessage` /
`GameStateEnteredMessage` to react to entering Flight, Map, OAB (vehicle assembly), etc.

---

## 6. Mod entry point & lifecycle (SpaceWarp2)

```csharp
using SpaceWarp2.API.Mods;
public class OrbitalSurveyPlugin : GeneralMod {        // or KerbalMod for MonoBehaviour loop
    public override void OnPreInitialized()  { /* register asset/loader actions */ }
    public override void OnInitialized()     { /* assets loaded; Harmony patch; hook MessageCenter */ }
    public override void OnPostInitialized() { /* other mods are up */ }
}
```
- `GeneralMod` is a plain lifecycle object; `KerbalMod` (per template comment) is the variant
  to extend when you need the MonoBehaviour `Update` loop and direct game references like
  legacy SpaceWarp 1.x mods.
- Provided members: `SWLogger`, `SWConfiguration`, `SWMetadata`, `CreateHarmonyAndPatchAll()`.
- Mod metadata is declared in `swinfo.json` (id, version, dependencies, `main_assembly`).

---

## 7. Assets, addressables & localization

- Game content ships as **Addressable asset bundles** (`Redux/Addressables/...`). Mods load
  their own bundles via SpaceWarp2's asset API (register in `OnPreInitialized`).
- Localization uses **I2.Loc** (`I2.Loc.*`) in the base game; SpaceWarp2/UitkForKsp2 provide
  localization converters (`LocalizationConverter`, `DocumentLocalization`). Mod localization
  files live under each mod's `localizations/` folder.

---

## 8. UI: UI Toolkit via `UitkForKsp2`

Modern mod UI uses Unity UI Toolkit (UXML/USS) through `UitkForKsp2`, which adds an MVVM layer:
- `UitkForKsp2.API.Dialog` / `DialogHandle` / `PanelFactory` — windows & panels.
- `UitkForKsp2.MVVM.Core.ViewModelBase`, `RelayCommand`, `CommandBinding`, `EventBinding`,
  and converter groups for data binding.
Stock UI lives in `KSP.UI` (202 types) and `KSP.UI.Binding`; map UI in `KSP.Map`. For an
orbital-survey map overlay, expect to combine a UitkForKsp2 window with `MapProvider`/`KSP.Map`
and possibly a render texture overlay (see `Redux.WorldVis.PqsOverlay` for PQS terrain overlay
precedent).

---

## 9. Harmony patching

KSP2 bundles Harmony as `0Harmony.dll`. Standard Harmony patterns apply:
`[HarmonyPatch(typeof(SomeType), nameof(SomeType.Method))]` with `Prefix`/`Postfix`/
`Transpiler`. Apply all patches in a mod via `CreateHarmonyAndPatchAll()` from `GeneralMod`.

---

## Pattern cheat-sheet

| Need | Use |
|------|-----|
| Reach a game subsystem | `GameManager.Instance.Game.<Provider/Manager>` |
| React to game events | `game.Messages.Subscribe<XMessage>(cb)` (`PersistentSubscribe` for global) |
| Add part behaviour | `Module_X` + `PartComponentModule_X` + `Data_X` trio |
| Per-tick sim logic | override `OnUpdate` + `Part.RegisterModuleForUpdates(this)` |
| Read orbit/position | `VesselComponent.Orbit` / `mainBody` / `Latitude`/`Longitude`/`Altitude*` |
| Consume/produce resources | `ResourceFlowRequestBroker` + `ResourceContainerGroup` |
| Modify game behaviour | Harmony patch via `CreateHarmonyAndPatchAll()` |
| Build a window | `UitkForKsp2` Dialog/Panel + `ViewModelBase` |
| Inspect any game type | `ilspycmd -t <FullName> Packages/KSP2_x64/Assembly-CSharp.dll` |
