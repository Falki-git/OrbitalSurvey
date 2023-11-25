using BepInEx.Logging;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;
using SpaceWarp.API.SaveGameManager;

namespace OrbitalSurvey.Managers;

public class SaveManager
{
    private SaveManager() { }
    
    public static SaveManager Instance { get; } = new();
    
    public Dictionary<string, SaveDataAdapter> bufferedLoadData;
    public bool HasBufferedLoadData;
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.SaveManager");

    

    public void Register()
    {
        ModSaves.RegisterSaveLoadGameData<Dictionary<string, SaveDataAdapter>>(
            OrbitalSurveyPlugin.ModGuid,
            OnSave,
            OnLoad
        );
    }

    public void OnSave(Dictionary<string, SaveDataAdapter> dataToSave)
    {
        _LOGGER.LogDebug("OnSave triggered.");
        
        dataToSave.Clear();

        foreach (var celestialData in Core.Instance.CelestialDataDictionary)
        {
            if (!celestialData.Value.ContainsData)
                continue;
            
            var celesAdapter = new SaveDataAdapter(); 
            
            foreach (var mapData in celestialData.Value.Maps) 
            { 
                if (!mapData.Value.HasData) 
                    continue;

                var mapsAdapter = new SaveDataAdapter.MapsAdapter();
                if (mapData.Value.IsFullyScanned || mapData.Value.CheckIfMapIsFullyScannedNow())
                {
                    mapsAdapter.IsFullyScanned = true;
                    mapsAdapter.DiscoveredPixels = string.Empty;
                }
                else
                {
                    mapsAdapter.IsFullyScanned = false;
                    mapsAdapter.DiscoveredPixels = SaveUtility.CompressData(mapData.Value.DiscoveredPixels);
                }

                celesAdapter.Maps.Add(mapData.Key, mapsAdapter);
            }
            
            dataToSave.Add(celestialData.Key, celesAdapter);
            
            _LOGGER.LogDebug($"{celestialData.Key} prepared for saving.");
        }
    }

    public void OnLoad(Dictionary<string, SaveDataAdapter> dataToLoad)
    {
        _LOGGER.LogDebug("OnLoad triggered.");

        bufferedLoadData = dataToLoad;
        HasBufferedLoadData = true;
        
        // skip loading if Maps haven't been initialized yet. Initialization will call LoadData();
        if (!Core.Instance.MapsInitialized)
            return;
        
        LoadData();
    }

    public void LoadData()
    {
        foreach (var celestialData in Core.Instance.CelestialDataDictionary)
        {
            if (!bufferedLoadData.ContainsKey(celestialData.Key))
            {
                // this body is not in saved data. Need to reset all data.
                foreach (var map in celestialData.Value.Maps)
                {
                    if (map.Value.HasData) 
                        map.Value.ClearMap();
                }
            }
            else
            {
                // this body has discovered pixels in saved data
                foreach (var map in celestialData.Value.Maps)
                {
                    var celesAdapter = bufferedLoadData[celestialData.Key];
                    
                    if (!celesAdapter.Maps.ContainsKey(map.Key))
                    {
                        // this specific map isn't in saved data. Need to reset all data
                        if (map.Value.HasData)
                            map.Value.ClearMap();
                    }
                    else
                    {
                        // this map holds data. Need to update the map.
                        if (celesAdapter.Maps[map.Key].IsFullyScanned)
                        {
                            map.Value.UpdateDiscoveredPixels(null, true);
                        }
                        else
                        {
                            var loadedPixels =
                                SaveUtility.DecompressData(celesAdapter.Maps[map.Key].DiscoveredPixels);
                            map.Value.UpdateDiscoveredPixels(loadedPixels);
                        }
                    }
                }
            }
        }

        bufferedLoadData = null;
        HasBufferedLoadData = false;
    }
}