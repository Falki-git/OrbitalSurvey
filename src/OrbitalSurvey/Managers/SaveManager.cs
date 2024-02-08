using BepInEx.Logging;
using KSP.Game;
using OrbitalSurvey.Models;
using OrbitalSurvey.UI;
using OrbitalSurvey.UI.Controls;
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
        
        dataToSave.Waypoints.Clear();
        foreach (var waypointModel in SceneController.Instance.Waypoints)
        {
            dataToSave.Waypoints.Add(waypointModel.Waypoint.Serialize());
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

    public async Task LoadData()
    {
        // mapping data
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
        _LOGGER.LogInfo("Mapping data loaded.");

        
        // waypoints
        SceneController.Instance.Waypoints.Clear();
        if (bufferedLoadData.Waypoints.Count > 0)
        {
            _LOGGER.LogDebug($"Found {bufferedLoadData.Waypoints.Count} waypoints to load.");
        
            // wait until all celestial bodies are loaded into UniverseModel
            await WaitUntilAllWaypointBodiesAreLoaded();
            
            foreach (var serializedWaypoint in bufferedLoadData.Waypoints)
            {
                var waypointModel = new WaypointModel();
                waypointModel.Waypoint = serializedWaypoint.Deserialize();
                waypointModel.Body = waypointModel.Waypoint.BodyName;
                waypointModel.MapPositionPercentage = UiUtility.GetPositionPercentageFromGeographicCoordinates(
                    waypointModel.Waypoint.Latitude, waypointModel.Waypoint.Longitude);
            
                var control = new MapMarkerControl(
                    isNameVisible: false, isGeoCoordinatesVisible: false,
                    MapMarkerControl.MarkerType.Waypoint, waypointModel.Waypoint.WaypointColor)
                {
                    NameValue = waypointModel.Waypoint.Name,
                    LatitudeValue = waypointModel.Waypoint.Latitude,
                    LongitudeValue = waypointModel.Waypoint.Longitude
                };
            
                waypointModel.Marker = control;
                SceneController.Instance.Waypoints.Add(waypointModel);
                _LOGGER.LogDebug($"Loaded waypoint '{waypointModel.Waypoint.Name}' on '{waypointModel.Waypoint.BodyName}'.");
            }
            _LOGGER.LogInfo("Waypoint data loaded.");
        }
        
        SceneController.Instance.WindowPosition = bufferedLoadData.WindowPosition;
        Core.Instance.SessionGuidString = bufferedLoadData.SessionGuidString;

        bufferedLoadData = null;
        HasBufferedLoadData = false;
        
        VesselManager.Instance.SetLastRefreshTimeToNow();
    }

    private async Task WaitUntilAllWaypointBodiesAreLoaded()
    {
        //select all bodies containing waypoints
        var waypointBodies = bufferedLoadData.Waypoints.Select(w => w.BodyName).Distinct();
        
        // try to find each body in UniverseModel and wait until it's loaded
        foreach (var waypointBody in waypointBodies)
        {
            bool isBodyLoaded = false;
            while (!isBodyLoaded)
            {
                var loadedCelestialBodies = GameManager.Instance.Game.UniverseModel.GetAllCelestialBodies();
                if (loadedCelestialBodies.Find(loadedBodies => loadedBodies.Name == waypointBody) != null)
                {
                    isBodyLoaded = true;
                }
                else
                {
                    // body isn't loaded yet, try again in 100 ms
                    await Task.Delay(100);
                }
            }
        }
    }
}