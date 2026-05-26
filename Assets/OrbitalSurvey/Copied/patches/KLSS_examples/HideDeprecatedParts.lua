--[[
    Likely not necessary, but adding it here for a reason of showing how to add assets
]]

local deprecated = {
    "KLSS_food_tank_2v_1x2",
    "KLSS_water_tank_2v_1x2",
    "KLSS_oxygen_tank_2v_1x2",
    "KLSS_food_pack_0v_radial",
    "KLSS_water_tank_0v_radial",
    "KLSS_oxygen_tank_0v_radial",
    "KLSS_food_pack_2v_radial",
    "KLSS_water_tank_2v_radial",
    "KLSS_oxygen_tank_2v_radial"
}

for i,v in ipairs(deprecated) do
    PM.JSON:New("spring_cleaning", v .. "_cleanup", {
        PartId = v,
        Hidden = true,
        Toggleable = false
    })
end