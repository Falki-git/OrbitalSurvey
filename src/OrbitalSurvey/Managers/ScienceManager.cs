using BepInEx.Logging;
using KSP.Modules;
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

    public bool TriggerExperiment(Data_ScienceExperiment moduleData, MapType map, ExperimentLevel level)
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

        foreach (var experiment in moduleData.ExperimentStandings)
        {
            if (experiment.ExperimentID == experimentToTrigger)
            {
                experiment.CurrentExperimentState = ExperimentState.RUNNING;
                experiment.ConditionMet = true;
                
                _LOGGER.LogInfo($"Experiment {experimentToTrigger} triggered!");
                return true;
            }
        }
        
        _LOGGER.LogError($"Experiment {experimentToTrigger} not found in data module! Cannot trigger experiment");
        return false;
    }
}