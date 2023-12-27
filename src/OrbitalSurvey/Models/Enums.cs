namespace OrbitalSurvey.Models;

public enum MapType
{
    Visual,
    Biome
}

public enum Status
{ 
    Disabled,
    Scanning,
    Idle,
    Complete,
    NoPower
}

public enum State
{
    BelowMin,
    BelowIdeal,
    Ideal,
    AboveIdeal,
    AboveMax,
}

public enum Difficulty
{
    EASY,
    NORMAL,
    HARD
}

public enum ExperimentLevel
{
    None,
    Quarter,
    Half,
    ThreeQuarters,
    Full
}