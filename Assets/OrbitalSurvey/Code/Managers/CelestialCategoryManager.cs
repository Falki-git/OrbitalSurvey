using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using OrbitalSurvey.Models;
using ReduxLib.Configuration;
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

        private static readonly ILogger Logger = ReduxLib.ReduxLib.GetLogger($"OrbitalSurvey|{typeof(CelestialCategoryManager).Name}");

        // The category stars are pinned to. See InitializeCelestialBodyCategories.
        private const string StarCategory = "Small";

        // Config sections written by the Lua patch orbital_survey_definitions.lua.
        private const string MaxRadiusSection = "orbital-survey-category-max-radius";
        private const string LocalizationSection = "orbital-survey-category-localization";
        private const string AltitudesSection = "orbital-survey-category-altitudes";

        public void InitializeConfigs()
        {
            MaxRadiusDefinition = new Dictionary<string, double>();
            AltitudesDefinition = new Dictionary<string, Dictionary<MapType, ScanningAltitudes>>();
            CategoryLocalization = new Dictionary<string, LocalizedString>();

            Logger.LogInfo("Initialization starting.");

            try
            {
                // The category definitions live in the mod's config file, populated by the
                // Lua patch orbital_survey_definitions.lua and overridable by other mods.
                var config = OrbitalSurveyPlugin.Instance.SWConfiguration;

                InitializeCategoryMaxRadiusDefinition(config);
                InitializeAltitudesDefinition(config);
                InitializeCategoryLocalization(config);

                Logger.LogInfo("Initialization finished successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception in CelestialCategoryManager.Initialization\n {ex}");
            }
        }

        private void InitializeCategoryMaxRadiusDefinition(IConfigFile config)
        {
            var section = config.GetOrCreateSection(MaxRadiusSection);

            // Read every category, then insert them ordered by ascending radius so that
            // InitializeCelestialBodyCategories (which walks largest-to-smallest) is correct
            // even if categories were defined or overridden in an arbitrary order.
            var categories = new List<(string category, double maxRadius)>();
            foreach (var category in section.Keys)
            {
                var maxRadius = Convert.ToDouble(section[category].Value);
                categories.Add((category, maxRadius));
                Logger.LogInfo($"Category '{category}' has a maximum radius of {maxRadius} m.");
            }

            foreach (var (category, maxRadius) in categories.OrderBy(c => c.maxRadius))
                MaxRadiusDefinition.Add(category, maxRadius);

            if (MaxRadiusDefinition.Count == 0)
            {
                Logger.LogError("Did not find any category definitions! This must not happen. " +
                                "Something is seriously wrong. Patch Manager configs are not properly defined.");
            }
        }

        private void InitializeAltitudesDefinition(IConfigFile config)
        {
            var section = config.GetOrCreateSection(AltitudesSection);
            var mapTypes = (MapType[])Enum.GetValues(typeof(MapType));

            foreach (var category in MaxRadiusDefinition.Keys)
            {
                Logger.LogInfo($"\"{category}\" scanning altitudes are:");

                var mapTypeAndAltitudesDict = new Dictionary<MapType, ScanningAltitudes>();

                foreach (var mapType in mapTypes)
                {
                    var prefix = $"{category}.{mapType}.";

                    if (!TryGetEntry(section, prefix + "Min", out var min) ||
                        !TryGetEntry(section, prefix + "Ideal", out var ideal) ||
                        !TryGetEntry(section, prefix + "Max", out var max))
                    {
                        Logger.LogError($"Missing scanning altitude definition(s) for '{category}' / {mapType}. " +
                                        "Patch Manager configs are not properly defined.");
                        continue;
                    }

                    var altitudes = new ScanningAltitudes
                    {
                        MinAltitude = Convert.ToSingle(min),
                        IdealAltitude = Convert.ToSingle(ideal),
                        MaxAltitude = Convert.ToSingle(max)
                    };

                    mapTypeAndAltitudesDict.Add(mapType, altitudes);
                    Logger.LogInfo(
                        $"    {mapType} => {altitudes.MinAltitude} m / {altitudes.IdealAltitude} m / {altitudes.MaxAltitude} m.");
                }

                if (mapTypeAndAltitudesDict.Count != mapTypes.Length)
                {
                    Logger.LogError("Did not find altitude definitions for each MapType! " +
                                    "This can't happen. Patch Manager configs are not properly defined.");
                }

                AltitudesDefinition.Add(category, mapTypeAndAltitudesDict);
            }

            if (AltitudesDefinition.Count == 0)
            {
                Logger.LogError(
                    "Did not find any category definitions for scanning altitudes! This must not happen. " +
                    "Something is seriously wrong. Patch Manager configs are not properly defined.");
            }
        }

        private void InitializeCategoryLocalization(IConfigFile config)
        {
            var section = config.GetOrCreateSection(LocalizationSection);

            foreach (var category in MaxRadiusDefinition.Keys)
            {
                if (TryGetEntry(section, category, out var value))
                {
                    CategoryLocalization.Add(category, new LocalizedString(value.ToString()));
                    Logger.LogInfo($"Localization string for category '{category}' added.");
                }
                else
                {
                    Logger.LogError($"No localization string defined for category '{category}'. " +
                                    "Patch Manager configs are not properly defined.");
                }
            }

            if (CategoryLocalization.Count == 0)
            {
                Logger.LogError("Did not find any category localization strings! This must not happen. " +
                                "Patch Manager configs are not properly defined.");
            }
        }

        private static bool TryGetEntry(IConfigSection section, string key, out object value)
        {
            if (section.Keys.Contains(key))
            {
                value = section[key].Value;
                return true;
            }

            value = null;
            return false;
        }

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
                    var message = $"Unable to assign a category to body '{body.Name}'. " +
                                  "There is no maximum radius value that is higher than the radius of the body. " +
                                  $"Body radius is {body.radius} m.";

                    // Expected for stars - Kerbol's radius dwarfs even the Giant maximum - and
                    // corrected by the IsStar special case below, so it isn't worth an error.
                    if (body.IsStar)
                        Logger.LogWarning(message);
                    else
                        Logger.LogError(message);

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

                // Special case for Kerbol - we'll define it as Small so it doesn't get the Giant category
                // which would clutter the UI unnecessarily since there are no other Giant bodies.
                // Resolved by name: this used to take categoryDefinitions[0], which follows whichever
                // category happens to be smallest and so silently moved Kerbol into Micro once the
                // asteroid-sized tiers were added.
                if (body.IsStar)
                {
                    if (MaxRadiusDefinition.ContainsKey(StarCategory))
                    {
                        bodyCategory = StarCategory;
                    }
                    else
                    {
                        Logger.LogWarning($"Category '{StarCategory}' is not defined, so star '{body.Name}' " +
                                          $"keeps the category '{bodyCategory}'.");
                    }
                }

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