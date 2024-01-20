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
                { 1, new RegionColor { RegionId = "MohoLowlands", Color = new Color32(145, 131, 125, 255) } },
                { 0, new RegionColor { RegionId = "MohoHighlands", Color = new Color32(150, 102, 72, 255) } },
                { 2, new RegionColor { RegionId = "MohoCrater", Color = new Color32(20, 20, 20, 255) } },
                { 3, new RegionColor { RegionId = "MohoTwoRingCrater", Color = new Color32(64, 62, 58, 255) } },
            }
        },
        {
            "Eve", new Dictionary<int, RegionColor>
            {
                { 0, new RegionColor { RegionId = "EveFoothills", Color = new Color32(190, 41, 236, 255) } },
                { 3, new RegionColor { RegionId = "EveImpactSites", Color = new Color32(128, 0, 128, 255) } },
                { 4, new RegionColor { RegionId = "EveOlympus", Color = new Color32(0, 0, 0, 255) } },
                { 1, new RegionColor { RegionId = "EveSeas", Color = new Color32(255, 255, 255, 255) } },
                { 2, new RegionColor { RegionId = "EveShallows", Color = new Color32(216, 150, 255, 255) } },
            }
        },
        {
            "Gilly", new Dictionary<int, RegionColor>
            {
                { 0, new RegionColor { RegionId = "GillyHighlands", Color = new Color32(61, 56, 54, 255) } },
                { 1, new RegionColor { RegionId = "GillyLowlands", Color = new Color32(163, 158, 157, 255) } },
                { 3, new RegionColor { RegionId = "GillyMidlands", Color = new Color32(250, 192, 155, 255) } },
                { 2, new RegionColor { RegionId = "GillyObliqueImpact", Color = new Color32(120, 82, 40, 255) } },
                { 4, new RegionColor { RegionId = "GillySilicateFields", Color = new Color32(255, 128, 61, 255) } },
            }
        },
        {
            "Kerbin", new Dictionary<int, RegionColor>
            {
                { 0, new RegionColor { RegionId = "KerbinGrasslands", Color = new Color32(101, 168, 102, 255) } }, 
                { 5, new RegionColor { RegionId = "KerbinHighlands", Color = new Color32(26, 82, 28, 255) } },
                { 3, new RegionColor { RegionId = "KerbinMountains", Color = new Color32(75, 44, 7, 255) } },
                { 4, new RegionColor { RegionId = "KerbinDesert", Color = new Color32(225, 191, 146, 255) } },
                { 2, new RegionColor { RegionId = "KerbinIce", Color = new Color32(255, 255, 255, 255) } },
                { 1, new RegionColor { RegionId = "KerbinWater", Color = new Color32(65, 117, 162, 255) } },
                { 6, new RegionColor { RegionId = "KerbinBeach", Color = new Color32(255, 233, 198, 255) } },
            }
        },
        {
            "Mun", new Dictionary<int, RegionColor>
            {
                { 4, new RegionColor { RegionId = "MunBiggestCrater", Color = new Color32(20, 20, 20, 255) } }, 
                { 3, new RegionColor { RegionId = "MunCraters", Color = new Color32(190, 190, 190, 255) } },
                { 0, new RegionColor { RegionId = "MunHighlands", Color = new Color32(140, 140, 140, 255) } },
                { 2, new RegionColor { RegionId = "MunMare", Color = new Color32(80, 80, 80, 255) } },
                { 1, new RegionColor { RegionId = "MunMareEyes", Color = new Color32(255, 255, 255, 255) } },
            }
        },
        {
            "Minmus", new Dictionary<int, RegionColor>
            {
                { 3, new RegionColor { RegionId = "MinmusArcticIce", Color = new Color32(235, 235, 235, 255) } }, 
                { 2, new RegionColor { RegionId = "MinmusCraters", Color = new Color32(49, 53, 46, 255) } },
                { 1, new RegionColor { RegionId = "MinmusSheetIce", Color = new Color32(160, 250, 196, 255) } },
                { 0, new RegionColor { RegionId = "MinmusSnowdrifts", Color = new Color32(119, 138, 53, 255) } },
            }
        },
        {
            "Duna", new Dictionary<int, RegionColor>
            {
                { 5, new RegionColor { RegionId = "DunaHighlands", Color = new Color32(100, 44, 34, 255) } }, 
                { 4, new RegionColor { RegionId = "DunaLowlands", Color = new Color32(187, 103, 75, 255) } },
                { 0, new RegionColor { RegionId = "DunaMidlands", Color = new Color32(181, 56, 14, 255) } },
                { 3, new RegionColor { RegionId = "DunaNorthPole", Color = new Color32(210, 210, 210, 255) } },
                { 2, new RegionColor { RegionId = "DunaPolarDeserts", Color = new Color32(158, 140, 148, 255) } },
                { 1, new RegionColor { RegionId = "DunaSouthPole", Color = new Color32(255, 255, 255, 255) } },
            }
        },
        {
            "Ike", new Dictionary<int, RegionColor>
            {
                { 3, new RegionColor { RegionId = "IkeCaldera", Color = new Color32(96, 96, 96, 255) } },
                { 0, new RegionColor { RegionId = "IkeLavaFields", Color = new Color32(235, 209, 209, 255) } },
                { 2, new RegionColor { RegionId = "IkeVolcanicFields", Color = new Color32(79, 40, 40, 255) } },
                { 1, new RegionColor { RegionId = "IkeVolcanoes", Color = new Color32(28, 0, 0, 255) } },
            }
        },
        {
            "Dres", new Dictionary<int, RegionColor>
            {
                { 1, new RegionColor { RegionId = "DresLowlands", Color = new Color32(180, 168, 146, 255) } },
                { 0, new RegionColor { RegionId = "DresHighlands", Color = new Color32(161, 139, 98, 255) } },
                { 2, new RegionColor { RegionId = "DresEquatorialRidge", Color = new Color32(74, 66, 50, 255) } },
                { 4, new RegionColor { RegionId = "DresCraters", Color = new Color32(80, 80, 80, 255) } },
                { 3, new RegionColor { RegionId = "DresEyeOfDres", Color = new Color32(0, 0, 0, 255) } },
            }
        },
        {
            "Jool", new Dictionary<int, RegionColor>
            {
                { 4, new RegionColor { RegionId = "JoolNorthernCircle", Color = new Color32(193, 162, 148, 255) } },
                { 0, new RegionColor { RegionId = "JoolNorthernTropic", Color = new Color32(191, 103, 57, 255) } },
                { 3, new RegionColor { RegionId = "JoolEquator", Color = new Color32(255, 255, 255, 255) } },
                { 2, new RegionColor { RegionId = "JoolSouthernTropic", Color = new Color32(74, 41, 28, 255) } },
                { 1, new RegionColor { RegionId = "JoolSouthernCircle", Color = new Color32(82, 76, 73, 255) } },
            }
        },
        {
            "Laythe", new Dictionary<int, RegionColor>
            {
                { 6, new RegionColor { RegionId = "LaytheHeartLake", Color = new Color32(166, 109, 252, 255) } },
                { 2, new RegionColor { RegionId = "LaytheHills", Color = new Color32(173, 144, 121, 255) } },
                { 3, new RegionColor { RegionId = "LaytheLakes", Color = new Color32(0, 182, 214, 255) } },
                { 0, new RegionColor { RegionId = "LaytheOcean", Color = new Color32(14, 08, 64, 255) } },
                { 1, new RegionColor { RegionId = "LaythePoles", Color = new Color32(200, 200, 200, 255) } },
                { 4, new RegionColor { RegionId = "LaytheShallows", Color = new Color32(90, 100, 200, 255) } },
                { 5, new RegionColor { RegionId = "LaytheShores", Color = new Color32(159, 188, 204, 255) } },
                { 7, new RegionColor { RegionId = "LaytheBullseyeLake", Color = new Color32(78, 14, 173, 255) } },
            }
        },
        {
            "Vall", new Dictionary<int, RegionColor>
            {
                { 1, new RegionColor { RegionId = "VallHighlands", Color = new Color32(119, 138, 53, 255) } },
                { 0, new RegionColor { RegionId = "VallLowlands", Color = new Color32(189, 226, 196, 255) } },
                { 2, new RegionColor { RegionId = "VallMountains", Color = new Color32(100, 100, 100, 255) } },
                { 3, new RegionColor { RegionId = "VallWell", Color = new Color32(255, 0, 0, 255) } },
            }
        },
        {
            "Tylo", new Dictionary<int, RegionColor>
            {
                { 3, new RegionColor { RegionId = "TyloCraters", Color = new Color32(63, 51, 45, 255) } },
                { 2, new RegionColor { RegionId = "TyloDimple", Color = new Color32(156, 156, 112, 255) } },
                { 1, new RegionColor { RegionId = "TyloHighlands", Color = new Color32(173, 145, 116, 255) } },
                { 0, new RegionColor { RegionId = "TyloLowlands", Color = new Color32(110, 106, 102, 255) } },
            }
        },
        {
            "Bop", new Dictionary<int, RegionColor>
            {
                { 2, new RegionColor { RegionId = "BopCraters", Color = new Color32(84, 68, 56, 255) } },
                { 0, new RegionColor { RegionId = "BopHighlands", Color = new Color32(153, 128, 113, 255) } },
                { 1, new RegionColor { RegionId = "BopLargestImpactSite", Color = new Color32(0, 0, 0, 255) } },
            }
        },
        {
            "Pol", new Dictionary<int, RegionColor>
            {
                { 0, new RegionColor { RegionId = "PolHighlands", Color = new Color32(59, 58, 4, 255) } },
                { 3, new RegionColor { RegionId = "PolLowlands", Color = new Color32(222, 205, 35, 255) } },
                { 1, new RegionColor { RegionId = "PolMountains", Color = new Color32(110, 84, 13, 255) } },
                { 4, new RegionColor { RegionId = "PolVolcanoes", Color = new Color32(79, 17, 17, 255) } },
                { 2, new RegionColor { RegionId = "PolMidlands", Color = new Color32(240, 167, 50, 255) } },
            }
        },
        {
            "Eeloo", new Dictionary<int, RegionColor>
            {
                { 2, new RegionColor { RegionId = "EelooCanyons", Color = new Color32(51, 47, 38, 255) } },
                { 3, new RegionColor { RegionId = "EelooCraters", Color = new Color32(105, 100, 100, 255) } },
                { 1, new RegionColor { RegionId = "EelooGreatRift", Color = new Color32(176, 181, 181, 255) } },
                { 0, new RegionColor { RegionId = "EelooIceFields", Color = new Color32(149, 227, 243, 255) } },
            }
        },
    };
    
    public struct RegionColor
    {
        public string RegionId;
        public Color32 Color;
    }
}