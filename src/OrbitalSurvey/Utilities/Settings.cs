namespace OrbitalSurvey.Utilities;

public static class Settings
{
    public const bool WILL_DEBUG_WINDOW_OPEN_ON_GAME_LOAD = true;
    public const double TIME_BETWEEN_SCANS = 1f;
    public const double TIME_BETWEEN_RETROACTIVE_SCANS = 8f;
    public static readonly List<int> AVAILABLE_RESOLUTIONS = new() { 1024 };
    public static int ActiveResolution = 1024;
}