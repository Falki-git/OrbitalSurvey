-- Orbital Survey: celestial-body category definitions.
--
-- These values describe how Orbital Survey classifies celestial bodies and which
-- scanning altitudes apply to each category. They are stored in the mod's config
-- (the same IConfigFile the C# side reads as SWConfiguration), so that:
--   * CelestialCategoryManager can read them back at runtime, and
--   * other mods can override individual values with Config:Mod("OrbitalSurvey").
--
-- This replaces the legacy "orbital-survey-definitions" PatchManager config.

local MAXR = "orbital-survey-category-max-radius"
local LOC  = "orbital-survey-category-localization"
local ALT  = "orbital-survey-category-altitudes"

-- Entries are declared with the Config:Define(...):Type(...):Default(...):Bind() builder.
-- Only C# reads these back, so they don't need PM.InvalidatesOnChange.
local function defineInt(section, name, default, desc)
    Config:Define(section, name):Type(Integer):Default(default):Desc(desc):Bind()
end

local function defineString(section, name, default, desc)
    Config:Define(section, name):Type(String):Default(default):Desc(desc):Bind()
end

-- Maximum body radius (metres) per category. A body belongs to the smallest
-- category whose maximum radius still exceeds the body's radius.
defineInt(MAXR, "Small",  150000,    "Max body radius (m) for the Small category")
defineInt(MAXR, "Medium", 350000,    "Max body radius (m) for the Medium category")
defineInt(MAXR, "Large",  10000000,  "Max body radius (m) for the Large category")
defineInt(MAXR, "Giant",  100000000, "Max body radius (m) for the Giant category")

-- Localization keys for category display names.
defineString(LOC, "Small",  "PartModules/OrbitalSurvey/BodyCategory/Small",  "Display name loc key for Small")
defineString(LOC, "Medium", "PartModules/OrbitalSurvey/BodyCategory/Medium", "Display name loc key for Medium")
defineString(LOC, "Large",  "PartModules/OrbitalSurvey/BodyCategory/Large",  "Display name loc key for Large")
defineString(LOC, "Giant",  "PartModules/OrbitalSurvey/BodyCategory/Giant",  "Display name loc key for Giant")

-- Scanning altitudes (metres). Key form: "<Category>.<MapType>.<Min|Ideal|Max>".
-- MapType values must match the C# OrbitalSurvey.Models.MapType enum (Visual, Biome).
local function altitudes(category, mapType, min, ideal, max)
    local prefix = category .. "." .. mapType .. "."
    defineInt(ALT, prefix .. "Min",   min,   category .. " " .. mapType .. " minimum scanning altitude (m)")
    defineInt(ALT, prefix .. "Ideal", ideal, category .. " " .. mapType .. " ideal scanning altitude (m)")
    defineInt(ALT, prefix .. "Max",   max,   category .. " " .. mapType .. " maximum scanning altitude (m)")
end

altitudes("Small",  "Visual", 60000,    170000,   220000)
altitudes("Small",  "Biome",  60000,    220000,   300000)
altitudes("Medium", "Visual", 100000,   300000,   500000)
altitudes("Medium", "Biome",  300000,   500000,   700000)
altitudes("Large",  "Visual", 500000,   800000,   1100000)
altitudes("Large",  "Biome",  1000000,  1500000,  2000000)
altitudes("Giant",  "Visual", 5000000,  8000000,  11000000)
altitudes("Giant",  "Biome",  10000000, 15000000, 20000000)
