using BepInEx.Logging;
using KSP.Sim.impl;
using OrbitalSurvey.Models;
using Logger = BepInEx.Logging.Logger;

namespace OrbitalSurvey.Managers;

public class ScienceManager
{
    private ScienceManager() { }
    public static ScienceManager Instance { get; } = new();
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.ScienceManager");

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

    public bool TriggerExperiment(PartComponentModule_ScienceExperiment scienceModule, MapType map, ExperimentLevel level)
    {
        if (!ExperimentDefinitions.ContainsKey(map))
        {
            _LOGGER.LogError($"MapType {map} not found! Experiment cannot trigger");
            return false;
        }
        
        if (!ExperimentDefinitions[map].ContainsKey(level))
        {
            _LOGGER.LogError($"ExperimentLevel {level} not found! Experiment cannot trigger");
            return false;
        }
        
        string experimentToTrigger = ExperimentDefinitions[map][level];

        var experiment = scienceModule.GetExperimentDefinitionByID(experimentToTrigger);
        if (experiment == null)
        {
            _LOGGER.LogError($"Experiment {experimentToTrigger} ID not found. Cannot trigger the experiment!");
            return false;
        }
        
        var index = scienceModule.GetExperimentIndexFromID(experimentToTrigger);
        
        scienceModule.CreateScienceReports(experiment, index);
        
        _LOGGER.LogInfo($"Experiment {experimentToTrigger} triggered!");
        return true;
    }
}