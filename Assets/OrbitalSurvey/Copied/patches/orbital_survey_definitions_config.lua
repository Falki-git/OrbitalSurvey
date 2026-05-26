-- Declares configurable definitions for celestial body categories.
-- Values are read by CelestialCategoryManager.cs at runtime via SWConfiguration.
-- Users can edit these in the mod config file. Other mods can override them by
-- shipping a patch that calls Config:Integer / Config:String with the same keys.

local section = "orbital-survey-definitions"

-- Maximum radius (in meters) that classifies a body into each category
Config:Integer(section, "celestial-category__maximum-radius__Small",  150000)
Config:Integer(section, "celestial-category__maximum-radius__Medium", 350000)
Config:Integer(section, "celestial-category__maximum-radius__Large",  10000000)
Config:Integer(section, "celestial-category__maximum-radius__Giant",  100000000)

-- Localization tag for each category's display name
Config:String(section, "celestial-category__localization-string__Small",  "PartModules/OrbitalSurvey/BodyCategory/Small")
Config:String(section, "celestial-category__localization-string__Medium", "PartModules/OrbitalSurvey/BodyCategory/Medium")
Config:String(section, "celestial-category__localization-string__Large",  "PartModules/OrbitalSurvey/BodyCategory/Large")
Config:String(section, "celestial-category__localization-string__Giant",  "PartModules/OrbitalSurvey/BodyCategory/Giant")

-- Scanning altitude ranges (in meters) per category and map type
Config:Integer(section, "celestial-category__scanning-altitudes__Small__Visual__MinAltitude",   60000)
Config:Integer(section, "celestial-category__scanning-altitudes__Small__Visual__IdealAltitude", 170000)
Config:Integer(section, "celestial-category__scanning-altitudes__Small__Visual__MaxAltitude",   220000)
Config:Integer(section, "celestial-category__scanning-altitudes__Small__Biome__MinAltitude",    60000)
Config:Integer(section, "celestial-category__scanning-altitudes__Small__Biome__IdealAltitude",  220000)
Config:Integer(section, "celestial-category__scanning-altitudes__Small__Biome__MaxAltitude",    300000)

Config:Integer(section, "celestial-category__scanning-altitudes__Medium__Visual__MinAltitude",   100000)
Config:Integer(section, "celestial-category__scanning-altitudes__Medium__Visual__IdealAltitude", 300000)
Config:Integer(section, "celestial-category__scanning-altitudes__Medium__Visual__MaxAltitude",   500000)
Config:Integer(section, "celestial-category__scanning-altitudes__Medium__Biome__MinAltitude",    300000)
Config:Integer(section, "celestial-category__scanning-altitudes__Medium__Biome__IdealAltitude",  500000)
Config:Integer(section, "celestial-category__scanning-altitudes__Medium__Biome__MaxAltitude",    700000)

Config:Integer(section, "celestial-category__scanning-altitudes__Large__Visual__MinAltitude",   500000)
Config:Integer(section, "celestial-category__scanning-altitudes__Large__Visual__IdealAltitude", 800000)
Config:Integer(section, "celestial-category__scanning-altitudes__Large__Visual__MaxAltitude",   1100000)
Config:Integer(section, "celestial-category__scanning-altitudes__Large__Biome__MinAltitude",    1000000)
Config:Integer(section, "celestial-category__scanning-altitudes__Large__Biome__IdealAltitude",  1500000)
Config:Integer(section, "celestial-category__scanning-altitudes__Large__Biome__MaxAltitude",    2000000)

Config:Integer(section, "celestial-category__scanning-altitudes__Giant__Visual__MinAltitude",   5000000)
Config:Integer(section, "celestial-category__scanning-altitudes__Giant__Visual__IdealAltitude", 8000000)
Config:Integer(section, "celestial-category__scanning-altitudes__Giant__Visual__MaxAltitude",   11000000)
Config:Integer(section, "celestial-category__scanning-altitudes__Giant__Biome__MinAltitude",    10000000)
Config:Integer(section, "celestial-category__scanning-altitudes__Giant__Biome__IdealAltitude",  15000000)
Config:Integer(section, "celestial-category__scanning-altitudes__Giant__Biome__MaxAltitude",    20000000)
