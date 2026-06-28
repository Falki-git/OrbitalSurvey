using System.IO;
using JetBrains.Annotations;
using OrbitalSurvey.Debug;
using UnityEngine;
using OrbitalSurvey.Managers;
using OrbitalSurvey.UI;
using OrbitalSurvey.Utilities;
using Redux.ExtraModTypes;
using SpaceWarp2.UI.API.Appbar;

namespace OrbitalSurvey
{
    public class OrbitalSurveyPlugin : KerbalMod
    {
        [PublicAPI] public const string ModGuid = "OrbitalSurvey";
        [PublicAPI] public const string ModName = "Orbital Survey";
        [PublicAPI] public const string ModVer = "0.9.5";
        
        [PublicAPI] public static OrbitalSurveyPlugin Instance { get; set; }

        // AppBar button IDs
        public const string ToolbarFlightButtonID = "BTN-OrbitalOverlayFlight";
        public const string ToolbarOABButtonID = "BTN-OrbitalOverlayOAB";
        public const string ToolbarKSCButtonID = "BTN-OrbitalOverlayKSC";
        
        internal Texture2D AppIcon;

        public override void OnPreInitialized()
        {
            Instance = this;
            
            var assetsPath = Path.Combine(SWMetadata.Folder.FullName, "assets");
            var imagesPath = Path.Combine(assetsPath, "images");
            var icon = Path.Combine(imagesPath, "icon.png");
            var bytes = File.ReadAllBytes(icon);
            AppIcon = new Texture2D(2, 2);
            AppIcon.LoadImage(bytes);
            
            Settings.Initialize();
        }

        public override void OnInitialized()
        {
            // Register Flight AppBar button
            Appbar.RegisterAppButton(
                ModName,
                ToolbarFlightButtonID,
                AppIcon,
                SceneController.Instance.ToggleUI
            );

            // Register OAB AppBar button
            Appbar.RegisterOABAppButton(
                ModName,
                ToolbarOABButtonID,
                AppIcon,
                SceneController.Instance.ToggleUI
            );

            // Register KSC AppBar button
            Appbar.RegisterKSCAppButton(
                ModName,
                ToolbarKSCButtonID,
                AppIcon,
                SceneController.Instance.ToggleUI
            );

            MessageListener.Instance.SubscribeToMessages();

            // create providers
            var providers = new GameObject("OrbitalSurvey_Providers");
            providers.transform.parent = this.transform;
            providers.AddComponent<AssetUtility>();
            providers.AddComponent<VesselManager>();

            // Note: CelestialCategoryManager.InitializeConfigs() is called on game load
            // (MessageListener.OnGameLoadFinishedMessage), after PatchManager has finished
            // binding the config values defined by the mod's Lua patches.

            // register for save/load events
            SaveManager.Instance.Register();

            // register for EC background processing
            SpaceWarp2.API.Parts.PartComponentModuleOverride
                .RegisterModuleForBackgroundResourceProcessing<
                    OrbitalSurvey.Modules.PartComponentModule_OrbitalSurvey>();

            CreateHarmonyAndPatchAll();
        }

        private void Update()
        {
                if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.O))
                DebugUI.Instance.IsDebugWindowOpen = !DebugUI.Instance.IsDebugWindowOpen;
        }

        private void OnGUI() => DebugUI.Instance.OnGUI();
    }
}