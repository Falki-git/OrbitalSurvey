using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using OrbitalSurvey.Models;
using Utility = OrbitalSurvey.Utilities.Utility;
using ILogger = ReduxLib.Logging.ILogger;

namespace OrbitalSurvey.Managers
{
    public class CelestialCategoryManager
    {
        private CelestialCategoryManager() { }

        public static CelestialCategoryManager Instance { get; } = new();
        public Dictionary<string, Dictionary<MapType, ScanningAltitudes>> AltitudesDefinition { get; private set; }
        public Dictionary<string, double> MaxRadiusDefinition { get; private set; }
        public Dictionary<string, LocalizedString> CategoryLocalization { get; private set; }

        public bool IsCelestialBodyCategoryInitialized;
        internal Dictionary<string, string> CelestialBodyCategory;

        private static readonly ILogger Logger = ReduxLib.ReduxLib.GetLogger("OrbitalSurvey.CelestialCategoryManager");

        private const string ConfigSection = "orbital-survey-definitions";

        public void InitializeConfigs()
        {
            Logger.LogInfo("Initialization starting.");

            try
            {
                var cfg = OrbitalSurveyPlugin.Instance.SWConfiguration;

                MaxRadiusDefinition = new Dictionary<string, double>
                {
                    ["Small"]  = (int)cfg.Bind(ConfigSection, "celestial-category__maximum-radius__Small",  150000,    "Max radius (m) for a body to be classified as Small.").Value,
                    ["Medium"] = (int)cfg.Bind(ConfigSection, "celestial-category__maximum-radius__Medium", 350000,    "Max radius (m) for a body to be classified as Medium.").Value,
                    ["Large"]  = (int)cfg.Bind(ConfigSection, "celestial-category__maximum-radius__Large",  10000000,  "Max radius (m) for a body to be classified as Large.").Value,
                    ["Giant"]  = (int)cfg.Bind(ConfigSection, "celestial-category__maximum-radius__Giant",  100000000, "Max radius (m) for a body to be classified as Giant.").Value,
                };

                foreach (var (category, maxRadius) in MaxRadiusDefinition)
                    Logger.LogInfo($"Category '{category}' has a maximum radius of {maxRadius} m.");

                if (MaxRadiusDefinition.Count == 0)
                    Logger.LogError("Did not find any category definitions! This must not happen. " +
                                    "Something is seriously wrong. Patch Manager configs are not properly defined.");

                CategoryLocalization = new Dictionary<string, LocalizedString>
                {
                    ["Small"]  = new((string)cfg.Bind(ConfigSection, "celestial-category__localization-string__Small",  "PartModules/OrbitalSurvey/BodyCategory/Small",  "Localization tag for the Small category display name.").Value),
                    ["Medium"] = new((string)cfg.Bind(ConfigSection, "celestial-category__localization-string__Medium", "PartModules/OrbitalSurvey/BodyCategory/Medium", "Localization tag for the Medium category display name.").Value),
                    ["Large"]  = new((string)cfg.Bind(ConfigSection, "celestial-category__localization-string__Large",  "PartModules/OrbitalSurvey/BodyCategory/Large",  "Localization tag for the Large category display name.").Value),
                    ["Giant"]  = new((string)cfg.Bind(ConfigSection, "celestial-category__localization-string__Giant",  "PartModules/OrbitalSurvey/BodyCategory/Giant",  "Localization tag for the Giant category display name.").Value),
                };

                foreach (var (category, _) in CategoryLocalization)
                    Logger.LogInfo($"Localization string for category '{category}' added.");

                if (CategoryLocalization.Count == 0)
                    Logger.LogError("Did not find any category localization strings! This must not happen. " +
                                    "Something is seriously wrong. Patch Manager configs are not properly defined.");

                AltitudesDefinition = new Dictionary<string, Dictionary<MapType, ScanningAltitudes>>();
                foreach (var category in MaxRadiusDefinition.Keys)
                {
                    Logger.LogInfo($"\"{category}\" scanning altitudes are:");

                    var mapTypeDict = new Dictionary<MapType, ScanningAltitudes>();
                    foreach (MapType mapType in Enum.GetValues(typeof(MapType)))
                    {
                        var prefix = $"celestial-category__scanning-altitudes__{category}__{mapType}";
                        var (defMin, defIdeal, defMax) = GetDefaultAltitudes(category, mapType);

                        var altitudes = new ScanningAltitudes
                        {
                            MinAltitude   = (float)cfg.Bind(ConfigSection, prefix + "__MinAltitude",   defMin,   $"Min scan altitude (m) for {category} bodies in {mapType} mode.").Value,
                            IdealAltitude = (float)cfg.Bind(ConfigSection, prefix + "__IdealAltitude", defIdeal, $"Ideal scan altitude (m) for {category} bodies in {mapType} mode.").Value,
                            MaxAltitude   = (float)cfg.Bind(ConfigSection, prefix + "__MaxAltitude",   defMax,   $"Max scan altitude (m) for {category} bodies in {mapType} mode.").Value,
                        };

                        mapTypeDict.Add(mapType, altitudes);
                        Logger.LogInfo($"    {mapType} => {altitudes.MinAltitude} m / {altitudes.IdealAltitude} m / {altitudes.MaxAltitude} m.");
                    }

                    if (mapTypeDict.Count != Enum.GetValues(typeof(MapType)).Length)
                        Logger.LogError("Did not find altitude definitions for each MapType! " +
                                        "This can't happen. Patch Manager configs are not properly defined.");

                    AltitudesDefinition.Add(category, mapTypeDict);
                }

                if (AltitudesDefinition.Count == 0)
                    Logger.LogError("Did not find any category definitions for scanning altitudes! This must not happen. " +
                                    "Something is seriously wrong. Patch Manager configs are not properly defined.");

                Logger.LogInfo("Initialization finished successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception in CelestialCategoryManager.Initialization\n {ex}");
            }
        }

        private static (float min, float ideal, float max) GetDefaultAltitudes(string category, MapType mapType) =>
            (category, mapType) switch
            {
                ("Small",  MapType.Visual) => (60000f,     170000f,   220000f),
                ("Small",  MapType.Biome)  => (60000f,     220000f,   300000f),
                ("Medium", MapType.Visual) => (100000f,    300000f,   500000f),
                ("Medium", MapType.Biome)  => (300000f,    500000f,   700000f),
                ("Large",  MapType.Visual) => (500000f,    800000f,   1100000f),
                ("Large",  MapType.Biome)  => (1000000f,   1500000f,  2000000f),
                ("Giant",  MapType.Visual) => (5000000f,   8000000f,  11000000f),
                ("Giant",  MapType.Biome)  => (10000000f,  15000000f, 20000000f),
                _ => (0f, 0f, 0f)
            };

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
