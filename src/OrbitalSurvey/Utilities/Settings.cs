using BepInEx.Configuration;
using OrbitalSurvey.Models;

namespace OrbitalSurvey.Utilities;

public static class Settings
{
    public static OrbitalSurveyPlugin Plugin => OrbitalSurveyPlugin.Instance;

    public static ConfigEntry<float> TimeBetweenScans;
    public static ConfigEntry<bool> ShowRegionLegend;
    public static ConfigEntry<bool> PlayUiSounds;
    public static ConfigEntry<float> GuiRefreshInterval;
    
    public static ConfigEntry<int> TimeBetweenRetroactiveScansHigh;
    public static ConfigEntry<int> TimeBetweenRetroactiveScansMid;
    public static ConfigEntry<int> TimeBetweenRetroactiveScansLow;
    public static ConfigEntry<Difficulty> Difficulty; // Not used
    
    public const bool WILL_DEBUG_WINDOW_OPEN_ON_GAME_LOAD = false;
    public static readonly List<int> AVAILABLE_RESOLUTIONS = new() { 1024 };
    public static int ActiveResolution = 1024;
    
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
        
        PlayUiSounds = Plugin.Config.Bind(
            "General",
            "Play UI sounds",
            true,
            new ConfigDescription("If UI sounds will be played when clicked.")
        );
        
        GuiRefreshInterval = Plugin.Config.Bind(
            "General",
            "GUI refresh interval (in sec)",
            1f,
            new ConfigDescription(
                "How much time in seconds needs to pass for mapping GUI to refresh.",
                new AcceptableValueRange<float>(0.5f, 5f)
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