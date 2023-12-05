using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using KSP.UI.Binding;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;
using KSP.Rendering.Planets;
using KSP.Game;
using OrbitalSurvey.Managers;
using OrbitalSurvey.UI;
using OrbitalSurvey.Utilities;

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

    public AssetUtility AssetUtility;

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        Instance = this;

        // Register Flight AppBar button
        Appbar.RegisterAppButton(
            "Orbital Survey DEBUG",
            ToolbarFlightButtonID+"DEBUG",
            AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
            isOpen =>
            {
                DEBUG_UI.Instance.IsDebugWindowOpen = isOpen;
                GameObject.Find(ToolbarFlightButtonID+"DEBUG")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(isOpen);
            }
        );
        
        Appbar.RegisterOABAppButton(
            "Orbital Survey DEBUG",
            ToolbarOABButtonID+"DEBUG",
            AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
            isOpen =>
            {
                DEBUG_UI.Instance.IsDebugWindowOpen = isOpen;
                GameObject.Find(ToolbarOABButtonID+"DEBUG")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(isOpen);
            }
        );
        
        Appbar.RegisterKSCAppButton(
            "Orbital Survey DEBUG",
            ToolbarKSCButtonID+"DEBUG",
            AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
            () =>
            {
                DEBUG_UI.Instance.IsDebugWindowOpen = !DEBUG_UI.Instance.IsDebugWindowOpen;
            }
        );
        
        // Register Flight AppBar button
        Appbar.RegisterAppButton(
            ModName,
            ToolbarFlightButtonID,
            AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
            isOpen =>
            {
                SceneController.Instance.ShowMainGui = !SceneController.Instance.ShowMainGui; 
                GameObject.Find(ToolbarFlightButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(isOpen);
            }
        );
        
        // Register OAB AppBar button
        Appbar.RegisterOABAppButton(
            ModName,
            ToolbarOABButtonID,
            AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
            isOpen =>
            {
                SceneController.Instance.ShowMainGui = !SceneController.Instance.ShowMainGui; 
                GameObject.Find(ToolbarOABButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(isOpen);
            }
        );
        
        // Register KSC AppBar button
        Appbar.RegisterKSCAppButton(
            ModName,
            ToolbarKSCButtonID,
            AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
            () =>
            {
                SceneController.Instance.ShowMainGui = !SceneController.Instance.ShowMainGui; 
            }
        );

        MessageListener.Instance.SubscribeToMessages();
        DEBUG_UI.Instance.InitializeStyles();

        // Register all Harmony patches in the project
        //Harmony.CreateAndPatchAll(typeof(SaveLoadPatches));
        Harmony.CreateAndPatchAll(typeof(PatchTest));

        //MySaveData = new MyTestSaveData { TestBool = true, TestString = "test string", TestInt = 1 };
        
        /*
        MySaveData = SpaceWarp.API.SaveGameManager.ModSaves.RegisterSaveLoadGameData<MyTestSaveData>(
            "falki.orbital_survey",
            (savedData) =>
            {
                // This function will be called when a SAVE event is triggered.
                // If you don't need to do anything on save events, pass null instead of this function.

                bool b = savedData.TestBool;
                int i = savedData.TestInt;
                string s = savedData.TestString;

            },
            (loadedData) =>
            {
                // This function will be called when a LOAD event is triggered and BEFORE data is loaded to your saveData object.
                // You don't need to manually update your data. It will be updated after this function.
                // If you don't need to do anything on load events, pass null instead of this function.

                bool b = loadedData.TestBool;
                int i = loadedData.TestInt;
                string s = loadedData.TestString;
            }//,
            //MySaveData
        );
        */
        
        // var isLoaded = true;
        // var key = "Assets/Environments/systems/kerbol/duna/scaledspace/duna_scaled_d.png";
        // GameManager.Instance.Assets.Load<Texture2D>(key, LoadTexture, isLoaded);

        AssetUtility = gameObject.AddComponent<AssetUtility>();
        
        SaveManager.Instance.Register();
    }

    // private void LoadTexture(Texture2D tex)
    // {
    //     Logger.LogDebug($"LogTexture triggered.");
    //     Logger.LogDebug($"Tex name it {tex.name}");
    // }
    
    private void OnGUI() => DEBUG_UI.Instance.OnGUI();

    private void Update()
    {
        try
        {
            var gamemanager = GameManager.Instance;
        }
        catch (Exception ex)
        {  }
    }

    private void TestBed()
    {
        //Texture2D texture = new Texture2D(100, 100);
        {
            var pqs = UnityEngine.GameObject.Find("#PhysicsSpace/#Celestial/596b3747-f06b-4218-87a5-ad5db6f0a360/Kerbin").GetComponent<KSP.Rendering.Planets.PQS>();

            var tex = new Texture2D(100, 100);
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, Color.red); // Set all pixels to red.
                }
            }
            tex.Apply(); // Apply changes to the texture.

            //pqs._scaledMaterial.mainTexture = tex;
            pqs._scaledMaterial.SetTexture("_MainTex", tex);

        }
        // KSP1 
        {
            //public virtual void ApplyTerrainShaderSettings()

            //if (!this.DEBUG_UseSharedMaterial)
            //Material material = new Material(this.surfaceMaterial);
            //material.CopyKeywordsFrom(this.surfaceMaterial);
            //this.surfaceMaterial = material;

            // copy shader keywords
            {
                //public static void CopyKeywordsFrom(this Material m, Material copyFrom)
                //string[] shaderKeywords = copyFrom.shaderKeywords;
                //string[] array = new string[shaderKeywords.Length];
                //shaderKeywords.CopyTo(array, 0);
                //m.shaderKeywords = array;

            }
        }


        // TRY1
        {
            var pqs = UnityEngine.GameObject.Find("#PhysicsSpace/#Celestial/6369a949-905c-40fd-a7e6-b5e2e3810bcf/Kerbin").GetComponent<KSP.Rendering.Planets.PQS>();

            var tex = new Texture2D(100, 100);
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, Color.red); // Set all pixels to red.
                }
            }
            tex.Apply(); // Apply changes to the texture.

            //pqs._scaledMaterial.mainTexture = tex;

            var scaledRenderer = pqs.ScaledRenderer;

            Material newMaterial = new Material(scaledRenderer.material);
            string[] shaderKeywords = scaledRenderer.material.shaderKeywords;
            string[] array = new string[shaderKeywords.Length];
            shaderKeywords.CopyTo(array, 0);
            newMaterial.shaderKeywords = array;

            newMaterial.SetTexture("_MainTex", tex);

            scaledRenderer.material = newMaterial;
            //scaledRenderer.SetMaterial(newMaterial);
        }

        // TRY2
        {
            var pqs = UnityEngine.GameObject.Find("#PhysicsSpace/#Celestial/6369a949-905c-40fd-a7e6-b5e2e3810bcf/Kerbin").GetComponent<KSP.Rendering.Planets.PQSRenderer>();

            var tex = new Texture2D(100, 100);
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, Color.red); // Set all pixels to red.
                }
            }
            tex.Apply(); // Apply changes to the texture.

            //pqs._scaledMaterial.mainTexture = tex;

            pqs._scaledSpaceMaterialPropertyBlock.SetTexture("_MainTex", tex);

            //Material newMaterial = new Material(scaledRenderer.material);
            //string[] shaderKeywords = scaledRenderer.material.shaderKeywords;
            //string[] array = new string[shaderKeywords.Length];
            //shaderKeywords.CopyTo(array, 0);
            //newMaterial.shaderKeywords = array;

            //newMaterial.SetTexture("_MainTex", tex);

            //scaledRenderer.material = newMaterial;
            //scaledRenderer.SetMaterial(newMaterial);
        }

        // TRY3
        {
            var pqs = UnityEngine.GameObject.Find("#PhysicsSpace/#Celestial/6369a949-905c-40fd-a7e6-b5e2e3810bcf/Kerbin").GetComponent<KSP.Rendering.Planets.PQS>();
            var tex = new Texture2D(100, 100, TextureFormat.ARGB32, true);
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, Color.red); // Set all pixels to red.
                }
            }
            tex.Apply(); // Apply changes to the texture.

            var scaledRenderer = pqs.ScaledRenderer;

            Material newMaterial = new Material(scaledRenderer.material);
            string[] shaderKeywords = scaledRenderer.material.shaderKeywords;
            string[] array = new string[shaderKeywords.Length];
            shaderKeywords.CopyTo(array, 0);
            newMaterial.shaderKeywords = array;            
            //newMaterial.SetTexture("_MainTex", tex);
            newMaterial.mainTexture = tex;



            PQSRenderer pqsRenderer = UnityEngine.GameObject.Find("#PhysicsSpace/#Celestial/6369a949-905c-40fd-a7e6-b5e2e3810bcf/Kerbin").GetComponent<KSP.Rendering.Planets.PQSRenderer>();
            pqsRenderer.AddOverlay(new MyIPQS { OverlayMaterial = newMaterial });
        }

        // DRAW PQS OVERLAY
        {
            var kerbin = UnityEngine.GameObject.Find("#PhysicsSpace/#Celestial/d96888e1-7181-48ed-b1cb-67a572893441/Kerbin");
            var pqsRenderer = kerbin.GetComponent<KSP.Rendering.Planets.PQSRenderer>();
            //pqsRenderer;

            pqsRenderer.DrawPQSOverlays(pqsRenderer.SourceCamera);
        }
    }
        
}
