# CLAUDE.md — OrbitalSurveyRedux

Guidance for Claude Code when working in this repository.

## What this project is

This is a **Kerbal Space Program 2 (KSP2) mod project** built on the **KSP2 Redux
mod template** (a ThunderKit + SpaceWarp2 based Unity project). It is intended to host
the **"Orbital Survey"** mod — a mod that lets vessels in orbit scan celestial bodies
for resources/visuals and build up a discoverable map overlay.

> **Current state: the mod files have NOT been migrated yet.** The repo currently
> contains only the template scaffolding (`MyTest11` is the sample mod). The job of
> early iterations is to migrate/author the Orbital Survey mod into this template.

- **Engine:** Unity `6000.4.1f1` (Unity 6.4)
- **Modding stack:** ReduxLib (loader) → SpaceWarp2 (mod API) → your mod
- **Build/packaging:** ThunderKit (Unity editor pipelines) — NOT a plain `dotnet build`
- **Not a git repo yet** (`git init` if version control is wanted — offer first).

## Repository layout

| Path | What it is |
|------|------------|
| `Assets/MyTest11/` | The **sample mod source** (template example). `Code/MyTest11Plugin.cs` is the entry point; `Definitions/` holds Parts/Experiments/Missions; `Pipelines/` holds ThunderKit build pipelines; `swinfo.asset` is mod metadata. |
| `Assets/Mods/__Testing/MyTest11/` | **Built output** of the sample mod (compiled `MyTest11.dll` + `swinfo.json`), staged for the game. |
| `Packages/KSP2_x64/` | **The game's assemblies** (reference-only). `Assembly-CSharp.dll` is ~13 MB and contains almost the entire KSP2 codebase. See `.claude/ksp2_assembly_csharp_reference.md`. *(git-ignored; binaries — do not `Read` directly, decompile instead.)* |
| `Packages/manifest.json` | Unity package dependencies (ThunderKit, BundleKit, KSP2 Redux SDK, UitkForKsp2, etc.). |
| `Redux/` | Game runtime config + Addressable asset bundles (git-ignored, large/binary — ignore). |
| `Utilities/NStrip/` | **NStrip.exe** — the assembly publicizer. |
| `publicize.bat` | Runs NStrip on `Assembly-CSharp.dll` to make all members public (see below). |
| `ThunderKit/` | ThunderKit staging/libraries (git-ignored build artifacts). |
| `*.csproj`, `*.sln` | **Unity-generated** project files (git-ignored). Do not hand-edit; Unity regenerates them. `MyTest11.csproj` is the sample mod's generated project. |
| `.claude/*.md` | Claude-authored reference docs: `ksp2_assembly_csharp_reference.md`, `architectural_patterns_ksp2.md`, `architectural_patterns_orbital-survey.md` — read these before doing mod work. |

## The modding stack (load order)

```
BepInEx-style native loader
  └─ ReduxLib.dll            ← low-level loader, logging, message bus, config
       └─ SpaceWarp2.dll     ← mod API: GeneralMod/ISpaceWarpMod, assets, localization
            └─ SpaceWarp2.Game.dll / .UI.dll / .Sound.dll / .VersionChecking.dll
                 └─ YourMod   ← extends GeneralMod (or KerbalMod for MonoBehaviour access)
```

A mod is a class extending `SpaceWarp2.API.Mods.GeneralMod` with three lifecycle hooks:

```csharp
public class MyMod : GeneralMod {
    public override void OnPreInitialized()  { /* before assets/addressables load — register loaders */ }
    public override void OnInitialized()     { /* assets loaded; safe to touch game code */ }
    public override void OnPostInitialized() { /* all mods initialized */ }
}
```
`GeneralMod` provides `SWLogger` (ILogger), `SWConfiguration` (IConfigFile), `SWMetadata`
(guid/version), and `CreateHarmonyAndPatchAll()` for Harmony patching.

Each mod ships a **`swinfo.json`** manifest: `mod_id`, `version`, `main_assembly`,
`dependencies` (e.g. `SpaceWarp2 >= 2.0.0`), `ksp2_version` range.

## Working with the KSP2 codebase (CRITICAL)

`Packages/KSP2_x64/Assembly-CSharp.dll` is the game. You cannot read it as text.

**To inspect game code, decompile with `ilspycmd`** (installed globally via `dotnet tool`):

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
# List every type (7,194 of them):
ilspycmd -l c "Packages/KSP2_x64/Assembly-CSharp.dll"
# Decompile a single type (preferred — fast, targeted):
ilspycmd -t "KSP.Sim.impl.VesselComponent" "Packages/KSP2_x64/Assembly-CSharp.dll"
```
Always prefer `-t <FullTypeName>` over decompiling the whole assembly. To find a type,
list with `-l c` and `grep`. Other game assemblies (`SpaceWarp2.dll`, `ReduxLib.dll`,
`UitkForKsp2.dll`, `SpaceWarp2.Game.dll`) decompile the same way.

> ilspycmd prints a "not using the latest version" notice to stderr — ignore it. The
> installed version (9.1.0) decompiles this assembly correctly.

### Publicizing (required to actually compile against private game members)
The shipped `Assembly-CSharp.dll` still has its original access modifiers (private/
internal). Mods that touch internals reference a **publicized** copy. Run:
```bat
publicize.bat
```
which invokes `NStrip.exe -p -n -d ... Assembly-CSharp.dll` and replaces the DLL with an
all-public version. This is a one-time setup step on the reference assembly; the game
binary itself is unaffected. **The DLL in this repo is currently NOT publicized.**

## Key game APIs for an Orbital Survey mod

(Full detail in `.claude/ksp2_assembly_csharp_reference.md` and `.claude/architectural_patterns_ksp2.md`.)

- **`KSP.Game.GameInstance`** — the central service locator. Reach it via
  `GameManager.Instance.Game`. Exposes `Messages` (MessageCenter), `Map` (MapProvider),
  `CelestialBodies`, `ResourceDefinitionDatabase`, `Notifications`, `SessionManager`,
  `UniverseModel`, `SpaceSimulation`, etc.
- **`KSP.Messages.MessageCenter`** — pub/sub event bus. `Subscribe<T>`, `PersistentSubscribe<T>`,
  `Publish<T>`, `Unsubscribe`. Relevant messages: `GameStateChangedMessage`,
  `VesselChangedMessage`, `MapViewEnteredMessage`/`MapViewLeftMessage`,
  `ObserverSOIChangedMessage`, `FlightViewEnteredMessage`.
- **`KSP.Sim.impl.VesselComponent`** — the active vessel. Has `Orbit` (PatchedConicsOrbit),
  `mainBody` (CelestialBodyComponent), `Latitude`/`Longitude`,
  `AltitudeFromRadius`/`AltitudeFromTerrain`/`AltitudeFromSeaLevel`, `Situation`
  (VesselSituations enum). These drive "am I in a valid scanning orbit?" logic.
- **Part modules use a dual class pattern** (see architecture doc):
  `Module_X : PartBehaviourModule` (Unity/view side) +
  `PartComponentModule_X : PartComponentModule` (simulation side) +
  `Data_X : ModuleData` (serialized config). **The Redux fork already ships
  `Redux.Modules.PartComponentModule_ResourceScanner` (+ `Module_ResourceScanner`,
  `Data_ResourceScanner`)** — study these first; the Orbital Survey scanner likely
  builds on or parallels them.
- **UI:** `UitkForKsp2` (UI Toolkit / UXML + MVVM) is the modern UI path —
  `UitkForKsp2.API.Dialog`, `PanelFactory`, `ViewModelBase`, binding/converters.

## Conventions & gotchas

- **Don't edit generated files** (`*.csproj`, `*.sln`, `Library/`, `Temp/`, `obj/`,
  `Redux/`, `ThunderKit/Staging|Libraries`). They're git-ignored and regenerated by Unity.
- **`Library/`, `Temp/`, `pm_cache/`, `Redux/Addressables/`** are large/binary noise —
  exclude them from searches. Real source lives under `Assets/`.
- KSP2 wraps Harmony as `0Harmony.dll`. Patch via `CreateHarmonyAndPatchAll()`.
- Building is done through **ThunderKit pipelines in the Unity Editor** (`Build for Editor`,
  `Build for Player`, `Deploy to Zip File` under `Assets/MyTest11/Pipelines/`), not from a
  terminal. Claude generally cannot drive the Unity editor; ask the user to run pipelines.
- Logging in mod code: use `SWLogger` (from `GeneralMod`), not `UnityEngine.Debug` for
  user-facing logs (though decompiled game code uses `Debug.Log*` heavily).

## Suggested next docs / TODOs (not yet created)

- `docs/orbital_survey_design.md` — once migration starts, capture the mod's intended
  scanner mechanics, map-overlay rendering, and save data model.
- `docs/build_and_deploy.md` — exact ThunderKit pipeline steps once verified with the user.
