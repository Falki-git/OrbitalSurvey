local function newExperiment(id, mappingType, percentage, dataValue)
    local typeName = mappingType .. "Mapping"
    PM.Science:NewExperiment(id, function(exp)
        exp.DisplayName = "OrbitalSurvey/Experiments/OabDescription/DisplayName/" .. typeName
        exp.DisplayRequirements = "OrbitalSurvey/Experiments/OabDescription/DisplayRequirements"
        exp.ExperimentType = "DataType"
        exp.DataReportDisplayName = "OrbitalSurvey/Experiments/DataReportDisplayName/" .. typeName .. "/" .. percentage
        exp.DataFlavorDescriptions = {
            {
                ResearchLocationID = "Default",
                LocalizationTag = "OrbitalSurvey/Experiments/DataFlavor/" .. typeName .. "/Default/" .. percentage
            }
        }
        exp.ValidLocations = {}
        exp.DataValue = dataValue
        exp.TransmissionSize = 100
    end)
end

-- VISUAL MAPPING
newExperiment("orbital_survey_visual_mapping_high_25",  "Visual", "25",  3)
newExperiment("orbital_survey_visual_mapping_high_50",  "Visual", "50",  3)
newExperiment("orbital_survey_visual_mapping_high_75",  "Visual", "75",  3)
newExperiment("orbital_survey_visual_mapping_high_100", "Visual", "100", 6)

-- BIOME MAPPING
newExperiment("orbital_survey_biome_mapping_high_25",  "Biome", "25",  3)
newExperiment("orbital_survey_biome_mapping_high_50",  "Biome", "50",  3)
newExperiment("orbital_survey_biome_mapping_high_75",  "Biome", "75",  3)
newExperiment("orbital_survey_biome_mapping_high_100", "Biome", "100", 6)
