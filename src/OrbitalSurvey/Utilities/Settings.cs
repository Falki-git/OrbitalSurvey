namespace OrbitalSurvey.Utilities;

public static class Settings
{
    public const bool WILL_DEBUG_WINDOW_OPEN_ON_GAME_LOAD = true;
    public const double TIME_BETWEEN_SCANS = 1f;
    public const double TIME_BETWEEN_RETROACTIVE_SCANS_LOW = 8f;
    public const double TIME_BETWEEN_RETROACTIVE_SCANS_MID = 20f;
    public const double TIME_BETWEEN_RETROACTIVE_SCANS_HIGH = 50f;
    public static readonly List<int> AVAILABLE_RESOLUTIONS = new() { 1024 };
    public static int ActiveResolution = 1024;

    public static double VisualMinAltitude = 100000;
    public static double VisualIdealAltitude = 500000;
    public static double VisualMaxAltitude = 1000000;
    public static double BiomeMinAltitude = 300000;
    public static double BiomeIdealAltitude = 750000;
    public static double BiomeMaxAltitude = 900000;
}