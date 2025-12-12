using System.Collections.Generic;
using ReduxLib.Configuration;
using OrbitalSurvey.Models;

namespace OrbitalSurvey.Utilities
{
    public static class Settings
    {
        public static OrbitalSurveyPlugin Plugin => OrbitalSurveyPlugin.Instance;

        // "General" section
        public static ConfigValue<float> TimeBetweenScans;
        public static ConfigValue<bool> PlayUiSounds;
        public static ConfigValue<bool> ShowMapOverlayAlways;

        // "Maps" section
        public static ConfigValue<bool> ShowRegionLegend;
        public static ConfigValue<float> GuiRefreshInterval;
        public static ConfigValue<float> ZoomFactor;
        public static ConfigValue<float> MaxZoom;

        // "Time between retroactive scans during high warp" section
        public static ConfigValue<int> TimeBetweenRetroactiveScansHigh;
        public static ConfigValue<int> TimeBetweenRetroactiveScansMid;
        public static ConfigValue<int> TimeBetweenRetroactiveScansLow;
        public static ConfigValue<Difficulty> Difficulty; // Not used

        public const bool WILL_DEBUG_WINDOW_OPEN_ON_GAME_LOAD = false;
        public static readonly List<int> AVAILABLE_RESOLUTIONS = new() { 1024 };
        public static int ActiveResolution = 1024;
        public static int HighResolution = 2048;

        public static void Initialize()
        {
            // GENERAL
            TimeBetweenScans = new(Plugin.SWConfiguration.Bind(
                "General",
                "Time between scans (in sec)",
                1f, 
                "Time between scans.\n\n"
                    + "Increase the value for better performance, but at a cost for possibility of spotty scans.", 
                new RangeConstraint<float>(0.5f, 5f) 
                ));

            PlayUiSounds = new(Plugin.SWConfiguration.Bind(
                "General",
                "Play UI sounds",
                true,
                "If UI sounds will be played when clicked." 
                ));

            ShowMapOverlayAlways = new(Plugin.SWConfiguration.Bind(
                "General",
                "Always show Map view overlay",
                false,
                "If toggled on Map view overlay texture will always be applied to every body in game.\n\n"
                       + "This effectively makes all bodies \"hidden\" in Map view until you scan them." 
                ));

            // MAPS
            ShowRegionLegend = new(Plugin.SWConfiguration.Bind(
                "Maps",
                "Show Region legend when mapping is 100% complete",
                true,
                "Whether a legend with colors and Region names will be shown after Region mapping is 100% complete.\n\n"
                    + "Toggle this off if you want spoiler free mapping." 
                ));

            GuiRefreshInterval = new(Plugin.SWConfiguration.Bind(
                "Maps",
                "UI refresh interval (in sec)",
                1f,
                "How much time in seconds needs to pass for mapping UI to refresh.",
                new RangeConstraint<float>(0.5f, 5f)
                ));

            ZoomFactor = new(Plugin.SWConfiguration.Bind(
                "Maps",
                "Zoom factor",
                1.10f,
                "How \"aggressive\" zooming will be.",
                new RangeConstraint<float>(1.05f, 1.50f)
                )
            );

            MaxZoom = new(Plugin.SWConfiguration.Bind(
                "Maps",
                "Maximum zoom",
                10f,
                "What's the maximum zoom.", 
                new RangeConstraint<float>(5f, 20f)
                ));

            TimeBetweenRetroactiveScansHigh = new(Plugin.SWConfiguration.Bind(
                "Time between retroactive scans during high warp",
                "When performance is high (in sec)",
                2,
                "Time between scans when analytics scanning kicks in during high warp factors.\n\n"
                   + "Increase the value for better performance, but at a cost for possibility of spotty scans.", 
                new RangeConstraint<int>(2, 20)
                ));

            TimeBetweenRetroactiveScansMid = new(Plugin.SWConfiguration.Bind(
                "Time between retroactive scans during high warp",
                "When performance is medium (in sec)",
                5, 
                "Time between scans when analytics scanning kicks in during high warp factors.\n\n"
                   + "Increase the value for better performance, but at a cost for possibility of spotty scans.", 
                new RangeConstraint<int>(3, 50)
                ));

            TimeBetweenRetroactiveScansLow = new(Plugin.SWConfiguration.Bind(
                "Time between retroactive scans during high warp",
                "When performance is low (in sec)",
                10, 
                "Time between scans when analytics scanning kicks in during high warp factors.\n\n"
                    + "Increase the value for better performance, but at a cost for possibility of spotty scans.", 
                new RangeConstraint<int>(4, 80)
                ));
        }
    }
}