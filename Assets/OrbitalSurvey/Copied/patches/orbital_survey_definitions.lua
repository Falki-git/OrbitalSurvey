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
--
-- Micro and Tiny exist for the asteroid-sized bodies orbiting Dres, whose spheres of
-- influence are far too small for the Small band: Drast (r = 1596 m) has a *forced*
-- SOI of 4000 m, leaving only 2404 m of usable altitude, and Beyl (r = 4595 m) has a
-- computed SOI of ~230 km but roughly 13.4 km of terrain relief to clear. The caps are
-- picked so no stock body moves category - Gilly, at 13000 m, is the next smallest.
defineInt(MAXR, "Micro",  2500,      "Max body radius (m) for the Micro category")
defineInt(MAXR, "Tiny",   10000,     "Max body radius (m) for the Tiny category")
defineInt(MAXR, "Small",  150000,    "Max body radius (m) for the Small category")
defineInt(MAXR, "Medium", 350000,    "Max body radius (m) for the Medium category")
defineInt(MAXR, "Large",  10000000,  "Max body radius (m) for the Large category")
defineInt(MAXR, "Giant",  100000000, "Max body radius (m) for the Giant category")

-- Localization keys for category display names.
defineString(LOC, "Micro",  "PartModules/OrbitalSurvey/BodyCategory/Micro",  "Display name loc key for Micro")
defineString(LOC, "Tiny",   "PartModules/OrbitalSurvey/BodyCategory/Tiny",   "Display name loc key for Tiny")
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

-- Micro (Drast): the whole band has to fit under the 2404 m SOI ceiling, so Biome's
-- maximum sits 24 m below it. Ideal altitudes land at h/r ~ 1.2-1.3, matching the ratio
-- the larger categories already use (Kerbin 1.33, Mun 1.50, Dres 1.23).
altitudes("Micro",  "Visual", 1000,     2000,     3000)
altitudes("Micro",  "Biome",  1000,     2000,     3000)

-- Tiny (Beyl): scan width depends only on the altitude-to-radius ratio, so these are Kerbin's
-- ratios (0.83 / 1.33 / 1.83 visual, 1.67 / 2.50 / 3.33 biome) applied to Beyl's 4595 m radius.
-- That puts the swath at 9.5 px visual and 10.7 px biome - identical to Kerbin. The earlier
-- 15-80 km band sat at h/r 5-9, covering ~4x the surface per tick and bogging the game down.
altitudes("Tiny",   "Visual", 4000,     6000,     8000)
altitudes("Tiny",   "Biome",  8000,     12000,    16000)

altitudes("Small",  "Visual", 60000,    170000,   220000)
altitudes("Small",  "Biome",  60000,    220000,   300000)
altitudes("Medium", "Visual", 100000,   300000,   500000)
altitudes("Medium", "Biome",  300000,   500000,   700000)
altitudes("Large",  "Visual", 500000,   800000,   1100000)
altitudes("Large",  "Biome",  1000000,  1500000,  2000000)
altitudes("Giant",  "Visual", 5000000,  8000000,  11000000)
altitudes("Giant",  "Biome",  10000000, 15000000, 20000000)
