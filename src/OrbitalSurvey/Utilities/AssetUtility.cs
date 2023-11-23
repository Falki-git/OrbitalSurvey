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
    
    public readonly Dictionary<string, string> BiomeBundleAssetAddresses = new()
    {
        { "Moho_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/moho_biome_1024.png"},
        { "Moho_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/moho_biome_2048.png"},
        { "Moho_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/moho_biome_4096.png"},
        { "Eve_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/eve_biome_1024.png"},
        { "Eve_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/eve_biome_2048.png"},
        { "Eve_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/eve_biome_4096.png"},
        { "Gilly_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/gilly_biome_1024.png"},
        { "Gilly_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/gilly_biome_2048.png"},
        { "Gilly_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/gilly_biome_4096.png"},
        { "Kerbin_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/kerbin_biome_1024.png"},
        { "Kerbin_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/kerbin_biome_2048.png"},
        { "Kerbin_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/kerbin_biome_4096.png"},
        { "Mun_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/mun_biome_1024.png"},
        { "Mun_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/mun_biome_2048.png"},
        { "Mun_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/mun_biome_4096.png"},
        { "Minmus_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/minmus_biome_1024.png"},
        { "Minmus_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/minmus_biome_2048.png"},
        { "Minmus_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/minmus_biome_4096.png"},
        { "Duna_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/duna_biome_1024.png"},
        { "Duna_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/duna_biome_2048.png"},
        { "Duna_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/duna_biome_4096.png"},
        { "Ike_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/ike_biome_1024.png"},
        { "Ike_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/ike_biome_2048.png"},
        { "Ike_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/ike_biome_4096.png"},
        { "Dres_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/dres_biome_1024.png"},
        { "Dres_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/dres_biome_2048.png"},
        { "Dres_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/dres_biome_4096.png"},
        { "Jool_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/jool_biome_1024.png"},
        { "Jool_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/jool_biome_2048.png"},
        { "Jool_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/jool_biome_4096.png"},
        { "Laythe_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/laythe_biome_1024.png"},
        { "Laythe_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/laythe_biome_2048.png"},
        { "Laythe_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/laythe_biome_4096.png"},
        { "Vall_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/vall_biome_1024.png"},
        { "Vall_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/vall_biome_2048.png"},
        { "Vall_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/vall_biome_4096.png"},
        { "Tylo_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/tylo_biome_1024.png"},
        { "Tylo_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/tylo_biome_2048.png"},
        { "Tylo_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/tylo_biome_4096.png"},
        { "Bop_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/bop_biome_1024.png"},
        { "Bop_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/bop_biome_2048.png"},
        { "Bop_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/bop_biome_4096.png"},
        { "Pol_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/pol_biome_1024.png"},
        { "Pol_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/pol_biome_2048.png"},
        { "Pol_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/pol_biome_4096.png"},
        { "Eeloo_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/eeloo_biome_1024.png"},
        { "Eeloo_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/eeloo_biome_2048.png"},
        { "Eeloo_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/eeloo_biome_4096.png"}
    };
    
    public static readonly Dictionary<string, string> OtherAssetsAddresses = new()
    {
        { "HiddenMap_1024", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_other/orbitalsurvey/other/hiddenmap_1024.png"},
        { "HiddenMap_2048", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_other/orbitalsurvey/other/hiddenmap_2048.png"},
        { "HiddenMap_4096", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_other/orbitalsurvey/other/hiddenmap_4096.png"}
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
        
        Core.Instance.InitializeCelestialData(this);
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