using UnityEngine;

namespace OrbitalSurvey.Managers;

public class RegionsManager
{
    private RegionsManager() { }
    public static RegionsManager Instance { get; } = new();
    
    public Dictionary<string, Dictionary<int, RegionColor>> Data = new()
    {
        {
            "Moho", new Dictionary<int, RegionColor>
            {
                { 1, new RegionColor { RegionId = "Lowlands", Color = new Color32(145, 131, 125, 255) } },
                { 0, new RegionColor { RegionId = "Highlands", Color = new Color32(150, 102, 72, 255) } },
                { 2, new RegionColor { RegionId = "Crater", Color = new Color32(20, 20, 20, 255) } },
                { 3, new RegionColor { RegionId = "TwoRingCrater", Color = new Color32(64, 62, 58, 255) } },
            }
        },
        {
            "Eve", new Dictionary<int, RegionColor>
            {
                { 0, new RegionColor { RegionId = "Foothills", Color = new Color32(190, 41, 236, 255) } },
                { 3, new RegionColor { RegionId = "ImpactSites", Color = new Color32(128, 0, 128, 255) } },
                { 4, new RegionColor { RegionId = "Olympus", Color = new Color32(102, 0, 102, 255) } },
                { 1, new RegionColor { RegionId = "Seas", Color = new Color32(255, 255, 255, 255) } },
                { 2, new RegionColor { RegionId = "Shallows", Color = new Color32(216, 150, 255, 255) } },
            }
        },
        {
            "Gilly", new Dictionary<int, RegionColor>
            {
                { 0, new RegionColor { RegionId = "Highlands", Color = new Color32(61, 56, 54, 255) } },
                { 1, new RegionColor { RegionId = "Lowlands", Color = new Color32(163, 158, 157, 255) } },
                { 3, new RegionColor { RegionId = "Midlands", Color = new Color32(250, 192, 155, 255) } },
                { 2, new RegionColor { RegionId = "ObliqueImpact", Color = new Color32(120, 82, 40, 255) } },
                { 4, new RegionColor { RegionId = "SilicateFields", Color = new Color32(255, 128, 61, 255) } },
            }
        },
        {
            "Kerbin", new Dictionary<int, RegionColor>
            {
                { 0, new RegionColor { RegionId = "Grasslands", Color = new Color32(101, 168, 102, 255) } }, 
                { 5, new RegionColor { RegionId = "Highlands", Color = new Color32(26, 82, 28, 255) } },
                { 3, new RegionColor { RegionId = "Mountains", Color = new Color32(75, 44, 7, 255) } },
                { 4, new RegionColor { RegionId = "Desert", Color = new Color32(225, 191, 146, 255) } },
                { 2, new RegionColor { RegionId = "Ice", Color = new Color32(255, 255, 255, 255) } },
                { 1, new RegionColor { RegionId = "Water", Color = new Color32(65, 117, 162, 255) } },
                { 6, new RegionColor { RegionId = "Beach", Color = new Color32(255, 233, 198, 255) } },
            }
        },
        {
            "Mun", new Dictionary<int, RegionColor>
            {
                { 4, new RegionColor { RegionId = "BiggestCrater", Color = new Color32(20, 20, 20, 255) } }, 
                { 3, new RegionColor { RegionId = "Craters", Color = new Color32(190, 190, 190, 255) } },
                { 0, new RegionColor { RegionId = "Highlands", Color = new Color32(140, 140, 140, 255) } },
                { 2, new RegionColor { RegionId = "Mare", Color = new Color32(80, 80, 80, 255) } },
                { 1, new RegionColor { RegionId = "MareEyes", Color = new Color32(255, 255, 255, 255) } },
            }
        },
        {
            "Minmus", new Dictionary<int, RegionColor>
            {
                { 3, new RegionColor { RegionId = "ArticIce", Color = new Color32(235, 235, 235, 255) } }, 
                { 2, new RegionColor { RegionId = "Craters", Color = new Color32(49, 53, 46, 255) } },
                { 1, new RegionColor { RegionId = "SheetIce", Color = new Color32(160, 250, 196, 255) } },
                { 0, new RegionColor { RegionId = "Snowdrifts", Color = new Color32(119, 138, 53, 255) } },
            }
        },
        {
            "Duna", new Dictionary<int, RegionColor>
            {
                { 5, new RegionColor { RegionId = "Highlands", Color = new Color32(100, 44, 34, 255) } }, 
                { 4, new RegionColor { RegionId = "Lowlands", Color = new Color32(187, 103, 75, 255) } },
                { 0, new RegionColor { RegionId = "Midlands", Color = new Color32(181, 56, 14, 255) } },
                { 3, new RegionColor { RegionId = "NorthPole", Color = new Color32(210, 210, 210, 255) } },
                { 2, new RegionColor { RegionId = "PolarDeserts", Color = new Color32(158, 140, 148, 255) } },
                { 1, new RegionColor { RegionId = "SouthPole", Color = new Color32(255, 255, 255, 255) } },
            }
        },
        {
            "Ike", new Dictionary<int, RegionColor>
            {
                { 3, new RegionColor { RegionId = "Caldera", Color = new Color32(96, 96, 96, 255) } },
                { 0, new RegionColor { RegionId = "LavaFields", Color = new Color32(235, 209, 209, 255) } },
                { 2, new RegionColor { RegionId = "VolcanicFields", Color = new Color32(79, 40, 40, 255) } },
                { 1, new RegionColor { RegionId = "Volcanoes", Color = new Color32(28, 0, 0, 255) } },
            }
        },
        {
            "Dres", new Dictionary<int, RegionColor>
            {
                { 1, new RegionColor { RegionId = "Lowlands", Color = new Color32(180, 168, 146, 255) } },
                { 0, new RegionColor { RegionId = "Highlands", Color = new Color32(161, 139, 98, 255) } },
                { 2, new RegionColor { RegionId = "EquatorialRidge", Color = new Color32(74, 66, 50, 255) } },
                { 4, new RegionColor { RegionId = "Craters", Color = new Color32(80, 80, 80, 255) } },
                { 3, new RegionColor { RegionId = "EyeOfDres", Color = new Color32(0, 0, 0, 255) } },
            }
        },
        {
            "Jool", new Dictionary<int, RegionColor>
            {
                { 4, new RegionColor { RegionId = "NorthernCircle", Color = new Color32(193, 162, 148, 255) } },
                { 0, new RegionColor { RegionId = "NorthernTropic", Color = new Color32(191, 103, 57, 255) } },
                { 3, new RegionColor { RegionId = "Equator", Color = new Color32(255, 255, 255, 255) } },
                { 2, new RegionColor { RegionId = "SouthernTropic", Color = new Color32(74, 41, 28, 255) } },
                { 1, new RegionColor { RegionId = "SouthernCircle", Color = new Color32(82, 76, 73, 255) } },
            }
        },
        {
            "Laythe", new Dictionary<int, RegionColor>
            {
                { 6, new RegionColor { RegionId = "HeartLake", Color = new Color32(166, 109, 252, 255) } },
                { 2, new RegionColor { RegionId = "Hills", Color = new Color32(173, 144, 121, 255) } },
                { 3, new RegionColor { RegionId = "Lakes", Color = new Color32(0, 182, 214, 255) } },
                { 0, new RegionColor { RegionId = "Ocean", Color = new Color32(14, 08, 64, 255) } },
                { 1, new RegionColor { RegionId = "Poles", Color = new Color32(200, 200, 200, 255) } },
                { 4, new RegionColor { RegionId = "Shallows", Color = new Color32(90, 100, 200, 255) } },
                { 5, new RegionColor { RegionId = "Shores", Color = new Color32(159, 188, 204, 255) } },
                { 7, new RegionColor { RegionId = "BullseyeLake", Color = new Color32(78, 14, 173, 255) } },
            }
        },
        {
            "Vall", new Dictionary<int, RegionColor>
            {
                { 1, new RegionColor { RegionId = "Highlands", Color = new Color32(119, 138, 53, 255) } },
                { 0, new RegionColor { RegionId = "Lowlands", Color = new Color32(189, 226, 196, 255) } },
                { 2, new RegionColor { RegionId = "Mountains", Color = new Color32(100, 100, 100, 255) } },
                { 3, new RegionColor { RegionId = "Well", Color = new Color32(255, 0, 0, 255) } },
            }
        },
        {
            "Tylo", new Dictionary<int, RegionColor>
            {
                { 3, new RegionColor { RegionId = "Craters", Color = new Color32(63, 51, 45, 255) } },
                { 2, new RegionColor { RegionId = "Dimple", Color = new Color32(156, 156, 112, 255) } },
                { 1, new RegionColor { RegionId = "Highlands", Color = new Color32(173, 145, 116, 255) } },
                { 0, new RegionColor { RegionId = "Lowlands", Color = new Color32(110, 106, 102, 255) } },
            }
        },
        {
            "Bop", new Dictionary<int, RegionColor>
            {
                { 2, new RegionColor { RegionId = "Craters", Color = new Color32(84, 68, 56, 255) } },
                { 0, new RegionColor { RegionId = "Highlands", Color = new Color32(153, 128, 113, 255) } },
                { 1, new RegionColor { RegionId = "LargestImpactSite", Color = new Color32(0, 0, 0, 255) } },
            }
        },
        {
            "Pol", new Dictionary<int, RegionColor>
            {
                { 0, new RegionColor { RegionId = "Highlands", Color = new Color32(59, 58, 4, 255) } },
                { 3, new RegionColor { RegionId = "Lowlands", Color = new Color32(222, 205, 35, 255) } },
                { 1, new RegionColor { RegionId = "Mountains", Color = new Color32(110, 84, 13, 255) } },
                { 4, new RegionColor { RegionId = "Volcanoes", Color = new Color32(79, 17, 17, 255) } },
                { 2, new RegionColor { RegionId = "Midlands", Color = new Color32(240, 167, 50, 255) } },
            }
        },
        {
            "Eeloo", new Dictionary<int, RegionColor>
            {
                { 2, new RegionColor { RegionId = "Canyons", Color = new Color32(51, 47, 38, 255) } },
                { 3, new RegionColor { RegionId = "Craters", Color = new Color32(105, 100, 100, 255) } },
                { 1, new RegionColor { RegionId = "GreatRift", Color = new Color32(176, 181, 181, 255) } },
                { 0, new RegionColor { RegionId = "IceFields", Color = new Color32(116, 153, 124, 255) } },
            }
        },
    };
    
    public struct RegionColor
    {
        public string RegionId;
        public Color32 Color;
    }
}