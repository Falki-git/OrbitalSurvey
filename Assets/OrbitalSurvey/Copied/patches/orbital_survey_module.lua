---@type OrbitalSurvey.Constants
local C = require('constants')

local function addOrbitalSurveyModule(partName, mode, fov, ecRate)
    PM.Parts:Patch("AddOrbitalSurveyModule_" .. partName):Named(partName):Do(function(part)
        part:AddModule("Module_OrbitalSurvey", function(module)
            module:AddData("Data_OrbitalSurvey", function(data)
                data.ModeValue = mode
                data.ScanningFieldOfViewValue = fov
                data.RequiredResource = {
                    Rate = ecRate,
                    ResourceName = "ElectricCharge",
                    AcceptanceThreshold = 0.1
                }
            end)
        end)
        part.PAMModuleVisualsOverride:Append({
            PartComponentModuleName = "PartComponentModule_OrbitalSurvey",
            ModuleDisplayName = "PartModules/OrbitalSurvey/Name",
            ShowHeader = true,
            ShowFooter = true
        })
    end)
end

local VISUAL = "PartModules/OrbitalSurvey/Mode/Visual"
local BIOME  = "PartModules/OrbitalSurvey/Mode/Biome"

addOrbitalSurveyModule("antenna_0v_dish_ra-2",        VISUAL, C.VISUAL_EARLY_FIELD_OF_VIEW, C.VISUAL_EARLY_EC_CONSUMPTION)
addOrbitalSurveyModule("antenna_1v_parabolic_dts-m1", BIOME,  C.BIOME_EARLY_FIELD_OF_VIEW,  C.BIOME_EARLY_EC_CONSUMPTION)
addOrbitalSurveyModule("antenna_0v_dish_ra-15",       VISUAL, C.VISUAL_MID_FIELD_OF_VIEW,   C.VISUAL_MID_EC_CONSUMPTION)
addOrbitalSurveyModule("antenna_1v_dish_hg55",        BIOME,  C.BIOME_MID_FIELD_OF_VIEW,    C.BIOME_MID_EC_CONSUMPTION)
addOrbitalSurveyModule("antenna_1v_dish_ra-100",      VISUAL, C.VISUAL_LATE_FIELD_OF_VIEW,  C.VISUAL_LATE_EC_CONSUMPTION)
addOrbitalSurveyModule("antenna_1v_dish_88-88",       BIOME,  C.BIOME_LATE_FIELD_OF_VIEW,   C.BIOME_LATE_EC_CONSUMPTION)
