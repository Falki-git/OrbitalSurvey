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
    private Core()
    {
        CelestialDataDictionary = new();
    }
    
    public static Core Instance { get; } = new();
    public CelestialDataDictionary CelestialDataDictionary { get; set; }
    public bool MapsInitialized { get; set; }
    
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
                _LOGGER.LogInfo($"Visual map for {key} successfully initialized.");
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
                
                _LOGGER.LogInfo($"Biome map for {key} successfully initialized.");
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

    public void DoScan(string body, MapType mapType, double longitude, double latitude, double altitude, double scanningCone)
    {
        // Sometimes, load data can be done before the textures are initialized
        if (!MapsInitialized)
            return;
        
        var celestialData = CelestialDataDictionary[body];
        if (celestialData == null)
        {
            _LOGGER.LogError($"Error retrieving CelestialData while executing scan, for body {body}.");
            return;
        }
        
        celestialData.DoScan(mapType, longitude, latitude, altitude, scanningCone);
    }

    public void ClearMap(string body, MapType mapType)
    {
        CelestialDataDictionary[body].ClearMap(mapType);
    }
}

