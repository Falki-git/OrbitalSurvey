-- Orbital Survey: attach mapping experiments to antenna parts.
--
-- Adds Module_ScienceExperiment (+ Data_ScienceExperiment) to the antennas, listing
-- the milestone experiments each one performs. Visual antennas get the visual
-- mapping experiments; biome antennas get the biome mapping experiments.
--
-- This replaces the legacy orbital_survey_add_experiments_to_parts.patch.

local function addExperiments(patchName, partIds, experimentIds)
    local patch = PM.Parts:Patch(patchName)
    for _, partId in ipairs(partIds) do
        patch:Named(partId)
    end

    patch:Do(function(part)
        part:AddModule("Module_ScienceExperiment", function(module)
            module:AddData("Data_ScienceExperiment", function(data)
                -- ExperimentStandings is intentionally not set: the game initialises it
                -- to an empty list.
                data.LastKnownValidSituation = {
                    CelestialBodyScalar = 0.0,
                    ScienceRegionScalar = 0.0,
                    SituationScalar = 0.0,
                    -- ResearchLocation left unset (null).
                }

                -- Each entry's ResourcesCost must be a non-null (empty) array, otherwise
                -- the game dereferences it without a null check. A single-element
                -- placeholder makes the field a JSON array; it is emptied right after.
                local experiments = {}
                for _, experimentId in ipairs(experimentIds) do
                    experiments[#experiments + 1] = {
                        ExperimentDefinitionID = experimentId,
                        ContextualExperiment = false,
                        DenyInterruption = true,
                        CFXName = "fx_science",
                        SFXEvent = "Invalid",
                        ResourcesCost = { false },
                    }
                end
                data.Experiments = experiments

                for _, experiment in ipairs(data.Experiments) do
                    experiment.ResourcesCost:Clear()
                end

                data.NotifyOnCompletion = false
                data.ResourceThresholdMultiplier = 0.25
            end)
        end)
    end)
end

addExperiments(
    "OrbitalSurveyVisualExperiments",
    { "antenna_0v_dish_ra-2", "antenna_0v_dish_ra-15", "antenna_1v_dish_ra-100" },
    {
        "orbital_survey_visual_mapping_high_25",
        "orbital_survey_visual_mapping_high_50",
        "orbital_survey_visual_mapping_high_75",
        "orbital_survey_visual_mapping_high_100",
    }
)

addExperiments(
    "OrbitalSurveyBiomeExperiments",
    { "antenna_1v_parabolic_dts-m1", "antenna_1v_dish_hg55", "antenna_1v_dish_88-88" },
    {
        "orbital_survey_biome_mapping_high_25",
        "orbital_survey_biome_mapping_high_50",
        "orbital_survey_biome_mapping_high_75",
        "orbital_survey_biome_mapping_high_100",
    }
)
