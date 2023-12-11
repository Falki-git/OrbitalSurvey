using OrbitalSurvey.Models;

namespace OrbitalSurvey.Utilities;

public static class Settings
{
    public const bool WILL_DEBUG_WINDOW_OPEN_ON_GAME_LOAD = false;
    public const double TIME_BETWEEN_SCANS = 1f;
    public const double TIME_BETWEEN_RETROACTIVE_SCANS_LOW = 8f;
    public const double TIME_BETWEEN_RETROACTIVE_SCANS_MID = 20f;
    public const double TIME_BETWEEN_RETROACTIVE_SCANS_HIGH = 50f;
    public static readonly List<int> AVAILABLE_RESOLUTIONS = new() { 1024 };
    public static int ActiveResolution = 1024;

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
}