using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using OrbitalSurvey.Models;
// using PatchManager.SassyPatching; // PatchManager API changed; SassyPatching/DataValue no longer available
using UnityEngine;
using Utility = OrbitalSurvey.Utilities.Utility;
using ILogger = ReduxLib.Logging.ILogger;

namespace OrbitalSurvey.Managers
{
    public class CelestialCategoryManager
    {
        private CelestialCategoryManager()
        {
        }

        public static CelestialCategoryManager Instance { get; } = new();
        public Dictionary<string, Dictionary<MapType, ScanningAltitudes>> AltitudesDefinition { get; private set; }
        public Dictionary<string, double> MaxRadiusDefinition { get; private set; }
        public Dictionary<string, LocalizedString> CategoryLocalization { get; private set; }

        public bool IsCelestialBodyCategoryInitialized;
        internal Dictionary<string, string> CelestialBodyCategory;

        private static readonly ILogger Logger = ReduxLib.ReduxLib.GetLogger("OrbitalSurvey.CelestialCategoryManager");

        public void InitializeConfigs()
        {
            // PatchManager API changed; SassyPatching/DataValue no longer available.
            // Original implementation commented out below — rewrite using the new PatchManager (Lua/C# patches).
            MaxRadiusDefinition = new Dictionary<string, double>();
            AltitudesDefinition = new Dictionary<string, Dictionary<MapType, ScanningAltitudes>>();
            CategoryLocalization = new Dictionary<string, LocalizedString>();
            Logger.LogWarning("InitializeConfigs: PatchManager config loading not yet reimplemented.");

            /*
            Logger.LogInfo("Initialization starting.");

            try
            {
                var configs = PatchManager.Core.CoreModule.CurrentUniverse.Configs;
                var definitions = configs["orbital-survey-definitions"];

                InitializeCategoryMaxRadiusDefinition(definitions);
                InitializeAltitudesDefinition(definitions);
                InitializeCategoryLocalization(definitions);

                Logger.LogInfo($"Initialization finished successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception in CelestialCategoryManager.Initialization\n {ex}");
            }
            */
        }

        /*
        private void InitializeCategoryMaxRadiusDefinition(IReadOnlyDictionary<string, DataValue> definitions)
        {
            MaxRadiusDefinition = new Dictionary<string, double>();
            var categoryMaxRadiusDefinition = definitions["celestial-category__maximum-radius"].Dictionary;
            foreach (var categoryMaxRadius in categoryMaxRadiusDefinition)
            {
                var category = categoryMaxRadius.Key;
                var maxRadius = categoryMaxRadius.Value.Integer;

                MaxRadiusDefinition.Add(category, maxRadius);

                Logger.LogInfo($"Category '{category}' has a maximum radius of {maxRadius} m.");
            }

            if (MaxRadiusDefinition.Count == 0)
            {
                Logger.LogError("Did not find any category definitions! This must not happen. " +
                                "Something is seriously wrong. Patch Manager configs are not properly defined.");
            }
        }

        private void InitializeAltitudesDefinition(IReadOnlyDictionary<string, DataValue> definitions)
        {
            AltitudesDefinition = new Dictionary<string, Dictionary<MapType, ScanningAltitudes>>();
            var categoryScanningAltitudes = definitions["celestial-category__scanning-altitudes"].Dictionary;
            foreach (var categoryAndMapTypes in categoryScanningAltitudes)
            {
                Logger.LogInfo($"\"{categoryAndMapTypes.Key}\" scanning altitudes are:");

                var mapTypeAndAltitudesDict = new Dictionary<MapType, ScanningAltitudes>();

                foreach (var mapTypeAndAltitudes in categoryAndMapTypes.Value.Dictionary)
                {
                    var mapType = (MapType)Enum.Parse(typeof(MapType), mapTypeAndAltitudes.Key);

                    var altitudes = new ScanningAltitudes
                    {
                        MinAltitude = mapTypeAndAltitudes.Value.Dictionary["MinAltitude"].Integer,
                        IdealAltitude = mapTypeAndAltitudes.Value.Dictionary["IdealAltitude"].Integer,
                        MaxAltitude = mapTypeAndAltitudes.Value.Dictionary["MaxAltitude"].Integer
                    };

                    mapTypeAndAltitudesDict.Add(mapType, altitudes);
                    Logger.LogInfo(
                        $"    {mapType} => {altitudes.MinAltitude} m / {altitudes.IdealAltitude} m / {altitudes.MaxAltitude} m.");
                }

                if (mapTypeAndAltitudesDict.Count != Enum.GetValues(typeof(MapType)).Length)
                {
                    Logger.LogError("Did not find altitude definitions for each MapType! " +
                                    " This can't happen. Patch Manager configs are not properly defined.");
                }

                AltitudesDefinition.Add(categoryAndMapTypes.Key, mapTypeAndAltitudesDict);
            }

            if (AltitudesDefinition.Count == 0)
            {
                Logger.LogError(
                    "Did not find any category definitions for scanning altitudes! This must not happen. " +
                    "Something is seriously wrong. Patch Manager configs are not properly defined.");
            }
        }

        private void InitializeCategoryLocalization(IReadOnlyDictionary<string, DataValue> definitions)
        {
            CategoryLocalization = new Dictionary<string, LocalizedString>();
            var categoryLocalizations = definitions["celestial-category__localization-string"].Dictionary;

            foreach (var catLoc in categoryLocalizations)
            {
                CategoryLocalization.Add(catLoc.Key, new LocalizedString(catLoc.Value.String));
                Logger.LogInfo($"Localization string for category '{catLoc.Key}' added.");
            }

            if (CategoryLocalization.Count == 0)
            {
                Logger.LogError("Did not find any category localization strings! This must not happen. " +
                                "Patch Manager configs are not properly defined.");
            }
        }
        */

        public void InitializeCelestialBodyCategories()
        {
            var celestialBodies = Utility.GetAllCelestialBodies();
            CelestialBodyCategory = new();

            foreach (var body in celestialBodies)
            {
                string bodyCategory = string.Empty;

                var categoryDefinitions = MaxRadiusDefinition.ToList();

                // find the category for this body
                for (int i = categoryDefinitions.Count - 1; i >= 0; i--)
                {
                    if (categoryDefinitions[i].Value > body.radius)
                    {
                        bodyCategory = categoryDefinitions[i].Key;
                    }
                    else
                    {
                        break;
                    }
                }

                if (string.IsNullOrEmpty(bodyCategory))
                {
                    Logger.LogError($"Unable to assign a category to body '{body.Name}'. " +
                                    "There is no maximum radius value that is higher than the radius of the body. " +
                                    $"Body radius is {body.radius} m.");

                    if (MaxRadiusDefinition.Count > 0)
                    {
                        bodyCategory = MaxRadiusDefinition.Last().Key;
                        Logger.LogWarning(
                            $"'{body.Name}' is assigned to the last available category, which is '{bodyCategory}'.");
                    }
                    else
                    {
                        Logger.LogError("There are no categories defined. Something is seriously wrong. " +
                                        "Patch Manager configs are not properly defined.");
                    }
                }

                // special case for Kerbol - we'll define it as Small so it doesn't get the Giant category
                // which would clutter the UI unnecessarily since there are no other Giant bodies 
                if (body.IsStar) bodyCategory = categoryDefinitions[0].Key;

                CelestialBodyCategory.Add(body.Name, bodyCategory);
                Logger.LogInfo($"Body '{body.Name}' is assigned to category '{bodyCategory}'.");
            }

            IsCelestialBodyCategoryInitialized = true;
        }

        public (string category, ScanningAltitudes altitudes) GetScanningStats(string body, MapType scanningMode)
        {
            var category = CelestialBodyCategory[body];
            return (category, AltitudesDefinition[category][scanningMode]);
        }

        public ScanningAltitudes GetOabScanningStats(string category, MapType scanningMode)
        {
            return AltitudesDefinition[category][scanningMode];
        }

        public List<(string category, ScanningAltitudes altitudes)> GetCategoryAltitudesForGivenMapType(
            MapType mapTypeTarget)
        {
            var toReturn = new List<(string, ScanningAltitudes)>();

            foreach (var (category, mapTypeAltitudeDict) in AltitudesDefinition)
            {
                foreach (var (mapType, altitudes) in mapTypeAltitudeDict)
                {
                    if (mapType == mapTypeTarget)
                    {
                        // only return categories that have at least one celestial body attached to it
                        if (CelestialBodyCategory.ContainsValue(category))
                        {
                            toReturn.Add((category, altitudes));
                        }
                    }
                }
            }

            return toReturn;
        }
    }
}