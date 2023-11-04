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

namespace OrbitalSurvey;

[BepInPlugin("falki.orbital_survey", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class OrbitalSurveyPlugin : BaseSpaceWarpPlugin
{
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    // AppBar button IDs
    private const string ToolbarFlightButtonID = "BTN-OrbitalOverlayFlight";

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
            isOpen =>
            {
                DEBUG_UI.Instance.IsDebugWindowOpen = isOpen;
                GameObject.Find(ToolbarFlightButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(isOpen);
            }
        );

        MessageListener.Instance.SubscribeToMessages();
        DEBUG_UI.Instance.InitializeStyles();

        // Register all Harmony patches in the project
        Harmony.CreateAndPatchAll(typeof(SaveLoadPatches));
        Harmony.CreateAndPatchAll(typeof(PatchTest));

        // Fetch a configuration value or create a default one if it does not exist
        const string defaultValue = "my default value";
        var configValue = Config.Bind<string>("Settings section", "Option 1", defaultValue, "Option description");

        // Log the config value into <KSP2 Root>/BepInEx/LogOutput.log
        Logger.LogInfo($"Option 1: {configValue.Value}");

        
        
        
        
        _mySaveData = new MyTestSaveData { TestBool = true, TestString = "test string", TestInt = 1 };
        ModSaves.RegisterSaveLoadGameData("falki.orbital_survey", ref _mySaveData, (loadedData) =>
        {
            int i = loadedData.TestInt;
            string s = loadedData.TestString;
            bool b = loadedData.TestBool;
        });

        //OrbitalSurvey.OrbitalSurveyPlugin.Instance._mySaveData.TestInt

    }

    private MyTestSaveData _mySaveData;

    private void OnGUI() => DEBUG_UI.Instance.OnGUI();

    private void Update()
    {
        int i = 0;
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
