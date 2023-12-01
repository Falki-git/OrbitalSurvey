using I2.Loc;

namespace OrbitalSurvey.Models;

public static class StatusStrings
{
    public static readonly Dictionary<Status, LocalizedString> STATUS = new()
    {
        { Status.Disabled, "OrbitalSurvey/Status/Disabled" },
        { Status.Scanning, "OrbitalSurvey/Status/Scanning" },
        { Status.Idle, "OrbitalSurvey/Status/Idle" },
        { Status.Complete, "OrbitalSurvey/Status/Complete" },
        { Status.NoPower, "OrbitalSurvey/Status/NoPower" },
    };
    
    public static readonly Dictionary<State, LocalizedString> STATE = new()
    {
        { State.BelowMin, "OrbitalSurvey/State/BelowMin" },
        { State.BelowIdeal, "OrbitalSurvey/State/BelowIdeal" },
        { State.Ideal, "OrbitalSurvey/State/Ideal" },
        { State.AboveIdeal, "OrbitalSurvey/State/AboveIdeal" },
        { State.AboveMax, "OrbitalSurvey/State/AboveMax" },
    };
}