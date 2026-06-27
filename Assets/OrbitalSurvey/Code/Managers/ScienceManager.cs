using System.Collections.Generic;
using KSP.Game;
using KSP.Game.Science;
using KSP.Messages;
using KSP.Sim.impl;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;

namespace OrbitalSurvey.Managers
{
    /// <summary>
    /// Handles triggering mapping experiments
    /// </summary>
    public class ScienceManager
    {
        private ScienceManager()
        {
        }

        public static ScienceManager Instance { get; } = new();
        
        private static readonly ReduxLib.Logging.ILogger Logger = ReduxLib.ReduxLib.GetLogger("OrbitalSurvey.ScienceManager");

        public Dictionary<MapType, Dictionary<ExperimentLevel, string>> ExperimentDefinitions = new()
        {
            {
                MapType.Visual, new Dictionary<ExperimentLevel, string>
                {
                    { ExperimentLevel.Quarter, "orbital_survey_visual_mapping_high_25" },
                    { ExperimentLevel.Half, "orbital_survey_visual_mapping_high_50" },
                    { ExperimentLevel.ThreeQuarters, "orbital_survey_visual_mapping_high_75" },
                    { ExperimentLevel.Full, "orbital_survey_visual_mapping_high_100" }
                }
            },
            {
                MapType.Biome, new Dictionary<ExperimentLevel, string>
                {
                    { ExperimentLevel.Quarter, "orbital_survey_biome_mapping_high_25" },
                    { ExperimentLevel.Half, "orbital_survey_biome_mapping_high_50" },
                    { ExperimentLevel.ThreeQuarters, "orbital_survey_biome_mapping_high_75" },
                    { ExperimentLevel.Full, "orbital_survey_biome_mapping_high_100" }
                }
            }
        };

        /// <summary>
        /// Triggers experiment for the given Body, MapType and ExperimentLevel.
        /// This method is called when the experiment level reaches the next milestone (1/4 -> 1/2 -> 3/4 -> full)
        /// </summary>
        public bool TriggerExperiment(string body, MapType map, ExperimentLevel level)
        {
            if (!ExperimentDefinitions.ContainsKey(map))
            {
                Logger.LogError($"MapType {map} not found! Experiment cannot trigger");
                return false;
            }

            if (!ExperimentDefinitions[map].ContainsKey(level))
            {
                Logger.LogError($"ExperimentLevel {level} not found! Experiment cannot trigger");
                return false;
            }

            // grab the experimentId that needs to trigger
            string experimentIdToTrigger = ExperimentDefinitions[map][level];

            // grabs the experiment definition from the game's ScienceManager
            var experimentDefinition =
                GameManager.Instance.Game.ScienceManager.ScienceExperimentsDataStore.GetExperimentDefinition(
                    experimentIdToTrigger);

            if (experimentDefinition == null)
            {
                Logger.LogError(
                    $"Experiment with ID '{experimentIdToTrigger}' not found in ScienceExperimentDataStore!");
                return false;
            }

            var cbToScienceRegions = ReflectionUtility.GetPrivateField<Dictionary<string, CelestialBodyScienceRegionsData>>(
                GameManager.Instance.Game.ScienceManager.ScienceRegionsDataProvider, "_cbToScienceRegions");

            // we need to manually grab the CelestialBodyScalar
            var celestialScalar = cbToScienceRegions[body].SituationData.CelestialBodyScalar;

            // we'll use the HighOrbit scalar since the science value is balanced around the vessel being in HighOrbit
            var highOrbitScalar = cbToScienceRegions[body].SituationData.HighOrbitScalar;

            // find all vessels that participated in scanning of this body, for this MapType and for this ExperimentLevel
            var vesselGuids = Core.Instance.CelestialDataDictionary[body].Maps[map]
                .VesselGuidsParticipatingInCurrentExperimentLevel;

            if (vesselGuids == null || vesselGuids.Count == 0)
            {
                Logger.LogError(
                    $"No vessels found that participated in scanning this level! This shouldn't be possible." +
                    $"Body: {body}, MapType: {map}, ExperimentLevel: {level}");
                return false;
            }

            var vessels = Utility.GetAllVesselsWithGuids(vesselGuids);

            // iterate through each vessel and trigger experiments on all of them
            foreach (var vessel in vessels)
            {
                var scienceModule = vessel.GetModule<PartComponentModule_ScienceExperiment>();
                if (scienceModule == null) continue;

                ResearchReport researchReport = new ResearchReport(
                    experimentID: experimentDefinition.ExperimentID,
                    displayName: experimentDefinition.DataReportDisplayName,
                    vessel.VesselScienceRegionSituation.ResearchLocation,
                    ScienceReportType.DataType,
                    initialScienceValue: experimentDefinition.DataValue * celestialScalar * highOrbitScalar,
                    flavorText: experimentDefinition.DataFlavorDescriptions[0].LocalizationTag
                );

                // manually tweak Body and Situation. Set Region to null since experiments don't require Region
                researchReport.Location.RequiresRegion = false;
                researchReport.Location.SetBodyName(body);
                researchReport.Location.SetScienceRegion(null);
                researchReport.Location.SetScienceSituation(ScienceSitutation.HighOrbit);
                researchReport.ResearchLocationID = researchReport.Location.ResearchLocationId;
                
                scienceModule.Part.PartOwner.SimulationObject.ScienceStorage.StoreResearchReport(researchReport);

                ResearchReportAcquiredMessage message;
                if (GameManager.Instance.Game.Messages.TryCreateMessage(out message))
                {
                    GameManager.Instance.Game.Messages.Publish(message);
                }
            }

            // purge the list of vessels that participated in the current experiment level since we're now on a new level
            Core.Instance.CelestialDataDictionary[body].Maps[map].VesselGuidsParticipatingInCurrentExperimentLevel
                .Clear();

            Logger.LogInfo($"Experiment {experimentIdToTrigger} triggered!");
            return true;
        }
    }
}