using System;
using System.Collections.Generic;
using System.Linq;
using KSP.Game;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;

namespace OrbitalSurvey.Managers
{
    public class Core // : MonoBehaviour
    {
        private Core()
        {
            CelestialDataDictionary = new();
        }
        

        public static Core Instance { get; } = new();
        public CelestialDataDictionary CelestialDataDictionary { get; set; }

        public bool MapsInitialized { get; set; }
        public string SessionGuidString;
        
        private static readonly ReduxLib.Logging.ILogger Logger = ReduxLib.ReduxLib.GetLogger($"OrbitalSurvey|{typeof(Core).Name}");

        public void InitializeCelestialData()
        {
            var celestialBodies = GameManager.Instance.Game?.UniverseModel?.GetAllCelestialBodies();
            if (celestialBodies == null)
            {
                Logger.LogError(
                    "Error fetching celestial bodies from UniverseModel. Celestial bodies will not initialize!");
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

                    // PRE REDUX CODE
                    // celesData.Maps[MapType.Visual].ScannedMap =
                    //     AssetManager.GetAsset<Texture2D>(
                    //         AssetUtility.Instance.VisualBundleAssetAddresses[$"{key}_{Settings.ActiveResolution}"]
                    //     );

                    celesData.Maps[MapType.Visual].ScannedMap =
                        AssetUtility.Instance.GetTextureAsset(AssetType.Visual, $"{key}_{Settings.ActiveResolution}");

                    // PRE REDUX CODE
                    // celesData.Maps[MapType.Visual].ScannedMapHiRes =
                    //     AssetManager.GetAsset<Texture2D>(
                    //         AssetUtility.Instance.VisualBundleAssetAddresses[$"{key}_{Settings.HighResolution}"]
                    //     );
                    
                    celesData.Maps[MapType.Visual].ScannedMapHiRes =
                        AssetUtility.Instance.GetTextureAsset(AssetType.Visual, $"{key}_{Settings.HighResolution}");

                    Logger.LogInfo($"Visual map for {key} successfully initialized.");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error loading visual map for {key}.\n" + ex);
                }

                // Biome map
                try
                {
                    // PRE REDUX CODE
                    // celesData.Maps[MapType.Biome].ScannedMap =
                    //     AssetManager.GetAsset<Texture2D>(
                    //         AssetUtility.Instance.BiomeBundleAssetAddresses[$"{key}_{Settings.ActiveResolution}"]
                    //     );
                    celesData.Maps[MapType.Biome].ScannedMap =
                        AssetUtility.Instance.GetTextureAsset(AssetType.Biome, $"{key}_{Settings.ActiveResolution}");

                    // PRE REDUX CODE
                    // celesData.Maps[MapType.Biome].ScannedMapHiRes =
                    //     AssetManager.GetAsset<Texture2D>(
                    //         AssetUtility.Instance.BiomeBundleAssetAddresses[$"{key}_{Settings.HighResolution}"]
                    //     );
                    celesData.Maps[MapType.Biome].ScannedMapHiRes =
                        AssetUtility.Instance.GetTextureAsset(AssetType.Biome, $"{key}_{Settings.HighResolution}");
                    
                    Logger.LogInfo($"Biome map for {key} successfully initialized.");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error loading biome map for {key}.\n" + ex);
                }

                CelestialDataDictionary.Add(key, celesData);
                Logger.LogInfo($"Initialized CelestialDataDictionary for {key}.");
            }

            MapsInitialized = true;
            Logger.LogInfo(
                $"Finished CelestialDataDictionary initialization with {CelestialDataDictionary.Count} entries.");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            if (SaveManager.Instance.HasBufferedLoadData)
                SaveManager.Instance.LoadData();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public void DoScan(string body, MapType mapType, double longitude, double latitude, double altitude,
            ScanningStats scanningStats, string vesselGuid, bool isRetroActiveScanning = false)
        {
            // sometimes, load data can be done before the textures are initialized
            if (!MapsInitialized || !CelestialDataDictionary.ContainsKey(body))
            {
                return;
            }

            // skip scanning if we're below min or above max altitude
            if (altitude < scanningStats.MinAltitude || altitude > scanningStats.MaxAltitude)
            {
                return;
            }

            var celestialData = CelestialDataDictionary[body];

            celestialData.DoScan(mapType, longitude, latitude, altitude, scanningStats, isRetroActiveScanning,
                vesselGuid);
        }

        public void ClearMap(string body, MapType mapType)
        {
            CelestialDataDictionary[body].ClearMap(mapType);
        }

        public IEnumerable<string> GetBodiesContainingData()
        {
            var toReturn = CelestialDataDictionary
                .Where(entry => entry.Value.ContainsData)
                .Select(entry => entry.Key)
                .ToList();

            // if nothing has been discovered so far, return the HomeWorld (Kerbin)
            if (!toReturn.Any())
            {
                var homeWorld = GameManager.Instance.Game?.UniverseModel?
                    .GetAllCelestialBodies()
                    .Find(b => b.isHomeWorld).Name;

                toReturn.Add(homeWorld);
            }

            return toReturn;
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

        public void CheckIfExperimentNeedsToTrigger(string body, MapType mapType)
        {
            var celestialData = CelestialDataDictionary[body];
            var experimentLevel = celestialData.CheckIfExperimentNeedsToTrigger(mapType);

            if (experimentLevel != ExperimentLevel.None)
            {
                // Only announce the milestone if an experiment was actually awarded - a body with no
                // science regions (Beyl) would otherwise pop a "complete" notification for nothing.
                if (ScienceManager.Instance.TriggerExperiment(body, mapType, experimentLevel))
                    NotificationUtility.Instance.NotifyExperimentComplete(body, experimentLevel);
            }
        }
    }
}