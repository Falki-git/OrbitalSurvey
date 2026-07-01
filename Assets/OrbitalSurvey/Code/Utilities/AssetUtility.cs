using System.Collections;
using System.Collections.Generic;
using System.IO;
using KSP.Game;
using OrbitalSurvey.Debug;
using OrbitalSurvey.Models;
using UnityEngine;
using ILogger = ReduxLib.Logging.ILogger;

namespace OrbitalSurvey.Utilities
{
    public class AssetUtility : MonoBehaviour
    {
        public static AssetUtility Instance { get; set; }
        
        public AssetBundle Maps1024;
        public AssetBundle Maps2048;
        public AssetBundle Ui;
        public AssetBundle SwConsole;
        
        public string AssetsPath;
        public string BundlesPath;
        public string ImagesPath;
        
        private static readonly ILogger Logger = ReduxLib.ReduxLib.GetLogger($"OrbitalSurvey|{typeof(AssetUtility).Name}");

        private void Start()
        {
            Instance = this;
            
            Logger.LogInfo("Initializing Assets");
            
            AssetsPath = Path.Combine(OrbitalSurveyPlugin.Instance.SWMetadata.Folder.FullName, "assets");
            BundlesPath = Path.Combine(AssetsPath, "bundles");
            ImagesPath = Path.Combine(AssetsPath, "images");
            
            Maps1024 = AssetBundle.LoadFromFile(Path.Combine(BundlesPath, "orbitalsurvey_maps_1024.bundle"));
            Maps2048 = AssetBundle.LoadFromFile(Path.Combine(BundlesPath, "orbitalsurvey_maps_2048.bundle"));
            Ui = AssetBundle.LoadFromFile(Path.Combine(BundlesPath, "orbitalsurvey_ui.bundle"));
            SwConsole = AssetBundle.LoadFromFile(Path.Combine(BundlesPath, "swconsoleui.bundle"));
            
            foreach (var item in Maps1024.GetAllAssetNames())
            {
                Logger.LogInfo($"AssetBundle Maps1024 Item: {item}");
            }

            foreach (var item in Maps2048.GetAllAssetNames())
            {
                Logger.LogInfo($"AssetBundle Maps2048 Item: {item}");
            }
            
            foreach (var item in Ui.GetAllAssetNames())
            {
                Logger.LogInfo($"AssetBundle Ui Item: {item}");
            }
            
            foreach (var item in SwConsole.GetAllAssetNames())
            {
                Logger.LogInfo($"AssetBundle SwConsole Item: {item}");
            }
            
            DebugUI.Instance.InitializeStyles();
        }

        private readonly Dictionary<string, string> _scaledVisualAddressableAddresses = new()
        {
            { "Moho", "Assets/Environments/systems/kerbol/moho/scaledspace/moho_scaled_d.png" },
            { "Eve", "Assets/Environments/systems/kerbol/eve/scaledspace/eve_scaled_mesh_d.png" },
            { "Gilly", "Assets/Environments/systems/kerbol/gilly/scaledspace/gilly_scaled_d.png" },
            { "Kerbin", "Assets/Environments/systems/kerbol/kerbin/scaledspace/kerbin_scaled_d.png" },
            { "Mun", "Assets/Environments/systems/kerbol/mun/scaledspace/mun_scaled_d.png" },
            { "Minmus", "Assets/Environments/systems/kerbol/minmus/scaledspace/minmus_scaled_d.png" },
            { "Duna", "Assets/Environments/systems/kerbol/duna/scaledspace/duna_scaled_d.png" },
            { "Ike", "Assets/Environments/systems/kerbol/ike/scaledspace/ike_scaled_d.png" },
            { "Dres", "Assets/Environments/systems/kerbol/dres/scaledspace/dres_scaled_d.png" },
            { "Jool", "Assets/Environments/systems/kerbol/jool/scaledspace/jool_scaled_d.png" },
            { "Laythe", "Assets/Environments/systems/kerbol/laythe/scaledspace/laythe_scaled_water_d.png" },
            { "Vall", "Assets/Environments/systems/kerbol/vall/scaledspace/vall_scaled_d.png" },
            { "Tylo", "tylo_scaled_d.png" },
            { "Bop", "Assets/Environments/systems/kerbol/bop/scaledspace/bop_scaled_d.png" },
            { "Pol", "Assets/Environments/systems/kerbol/pol/scaledspace/pol_scaled_d.png" },
            { "Eeloo", "Assets/Environments/systems/kerbol/eeloo/scaledspace/eeloo_scaled_d.png" }
        };

        // TODO probably fix asset bundle addresses
        public readonly Dictionary<string, string> VisualBundleAssetAddresses = new()
        {
            {
                "Moho_1024",
                // $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/images/visualmaps/moho_scaled_d_1024.png"
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/moho_scaled_d_1024.png"
            },
            {
                "Eve_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/eve_scaled_mesh_d_1024.png"
            },
            {
                "Gilly_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/gilly_scaled_d_1024.png"
            },
            {
                "Kerbin_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/kerbin_scaled_d_1024.png"
            },
            {
                "Mun_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/mun_scaled_d_1024.png"
            },
            {
                "Minmus_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/minmus_scaled_d_1024.png"
            },
            {
                "Duna_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/duna_scaled_d_1024.png"
            },
            {
                "Ike_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/ike_scaled_d_1024.png"
            },
            {
                "Dres_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/dres_scaled_d_1024.png"
            },
            {
                "Jool_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/jool_scaled_d_1024.png"
            },
            {
                "Laythe_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/laythe_scaled_water_d_1024.png"
            },
            {
                "Vall_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/vall_scaled_d_1024.png"
            },
            {
                "Tylo_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/tylo_scaled_d_1024.png"
            },
            {
                "Bop_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/bop_scaled_d_1024.png"
            },
            {
                "Pol_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/pol_scaled_d_1024.png"
            },
            {
                "Eeloo_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/eeloo_scaled_d_1024.png"
            },

            {
                "Moho_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/moho_scaled_d_2048.png"
            },
            {
                "Eve_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/eve_scaled_mesh_d_2048.png"
            },
            {
                "Gilly_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/gilly_scaled_d_2048.png"
            },
            {
                "Kerbin_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/kerbin_scaled_d_2048.png"
            },
            {
                "Mun_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/mun_scaled_d_2048.png"
            },
            {
                "Minmus_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/minmus_scaled_d_2048.png"
            },
            {
                "Duna_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/duna_scaled_d_2048.png"
            },
            {
                "Ike_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/ike_scaled_d_2048.png"
            },
            {
                "Dres_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/dres_scaled_d_2048.png"
            },
            {
                "Jool_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/jool_scaled_d_2048.png"
            },
            {
                "Laythe_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/laythe_scaled_water_d_2048.png"
            },
            {
                "Vall_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/vall_scaled_d_2048.png"
            },
            {
                "Tylo_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/tylo_scaled_d_2048.png"
            },
            {
                "Bop_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/bop_scaled_d_2048.png"
            },
            {
                "Pol_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/pol_scaled_d_2048.png"
            },
            {
                "Eeloo_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/visualmaps/eeloo_scaled_d_2048.png"
            }
        };

        public readonly Dictionary<string, string> BiomeBundleAssetAddresses = new()
        {
            {
                "Moho_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/moho_region_1024.png"
            },
            {
                "Eve_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/eve_region_1024.png"
            },
            {
                "Gilly_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/gilly_region_1024.png"
            },
            {
                "Kerbin_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/kerbin_region_1024.png"
            },
            {
                "Mun_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/mun_region_1024.png"
            },
            {
                "Minmus_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/minmus_region_1024.png"
            },
            {
                "Duna_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/duna_region_1024.png"
            },
            {
                "Ike_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/ike_region_1024.png"
            },
            {
                "Dres_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/dres_region_1024.png"
            },
            {
                "Jool_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/jool_region_1024.png"
            },
            {
                "Laythe_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/laythe_region_1024.png"
            },
            {
                "Vall_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/vall_region_1024.png"
            },
            {
                "Tylo_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/tylo_region_1024.png"
            },
            {
                "Bop_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/bop_region_1024.png"
            },
            {
                "Pol_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/pol_region_1024.png"
            },
            {
                "Eeloo_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/eeloo_region_1024.png"
            },

            {
                "Moho_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/moho_region_2048.png"
            },
            {
                "Eve_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/eve_region_2048.png"
            },
            {
                "Gilly_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/gilly_region_2048.png"
            },
            {
                "Kerbin_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/kerbin_region_2048.png"
            },
            {
                "Mun_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/mun_region_2048.png"
            },
            {
                "Minmus_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/minmus_region_2048.png"
            },
            {
                "Duna_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/duna_region_2048.png"
            },
            {
                "Ike_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/ike_region_2048.png"
            },
            {
                "Dres_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/dres_region_2048.png"
            },
            {
                "Jool_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/jool_region_2048.png"
            },
            {
                "Laythe_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/laythe_region_2048.png"
            },
            {
                "Vall_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/vall_region_2048.png"
            },
            {
                "Tylo_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/tylo_region_2048.png"
            },
            {
                "Bop_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/bop_region_2048.png"
            },
            {
                "Pol_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/pol_region_2048.png"
            },
            {
                "Eeloo_2048",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/regionmaps/eeloo_region_2048.png"
            },
        };

        public readonly Dictionary<string, string> OtherAssetsAddresses = new()
        {
            {
                "HiddenMap_1024",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/other/hiddenmap_1024.png"
            },
            {
                "AllBlack_4096", $"assets/{OrbitalSurveyPlugin.ModGuid}/images/other/allblack_4096.png"
            },
            {
                "StaticBackground",
                $"assets/{OrbitalSurveyPlugin.ModGuid}/images/other/static_background.jpeg"
            }
        };

        /* Not used any more, I think
        public readonly Dictionary<string, Texture2D> ScaledVisualTextures = new();

        public void InitializeVisualTextures()
        {
            //StartCoroutine(LoadVisualTextures());
        }

        private IEnumerator LoadVisualTextures()
        {
            var assetCallbacks = 0;
            ScaledVisualTextures.Clear();

            Logger.LogInfo(
                $"Start loading visual map textures. Textures to load: {_scaledVisualAddressableAddresses.Count}.");

            foreach (var body in _scaledVisualAddressableAddresses)
            {
                GameManager.Instance.Assets.Load<Texture2D>(
                    body.Value,
                    tex =>
                    {
                        assetCallbacks++;
                        if (tex == null)
                        {
                            Logger.LogError($"Error loading visual map asset for body {body.Key}. " +
                                            $"No asset with address {body.Value}.");
                            return;
                        }

                        var readableTexture = ScanUtility.ConvertToReadableTexture(tex);
                        ScaledVisualTextures.Add(body.Key, readableTexture);
                        Logger.LogInfo($"Loaded visual map for {body.Key} ({assetCallbacks}).");
                    }
                );
            }

            // Wait until all maps are loaded
            while (assetCallbacks < _scaledVisualAddressableAddresses.Count)
                yield return null;

            Logger.LogInfo($"Finished loading {ScaledVisualTextures.Count} visual textures.");

            //Core.Instance.InitializeCelestialData(this);
        }
        */

        public Texture2D GenerateHiddenMap()
        {
            var source = Maps1024.LoadAsset<Texture2D>
                         (OtherAssetsAddresses[$"HiddenMap_{Settings.ActiveResolution}"]);
            
            var target = new Texture2D(source.width, source.height, source.format, source.mipmapCount > 1);
            Graphics.CopyTexture(source, target);
            target.Apply();
            return target;
        }

        public Texture2D GetTextureAsset(AssetType type, string key)
        {
            // var result = path.StartsWith("/microengineer_flightui")
            //     ? MicroEngineerPlugin.Instance.FlightUi.LoadAsset<VisualTreeAsset>(path.Replace("/microengineer_flightui","assets"))
            //     : MicroEngineerPlugin.Instance.OabUi.LoadAsset<VisualTreeAsset>(path.Replace("/microengineer_oabui","assets"));
            
            switch (type)
            {
                case AssetType.Visual:
                    if (key.Contains("1024")) return Maps1024.LoadAsset<Texture2D>(VisualBundleAssetAddresses[key]);
                    if (key.Contains("2048")) return Maps2048.LoadAsset<Texture2D>(VisualBundleAssetAddresses[key]); 
                    
                    Logger.LogError($"Unknown asset with key {key}!"); 
                    return new Texture2D(2, 2);
                
                case AssetType.Biome:
                    if (key.Contains("1024")) return Maps1024.LoadAsset<Texture2D>(BiomeBundleAssetAddresses[key]);
                    if (key.Contains("2048")) return Maps2048.LoadAsset<Texture2D>(BiomeBundleAssetAddresses[key]);
                    
                    Logger.LogError($"Unknown asset with key {key}!"); 
                    return new Texture2D(2, 2);
                
                case AssetType.Other:
                    if (key.Contains("static")) return Ui.LoadAsset<Texture2D>(OtherAssetsAddresses[key]);
                    
                    // This is bad... need to fix this
                    return Maps1024.LoadAsset<Texture2D>(OtherAssetsAddresses[key]);
                default:
                    Logger.LogError($"Unknown AssetType: {type}!");
                    return new Texture2D(2, 2);
            }
        }

        public GUISkin GetGUISkin()
        {
            return SwConsole.LoadAsset<GUISkin>($"assets/swconsoleui/spacewarpconsole.guiskin");
        }
    }
}