using BepInEx.Configuration;
using OrbitalSurvey.Models;

namespace OrbitalSurvey.Utilities;

public static class Settings
{
    public static OrbitalSurveyPlugin Plugin => OrbitalSurveyPlugin.Instance;

    public static ConfigEntry<float> TimeBetweenScans;
    public static ConfigEntry<int> TimeBetweenRetroactiveScansHigh;
    public static ConfigEntry<int> TimeBetweenRetroactiveScansMid;
    public static ConfigEntry<int> TimeBetweenRetroactiveScansLow;
    public static ConfigEntry<Difficulty> Difficulty; // Not used
    
    public static ConfigEntry<bool> ShowRegionLegend;
    
    public const bool WILL_DEBUG_WINDOW_OPEN_ON_GAME_LOAD = false;
    public static readonly List<int> AVAILABLE_RESOLUTIONS = new() { 1024 };
    public static int ActiveResolution = 1024;
    
    /*
    public static Dictionary<MapType, ScanningStats> ModeScanningStats = new()
    {
        {
            MapType.Visual,
            new ScanningStats
            {
                FieldOfView = 10,
                MinAltitude = 100000,
                IdealAltitude = 500000,
                MaxAltitude = 1000000
            }
        },
        {
            MapType.Biome,
            new ScanningStats
            {
                FieldOfView = 5,
                MinAltitude = 300000,
                IdealAltitude = 750000,
                MaxAltitude = 900000
            }
        }
    };
    
    public struct ScanningStats
    {
        public int FieldOfView;
        public float MinAltitude;
        public float IdealAltitude;
        public float MaxAltitude;
    }
    
    public static Dictionary<MapType, float> EcConsumptionRate = new()
    {
        { MapType.Visual, 1.0f },
        { MapType.Biome, 2.0f }
    };
    */
    
    public static void Initialize()
    {
        TimeBetweenScans = Plugin.Config.Bind(
            "General",
            "Time between scans (in sec)",
            1f,
            new ConfigDescription(
                "Time between scans.\n\n"
                + "Increase the value for better performance, but at a cost for possibility of spotty scans.",
                new AcceptableValueRange<float>(0.5f, 5f)
            )
        );
        
        ShowRegionLegend = Plugin.Config.Bind(
            "General",
            "Show Region legend when mapping is 100% complete",
            true,
            new ConfigDescription(
                "Whether a legend with colors and Region names will be shown after Region mapping is 100% complete.\n\n"
                + "Toggle this off if you want spoiler free mapping."
            )
        );
        
        TimeBetweenRetroactiveScansHigh = Plugin.Config.Bind(
            "Time between retroactive scans during high warp",
            "When performance is high (in sec)",
            2,
            new ConfigDescription(
                "Time between scans when analytics scanning kicks in during high warp factors.\n\n"
                + "Increase the value for better performance, but at a cost for possibility of spotty scans.",
                new AcceptableValueRange<int>(2, 20)
            )
        );
        
        TimeBetweenRetroactiveScansMid = Plugin.Config.Bind(
            "Time between retroactive scans during high warp",
            "When performance is medium (in sec)",
            5,
            new ConfigDescription(
                "Time between scans when analytics scanning kicks in during high warp factors.\n\n"
                + "Increase the value for better performance, but at a cost for possibility of spotty scans.",
                new AcceptableValueRange<int>(3, 50)
            )
        );
        
        TimeBetweenRetroactiveScansLow = Plugin.Config.Bind(
            "Time between retroactive scans during high warp",
            "When performance is low (in sec)",
            10,
            new ConfigDescription(
                "Time between scans when analytics scanning kicks in during high warp factors.\n\n"
                + "Increase the value for better performance, but at a cost for possibility of spotty scans.",
                new AcceptableValueRange<int>(4, 80)
            )
        );
    }
}