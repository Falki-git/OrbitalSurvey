using I2.Loc;

namespace OrbitalSurvey.Models;

public static class StatusStrings
{
    public static readonly LocalizedString test = "PartModules/OrbitalSurvey/ScanningFOV";
    
    public static readonly Dictionary<string, LocalizedString> Status = new()
    {
        { "Idle", "OrbitalSurvey/Status/Idle" },
        { "Scanning", "OrbitalSurvey/Status/Scanning" },
        { "Disabled", "OrbitalSurvey/Status/Disabled" },
        { "NoPower", "OrbitalSurvey/Status/NoPower" },
    };
    
    public static readonly Dictionary<string, LocalizedString> State = new()
    {
        { "BelowMin", "OrbitalSurvey/State/BelowMin" },
        { "BelowIdeal", "OrbitalSurvey/State/BelowIdeal" },
        { "Ideal", "OrbitalSurvey/State/Ideal" },
        { "AboveIdeal", "OrbitalSurvey/State/AboveIdeal" },
        { "AboveMax", "OrbitalSurvey/State/AboveMax" },
    };
}