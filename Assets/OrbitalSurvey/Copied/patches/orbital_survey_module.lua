-- Orbital Survey: add the scanning module to antenna parts.
--
-- Adds Module_OrbitalSurvey (+ Data_OrbitalSurvey) to the relevant stock antennas
-- and makes the module visible in the Part Action Menu.
--
-- The scanning field-of-view (degrees) and EC consumption (units/second) are read
-- from config so other mods can override them (section "orbital-survey-constants").
-- This replaces the legacy orbital_survey_module.patch.

local C = "orbital-survey-constants"

-- These entries are read by the patches below, so they carry PM.InvalidatesOnChange:
-- changing one makes PatchManager rebuild instead of serving a stale cached patch.
local function fov(name, default, desc)
    return Config:Define(C, name):Type(Integer):Default(default):Desc(desc)
        :Tag(PM.InvalidatesOnChange):Bind().value
end

local function ec(name, default, desc)
    return Config:Define(C, name):Type(Double):Default(default):Desc(desc)
        :Tag(PM.InvalidatesOnChange):Bind().value
end

-- Field of view (degrees) and EC consumption (units/s) per mode and tier.
local visualEarlyFov = fov("visual-early-field-of-view", 5, "Visual scanner (early) field of view (deg)")
local visualEarlyEc  = ec("visual-early-ec-consumption", 1.0, "Visual scanner (early) EC consumption (units/s)")
local biomeEarlyFov  = fov("biome-early-field-of-view", 3, "Biome scanner (early) field of view (deg)")
local biomeEarlyEc   = ec("biome-early-ec-consumption", 2.0, "Biome scanner (early) EC consumption (units/s)")

local visualMidFov = fov("visual-mid-field-of-view", 6, "Visual scanner (mid) field of view (deg)")
local visualMidEc  = ec("visual-mid-ec-consumption", 2.0, "Visual scanner (mid) EC consumption (units/s)")
local biomeMidFov  = fov("biome-mid-field-of-view", 4, "Biome scanner (mid) field of view (deg)")
local biomeMidEc   = ec("biome-mid-ec-consumption", 3.0, "Biome scanner (mid) EC consumption (units/s)")

local visualLateFov = fov("visual-late-field-of-view", 7, "Visual scanner (late) field of view (deg)")
local visualLateEc  = ec("visual-late-ec-consumption", 3.0, "Visual scanner (late) EC consumption (units/s)")
local biomeLateFov  = fov("biome-late-field-of-view", 5, "Biome scanner (late) field of view (deg)")
local biomeLateEc   = ec("biome-late-ec-consumption", 4.0, "Biome scanner (late) EC consumption (units/s)")

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
