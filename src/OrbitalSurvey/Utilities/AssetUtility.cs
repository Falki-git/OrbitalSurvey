using System.Collections;
using BepInEx.Logging;
using KSP.Game;
using OrbitalSurvey.Managers;
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
        { "Moho", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/moho_biome.png"},
        { "Eve", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/eve_biome.png"},
        { "Gilly", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/gilly_biome.png"},
        { "Kerbin", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/kerbin_biome.png"},
        { "Mun", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/mun_biome.png"},
        { "Minmus", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/minmus_biome.png"},
        { "Duna", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/duna_biome.png"},
        { "Ike", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/ike_biome.png"},
        { "Dres", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/dres_biome.png"},
        { "Jool", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/jool_biome.png"},
        { "Laythe", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/laythe_biome.png"},
        { "Vall", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/vall_biome.png"},
        { "Tylo", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/tylo_biome.png"},
        { "Bop", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/bop_biome.png"},
        { "Pol", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/pol_biome.png"},
        { "Eeloo", $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_biomes/orbitalsurvey/biomemaps/eeloo_biome.png"}
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
        //Core.Instance.InitializeCelestialData(_scaledVisualTextures);
        Core.Instance.InitializeCelestialData(this);
    }

    public static Texture2D GenerateHiddenMap()
    {
        return new Texture2D(Settings.MAP_RESOLUTION.Item1, Settings.MAP_RESOLUTION.Item2);
    }
}