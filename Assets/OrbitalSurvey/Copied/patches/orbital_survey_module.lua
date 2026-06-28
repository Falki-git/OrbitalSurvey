-- Orbital Survey: add the scanning module to antenna parts.
--
-- Adds Module_OrbitalSurvey (+ Data_OrbitalSurvey) to the relevant stock antennas
-- and makes the module visible in the Part Action Menu.
--
-- The scanning field-of-view (degrees) and EC consumption (units/second) are read
-- from config so other mods can override them (section "orbital-survey-constants").
-- This replaces the legacy orbital_survey_module.patch.

local C = "orbital-survey-constants"

-- Field of view (degrees) and EC consumption (units/s) per mode and tier.
local visualEarlyFov = Config:Integer(C, "visual-early-field-of-view", 5)
local visualEarlyEc  = Config:Float(C,   "visual-early-ec-consumption", 1.0)
local biomeEarlyFov  = Config:Integer(C, "biome-early-field-of-view", 3)
local biomeEarlyEc   = Config:Float(C,   "biome-early-ec-consumption", 2.0)

local visualMidFov = Config:Integer(C, "visual-mid-field-of-view", 6)
local visualMidEc  = Config:Float(C,   "visual-mid-ec-consumption", 2.0)
local biomeMidFov  = Config:Integer(C, "biome-mid-field-of-view", 4)
local biomeMidEc   = Config:Float(C,   "biome-mid-ec-consumption", 3.0)

local visualLateFov = Config:Integer(C, "visual-late-field-of-view", 7)
local visualLateEc  = Config:Float(C,   "visual-late-ec-consumption", 3.0)
local biomeLateFov  = Config:Integer(C, "biome-late-field-of-view", 5)
local biomeLateEc   = Config:Float(C,   "biome-late-ec-consumption", 4.0)

local VISUAL = "PartModules/OrbitalSurvey/Mode/Visual"
local BIOME  = "PartModules/OrbitalSurvey/Mode/Biome"

-- Antenna part id -> scanning configuration.
local parts = {
    { id = "antenna_0v_dish_ra-2",        mode = VISUAL, fov = visualEarlyFov, ec = visualEarlyEc },
    { id = "antenna_1v_parabolic_dts-m1", mode = BIOME,  fov = biomeEarlyFov,  ec = biomeEarlyEc },
    { id = "antenna_0v_dish_ra-15",       mode = VISUAL, fov = visualMidFov,   ec = visualMidEc },
    { id = "antenna_1v_dish_hg55",        mode = BIOME,  fov = biomeMidFov,    ec = biomeMidEc },
    { id = "antenna_1v_dish_ra-100",      mode = VISUAL, fov = visualLateFov,  ec = visualLateEc },
    { id = "antenna_1v_dish_88-88",       mode = BIOME,  fov = biomeLateFov,   ec = biomeLateEc },
}

-- A fresh Part Action Menu visuals-override entry (built per use to avoid sharing
-- a single JSON-backed table across parts).
local function makeVisualsOverride()
    return {
        PartComponentModuleName = "PartComponentModule_OrbitalSurvey",
        ModuleDisplayName = "PartModules/OrbitalSurvey/Name",
        ShowHeader = true,
        ShowFooter = true,
    }
end

for _, p in ipairs(parts) do
    PM.Parts:Patch("OrbitalSurveyModule_" .. p.id)
            :Named(p.id)
            :Do(function(part)
                part:AddModule("Module_OrbitalSurvey", function(module)
                    module:AddData("Data_OrbitalSurvey", function(data)
                        data.ModeValue = p.mode
                        data.ScanningFieldOfViewValue = p.fov
                        data.RequiredResource = {
                            Rate = p.ec,
                            ResourceName = "ElectricCharge",
                            AcceptanceThreshold = 0.1,
                        }
                    end)
                end)

                -- Show the module in the Part Action Menu. The field may not exist
                -- on a stock part, so create it as an array when absent.
                if part.PAMModuleVisualsOverride ~= nil then
                    part.PAMModuleVisualsOverride:Append(makeVisualsOverride())
                else
                    part.PAMModuleVisualsOverride = { makeVisualsOverride() }
                end
            end)
end
