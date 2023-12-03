using System.Collections;
using BepInEx.Logging;
using KSP.Game;
using OrbitalSurvey.Managers;
using SpaceWarp.API.Assets;
using UnityEngine;

namespace OrbitalSurvey.Utilities;

public class AssetUtility : MonoBehaviour
{
    private readonly Dictionary<string, string> _scaledVisualAddressableAddresses = new()
    {
        { "Moho", "Assets/Environments/systems/kerbol/moho/scaledspace/moho_scaled_d.png"},
        { "Eve", "Assets/Environments/systems/kerbol/eve/scaledspace/eve_scaled_mesh_d.png"},
        { "Gilly", "Assets/Environments/systems/kerbol/gilly/scaledspace/gilly_scaled_d.png"},
        { "Kerbin", "Assets/Environments/systems/kerbol/kerbin/scaledspace/kerbin_scaled_d.png"},
        { "Mun", "Assets/Environments/systems/kerbol/mun/scaledspace/mun_scaled_d.png"},
        { "Minmus", "Assets/Environments/systems/kerbol/minmus/scaledspace/minmus_scaled_d.png"},
        { "Duna", "Assets/Environments/systems/kerbol/duna/scaledspace/duna_scaled_d.png"},
        { "Ike", "Assets/Environments/systems/kerbol/ike/scaledspace/ike_scaled_d.png"},
        { "Dres", "Assets/Environments/systems/kerbol/dres/scaledspace/dres_scaled_d.png"},
        { "Jool", "Assets/Environments/systems/kerbol/jool/scaledspace/jool_scaled_d.png"},
        { "Laythe", "Assets/Environments/systems/kerbol/laythe/scaledspace/laythe_scaled_water_d.png"},
        { "Vall", "Assets/Environments/systems/kerbol/vall/scaledspace/vall_scaled_d.png"},
        { "Tylo", "tylo_scaled_d.png"},
        { "Bop", "Assets/Environments/systems/kerbol/bop/scaledspace/bop_scaled_d.png"},
        { "Pol", "Assets/Environments/systems/kerbol/pol/scaledspace/pol_scaled_d.png"},
        { "Eeloo", "Assets/Environments/systems/kerbol/eeloo/scaledspace/eeloo_scaled_d.png"}
    };
    
    public readonly Dictionary<string, string> VisualBundleAssetAddresses = new()
    {
        { "Moho_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/moho_scaled_d_1024.png"},
        { "Eve_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/eve_scaled_d_1024.png"},
        { "Gilly_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/gilly_scaled_d_1024.png"},
        { "Kerbin_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/kerbin_scaled_d_1024.png"},
        { "Mun_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/mun_scaled_d_1024.png"},
        { "Minmus_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/minmus_scaled_d_1024.png"},
        { "Duna_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/duna_scaled_d_1024.png"},
        { "Ike_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/ike_scaled_d_1024.png"},
        { "Dres_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/dres_scaled_d_1024.png"},
        { "Jool_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/jool_scaled_d_1024.png"},
        { "Laythe_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/laythe_scaled_water_d_1024.png"},
        { "Vall_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/vall_scaled_d_1024.png"},
        { "Tylo_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/tylo_scaled_d_1024.png"},
        { "Bop_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/bop_scaled_d_1024.png"},
        { "Pol_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/pol_scaled_d_1024.png"},
        { "Eeloo_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/visualmaps/eeloo_scaled_d_1024.png"}
    };
    
    public readonly Dictionary<string, string> BiomeBundleAssetAddresses = new()
    {
        { "Moho_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/moho_biome_1024.png"},
        { "Eve_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/eve_biome_1024.png"},
        { "Gilly_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/gilly_biome_1024.png"},
        { "Kerbin_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/kerbin_biome_1024.png"},
        { "Mun_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/mun_biome_1024.png"},
        { "Minmus_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/minmus_biome_1024.png"},
        { "Duna_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/duna_biome_1024.png"},
        { "Ike_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/ike_biome_1024.png"},
        { "Dres_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/dres_biome_1024.png"},
        { "Jool_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/jool_biome_1024.png"},
        { "Laythe_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/laythe_biome_1024.png"},
        { "Vall_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/vall_biome_1024.png"},
        { "Tylo_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/tylo_biome_1024.png"},
        { "Bop_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/bop_biome_1024.png"},
        { "Pol_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/pol_biome_1024.png"},
        { "Eeloo_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/biomemaps/eeloo_biome_1024.png"}
    };
    
    public static readonly Dictionary<string, string> OtherAssetsAddresses = new()
    {
        { "HiddenMap_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/other/hiddenmap_1024.png"},
        { "AllBlack_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps/orbitalsurvey/other/allblack_4096.png"},
        { "StaticBackground", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_ui/ui/orbital_survey/static_background.jpeg"}
    };

    public readonly Dictionary<string, Texture2D> ScaledVisualTextures = new();
    
    private static readonly ManualLogSource _LOGGER = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.AssetUtility");

    public void InitializeVisualTextures()
    {
        StartCoroutine(LoadVisualTextures());
    }

    private IEnumerator LoadVisualTextures()
    {
        var assetCallbacks = 0;
        ScaledVisualTextures.Clear();
        
        _LOGGER.LogInfo(
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
                            _LOGGER.LogError($"Error loading visual map asset for body {body.Key}. " +
                                             $"No asset with address {body.Value}.");
                            return;
                        }

                        var readableTexture = ScanUtility.ConvertToReadableTexture(tex);
                        ScaledVisualTextures.Add(body.Key, readableTexture);
                        _LOGGER.LogInfo($"Loaded visual map for {body.Key} ({assetCallbacks}).");
                    }
            );
        }
        
        // Wait until all maps are loaded
        while (assetCallbacks < _scaledVisualAddressableAddresses.Count)
            yield return null;
            
        _LOGGER.LogInfo($"Finished loading {ScaledVisualTextures.Count} visual textures.");
        
        //Core.Instance.InitializeCelestialData(this);
    }

    public static Texture2D GenerateHiddenMap()
    {
        var source = AssetManager.GetAsset<Texture2D>(OtherAssetsAddresses[$"HiddenMap_{Settings.ActiveResolution}"]);
        var target = new Texture2D(source.width, source.height, source.format, source.mipmapCount > 1);
        Graphics.CopyTexture(source, target);
        target.Apply();
        return target; 
    }
}