using BepInEx.Logging;
using OrbitalSurvey.Models;
using OrbitalSurvey.UI;
using OrbitalSurvey.Utilities;
using SpaceWarp.API.SaveGameManager;

namespace OrbitalSurvey.Managers;

public class SaveManager
{
    private SaveManager() { }
    
    public static SaveManager Instance { get; } = new();
    
    public SaveDataAdapter bufferedLoadData;
    public bool HasBufferedLoadData;
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.SaveManager");

    public void Register()
    {
        ModSaves.RegisterSaveLoadGameData<SaveDataAdapter>(
            OrbitalSurveyPlugin.ModGuid,
            OnSave,
            OnLoad
        );
    }

    public void OnSave(SaveDataAdapter dataToSave)
    {
        _LOGGER.LogDebug("OnSave triggered.");

        dataToSave.WindowPosition = SceneController.Instance.WindowPosition;
        dataToSave.SessionGuidString = Utility.SessionGuidString;
        dataToSave.Bodies.Clear();

        foreach (var celestialData in Core.Instance.CelestialDataDictionary)
        {
            if (!celestialData.Value.ContainsData)
                continue;
            
            var mapsDataAdapter = new Dictionary<MapType, SaveDataAdapter.MapsAdapter>();
            
            foreach (var mapData in celestialData.Value.Maps) 
            { 
                if (!mapData.Value.HasData) 
                    continue;

                var mapsAdapter = new SaveDataAdapter.MapsAdapter();
                if (mapData.Value.IsFullyScanned)
                {
                    mapsAdapter.IsFullyScanned = true;
                    mapsAdapter.DiscoveredPixels = string.Empty;
                }
                else
                {
                    mapsAdapter.IsFullyScanned = false;
                    mapsAdapter.DiscoveredPixels = SaveUtility.CompressData(mapData.Value.DiscoveredPixels);
                }

                mapsAdapter.ExperimentLevel = mapData.Value.ExperimentLevel;
                
                mapsDataAdapter.Add(mapData.Key, mapsAdapter);
            }
            
            dataToSave.Bodies.Add(celestialData.Key, mapsDataAdapter);
            
            _LOGGER.LogDebug($"{celestialData.Key} prepared for saving.");
        }
    }

    public void OnLoad(SaveDataAdapter dataToLoad)
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
            if (!bufferedLoadData.Bodies.ContainsKey(celestialData.Key))
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
                    var mapsDataAdapter = bufferedLoadData.Bodies[celestialData.Key];
                    
                    if (!mapsDataAdapter.ContainsKey(map.Key))
                    {
                        // this specific map isn't in saved data. Need to reset all data
                        if (map.Value.HasData)
                            map.Value.ClearMap();
                    }
                    else
                    {
                        // this map holds data. Need to update the map.
                        if (mapsDataAdapter[map.Key].IsFullyScanned)
                        {
                            map.Value.UpdateDiscoveredPixels(null, true);
                        }
                        else
                        {
                            var loadedPixels =
                                SaveUtility.DecompressData(mapsDataAdapter[map.Key].DiscoveredPixels);
                            map.Value.UpdateDiscoveredPixels(loadedPixels);
                        }

                        map.Value.ExperimentLevel = mapsDataAdapter[map.Key].ExperimentLevel;
                    }
                }
            }
        }

        SceneController.Instance.WindowPosition = bufferedLoadData.WindowPosition;
        Core.Instance.SessionGuidString = bufferedLoadData.SessionGuidString;

        bufferedLoadData = null;
        HasBufferedLoadData = false;
        
        VesselManager.Instance.SetLastRefreshTimeToNow();
    }
}