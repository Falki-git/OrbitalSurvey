local function makeExperiment(id)
    return {
        ExperimentDefinitionID = id,
        ContextualExperiment = false,
        ResourcesCost = {},
        DenyInterruption = true,
        CFXName = "fx_science",
        SFXEvent = "Invalid"
    }
end

local defaultSituation = {
    CelestialBodyScalar = 0.0,
    ScienceRegionScalar = 0.0,
    SituationScalar = 0.0,
    ResearchLocation = nil
}

PM.Parts:Patch("AddVisualScienceExperiments")
    :Named("antenna_0v_dish_ra-2", "antenna_0v_dish_ra-15", "antenna_1v_dish_ra-100")
    :Do(function(part)
        part:AddModule("Module_ScienceExperiment", function(module)
            module:AddData("Data_ScienceExperiment", function(data)
                data.ExperimentsStandings = {}
                data.LastKnownValidSituation = defaultSituation
                data.Experiments = {
                    makeExperiment("orbital_survey_visual_mapping_high_25"),
                    makeExperiment("orbital_survey_visual_mapping_high_50"),
                    makeExperiment("orbital_survey_visual_mapping_high_75"),
                    makeExperiment("orbital_survey_visual_mapping_high_100")
                }
                data.NotifyOnCompletion = false
                data.ResourceThresholdMultiplier = 0.25
            end)
        end)
    end)

PM.Parts:Patch("AddBiomeScienceExperiments")
    :Named("antenna_1v_parabolic_dts-m1", "antenna_1v_dish_hg55", "antenna_1v_dish_88-88")
    :Do(function(part)
        part:AddModule("Module_ScienceExperiment", function(module)
            module:AddData("Data_ScienceExperiment", function(data)
                data.ExperimentsStandings = {}
                data.LastKnownValidSituation = defaultSituation
                data.Experiments = {
                    makeExperiment("orbital_survey_biome_mapping_high_25"),
                    makeExperiment("orbital_survey_biome_mapping_high_50"),
                    makeExperiment("orbital_survey_biome_mapping_high_75"),
                    makeExperiment("orbital_survey_biome_mapping_high_100")
                }
                data.NotifyOnCompletion = false
                data.ResourceThresholdMultiplier = 0.25
            end)
        end)
    end)
