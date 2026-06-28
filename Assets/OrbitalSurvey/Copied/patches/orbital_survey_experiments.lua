-- Orbital Survey: science experiment definitions.
--
-- Creates the eight mapping-milestone experiments (visual & biome, at 25/50/75/100%
-- completion). This replaces the legacy orbital_survey_experiment_definitions.patch.

-- mode      : "VisualMapping" or "BiomeMapping" (used to build localization keys)
-- milestone : "25" | "50" | "75" | "100"
-- dataValue : science data value for the experiment
local function mappingExperiment(name, mode, milestone, dataValue)
    PM.Science:NewExperiment(name, function(experiment)
        experiment.DisplayName = "OrbitalSurvey/Experiments/OabDescription/DisplayName/" .. mode
        experiment.DisplayRequirements = "OrbitalSurvey/Experiments/OabDescription/DisplayRequirements"
        experiment.ExperimentType = "DataType"
        experiment.DataReportDisplayName =
            "OrbitalSurvey/Experiments/DataReportDisplayName/" .. mode .. "/" .. milestone
        experiment.DataFlavorDescriptions = {
            {
                ResearchLocationID = "Default",
                LocalizationTag =
                    "OrbitalSurvey/Experiments/DataFlavor/" .. mode .. "/Default/" .. milestone,
            },
        }
        -- ValidLocations is intentionally not set: ExperimentDefinition initialises it to
        -- an empty list, which is exactly what the legacy patch assigned ([]).
        -- Whole numbers are serialized as JSON integers automatically, so no J.Int needed.
        experiment.DataValue = dataValue
        experiment.TransmissionSize = 100
    end)
end

-- Visual mapping
mappingExperiment("orbital_survey_visual_mapping_high_25",  "VisualMapping", "25",  3)
mappingExperiment("orbital_survey_visual_mapping_high_50",  "VisualMapping", "50",  3)
mappingExperiment("orbital_survey_visual_mapping_high_75",  "VisualMapping", "75",  3)
mappingExperiment("orbital_survey_visual_mapping_high_100", "VisualMapping", "100", 6)

-- Biome mapping
mappingExperiment("orbital_survey_biome_mapping_high_25",  "BiomeMapping", "25",  3)
mappingExperiment("orbital_survey_biome_mapping_high_50",  "BiomeMapping", "50",  3)
mappingExperiment("orbital_survey_biome_mapping_high_75",  "BiomeMapping", "75",  3)
mappingExperiment("orbital_survey_biome_mapping_high_100", "BiomeMapping", "100", 6)
