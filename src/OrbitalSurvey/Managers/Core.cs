using BepInEx.Logging;
using KSP.Game;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;
using SpaceWarp.API.Assets;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace OrbitalSurvey.Managers;

public class Core : MonoBehaviour
{
    public bool MapsInitialized { get; set; }

    private Core()
    {
        CelestialDataDictionary = new();
    }
    
    public static Core Instance { get; } = new();
    public CelestialDataDictionary CelestialDataDictionary { get; set; }
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.Core");
    
    public void InitializeCelestialData(AssetUtility assetUtility)
    {
        var celestialBodies = GameManager.Instance.Game?.UniverseModel?.GetAllCelestialBodies();
        if (celestialBodies == null)
        {
            _LOGGER.LogError("Error fetching celestial bodies from UniverseModel. Celestial bodies will not initialize!");
            return;
        }
        
        CelestialDataDictionary.Clear();

        foreach (var body in celestialBodies.Where(celes => !celes.IsStar))
        {
            var key = body.Name;
            var celesData = new CelestialData();
            celesData.Body = body;

            // Visual map
            try
            {
                celesData.Maps[MapType.Visual].ScannedMap = assetUtility.ScaledVisualTextures[key];
            }
            catch (Exception ex)
            {
                _LOGGER.LogError($"Error loading visual map for {key}.\n" + ex);
            }
            
            // Biome map
            try
            {
                celesData.Maps[MapType.Biome].ScannedMap =
                    AssetManager.GetAsset<Texture2D>(assetUtility.BiomeBundleAssetAddresses[key]);
            }
            catch (Exception ex)
            {
                _LOGGER.LogError($"Error loading biome map for {key}.\n" + ex);
            }
            
            CelestialDataDictionary.Add(key, celesData);
            _LOGGER.LogInfo($"Initialized CelestialDataDictionary for {key}.");
        }
        
        MapsInitialized = true;
        _LOGGER.LogInfo($"Finished CelestialDataDictionary initialization with {CelestialDataDictionary.Count} entries.");
    }
}

