using BepInEx.Logging;
using KSP.Game;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;
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

    public void InitializeCelestialData(Dictionary<string, Texture2D> scaledVisualTextures)
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
            
            // Visual map
            var mapData = new MapData()
            {
                ScannedMap = scaledVisualTextures[key],
                HiddenMap = AssetUtility.GenerateHiddenMap(),
                CurrentMap = AssetUtility.GenerateHiddenMap()
            };

            var celesData = new CelestialData
            {
                Body = body
            };
            celesData.Maps.Add(MapType.Visual, mapData);
            
            CelestialDataDictionary.Add(key, celesData);
            _LOGGER.LogInfo($"Initialized CelestialDataDictionary for {key}.");
        }
        
        MapsInitialized = true;
        _LOGGER.LogInfo($"Finished CelestialDataDictionary initialization with {CelestialDataDictionary.Count} entries.");
    }
}

