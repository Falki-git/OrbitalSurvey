using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using KSP.Game;
using KSP.UI.Binding;
using OrbitalSurvey.Debug;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;
using OrbitalSurvey.Managers;
using OrbitalSurvey.UI;
using OrbitalSurvey.Utilities;
using Patches = OrbitalSurvey.Utilities.Patches;

namespace OrbitalSurvey;

[BepInPlugin(ModGuid, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class OrbitalSurveyPlugin : BaseSpaceWarpPlugin
{
    [PublicAPI] public const string ModGuid = "falki.orbital_survey";
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    // AppBar button IDs
    public const string ToolbarFlightButtonID = "BTN-OrbitalOverlayFlight";
    public const string ToolbarOABButtonID = "BTN-OrbitalOverlayOAB";
    public const string ToolbarKSCButtonID = "BTN-OrbitalOverlayKSC";

    // Singleton instance of the plugin class
    public static OrbitalSurveyPlugin Instance { get; set; }

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        Instance = this;
        
        // Register Flight AppBar button
        Appbar.RegisterAppButton(
            ModName,
            ToolbarFlightButtonID,
            AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
            SceneController.Instance.ToggleUI
        );
        
        // Register OAB AppBar button
        Appbar.RegisterOABAppButton(
            ModName,
            ToolbarOABButtonID,
            AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
            SceneController.Instance.ToggleUI
        );
        
        // Register KSC AppBar button
        Appbar.RegisterKSCAppButton(
            ModName,
            ToolbarKSCButtonID,
            AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
            SceneController.Instance.ToggleUI
        );
        
        Settings.Initialize();

        MessageListener.Instance.SubscribeToMessages();
        
        DebugUI.Instance.InitializeStyles();

        // create providers
        var providers = new GameObject("OrbitalSurvey_Providers");
        providers.transform.parent = this.transform;
        providers.AddComponent<AssetUtility>();
        providers.AddComponent<VesselManager>();
        
        // register for save/load events 
        SaveManager.Instance.Register();
        
        // register for EC background processing
        SpaceWarp.API.Parts.PartComponentModuleOverride
            .RegisterModuleForBackgroundResourceProcessing<OrbitalSurvey.Modules.PartComponentModule_OrbitalSurvey>();
        
        Harmony.CreateAndPatchAll(typeof(Patches));
        Harmony.CreateAndPatchAll(typeof(DebugPatches));
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.O))
            DebugUI.Instance.IsDebugWindowOpen = !DebugUI.Instance.IsDebugWindowOpen;
    }
    
    private void OnGUI() => DebugUI.Instance.OnGUI();
}
