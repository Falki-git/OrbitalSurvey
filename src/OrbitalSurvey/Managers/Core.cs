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
    public string SessionGuidString;
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.Core");
    
    public void InitializeCelestialData()
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
                //celesData.Maps[MapType.Visual].ScannedMap = assetUtility.ScaledVisualTextures[key];
                
                celesData.Maps[MapType.Visual].ScannedMap =
                    AssetManager.GetAsset<Texture2D>(
                        OrbitalSurveyPlugin.Instance.AssetUtility.VisualBundleAssetAddresses[$"{key}_{Settings.ActiveResolution}"]
                        );
                
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
                    AssetManager.GetAsset<Texture2D>(
                        OrbitalSurveyPlugin.Instance.AssetUtility.BiomeBundleAssetAddresses[$"{key}_{Settings.ActiveResolution}"]
                        );
                
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
        
        if (SaveManager.Instance.HasBufferedLoadData)
            SaveManager.Instance.LoadData();
    }

    public void DoScan(string body, MapType mapType, double longitude, double latitude, double altitude, float scanningCone)
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

    public IEnumerable<string> GetBodiesContainingData()
    {
        return CelestialDataDictionary
            .Where(entry => entry.Value.ContainsData)
            .Select(entry => entry.Key);
    }
    
    
    public delegate void MapHasDataValueChanged(IEnumerable<string> bodiesWithData);
    public event MapHasDataValueChanged OnMapHasDataValueChanged;

    /// <summary>
    /// OnMapHasDataValueChanged is triggered when 'HasData' property of a map is changed.
    /// I.e. when a previously unexplored Body/Map now receives data after scanning begins.
    /// </summary>
    public void InvokeOnMapHasDataValueChanged()
    {
        OnMapHasDataValueChanged?.Invoke(GetBodiesContainingData());
    }
}

