using System.Collections;
using BepInEx.Logging;
using KSP.Game;
using SpaceWarp.API.Assets;
using UnityEngine;

namespace OrbitalSurvey.Utilities;

public class AssetUtility : MonoBehaviour
{
    public static AssetUtility Instance { get; set; }

    private void Start() => Instance = this;
    
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
        { "Moho_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/moho_scaled_d_1024.png"},
        { "Eve_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/eve_scaled_mesh_d_1024.png"},
        { "Gilly_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/gilly_scaled_d_1024.png"},
        { "Kerbin_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/kerbin_scaled_d_1024.png"},
        { "Mun_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/mun_scaled_d_1024.png"},
        { "Minmus_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/minmus_scaled_d_1024.png"},
        { "Duna_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/duna_scaled_d_1024.png"},
        { "Ike_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/ike_scaled_d_1024.png"},
        { "Dres_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/dres_scaled_d_1024.png"},
        { "Jool_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/jool_scaled_d_1024.png"},
        { "Laythe_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/laythe_scaled_water_d_1024.png"},
        { "Vall_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/vall_scaled_d_1024.png"},
        { "Tylo_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/tylo_scaled_d_1024.png"},
        { "Bop_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/bop_scaled_d_1024.png"},
        { "Pol_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/pol_scaled_d_1024.png"},
        { "Eeloo_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/visualmaps/eeloo_scaled_d_1024.png"},
        
        { "Moho_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/moho_scaled_d_2048.png"},
        { "Eve_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/eve_scaled_mesh_d_2048.png"},
        { "Gilly_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/gilly_scaled_d_2048.png"},
        { "Kerbin_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/kerbin_scaled_d_2048.png"},
        { "Mun_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/mun_scaled_d_2048.png"},
        { "Minmus_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/minmus_scaled_d_2048.png"},
        { "Duna_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/duna_scaled_d_2048.png"},
        { "Ike_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/ike_scaled_d_2048.png"},
        { "Dres_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/dres_scaled_d_2048.png"},
        { "Jool_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/jool_scaled_d_2048.png"},
        { "Laythe_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/laythe_scaled_water_d_2048.png"},
        { "Vall_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/vall_scaled_d_2048.png"},
        { "Tylo_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/tylo_scaled_d_2048.png"},
        { "Bop_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/bop_scaled_d_2048.png"},
        { "Pol_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/pol_scaled_d_2048.png"},
        { "Eeloo_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/visualmaps/eeloo_scaled_d_2048.png"}
    };
    
    public readonly Dictionary<string, string> BiomeBundleAssetAddresses = new()
    {
        { "Moho_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/moho_region_1024.png"},
        { "Eve_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/eve_region_1024.png"},
        { "Gilly_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/gilly_region_1024.png"},
        { "Kerbin_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/kerbin_region_1024.png"},
        { "Mun_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/mun_region_1024.png"},
        { "Minmus_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/minmus_region_1024.png"},
        { "Duna_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/duna_region_1024.png"},
        { "Ike_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/ike_region_1024.png"},
        { "Dres_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/dres_region_1024.png"},
        { "Jool_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/jool_region_1024.png"},
        { "Laythe_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/laythe_region_1024.png"},
        { "Vall_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/vall_region_1024.png"},
        { "Tylo_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/tylo_region_1024.png"},
        { "Bop_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/bop_region_1024.png"},
        { "Pol_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/pol_region_1024.png"},
        { "Eeloo_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/regionmaps/eeloo_region_1024.png"},
        
        { "Moho_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/moho_region_2048.png"},
        { "Eve_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/eve_region_2048.png"},
        { "Gilly_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/gilly_region_2048.png"},
        { "Kerbin_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/kerbin_region_2048.png"},
        { "Mun_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/mun_region_2048.png"},
        { "Minmus_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/minmus_region_2048.png"},
        { "Duna_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/duna_region_2048.png"},
        { "Ike_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/ike_region_2048.png"},
        { "Dres_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/dres_region_2048.png"},
        { "Jool_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/jool_region_2048.png"},
        { "Laythe_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/laythe_region_2048.png"},
        { "Vall_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/vall_region_2048.png"},
        { "Tylo_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/tylo_region_2048.png"},
        { "Bop_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/bop_region_2048.png"},
        { "Pol_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/pol_region_2048.png"},
        { "Eeloo_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_2048/orbitalsurvey/regionmaps/eeloo_region_2048.png"},
    };
    
    public static readonly Dictionary<string, string> OtherAssetsAddresses = new()
    {
        { "HiddenMap_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/other/hiddenmap_1024.png"},
        { "AllBlack_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_maps_1024/orbitalsurvey/other/allblack_4096.png"},
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