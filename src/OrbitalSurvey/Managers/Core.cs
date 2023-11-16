using OrbitalSurvey.Models;

namespace OrbitalSurvey.Managers;

public class Core
{
    private Core() { }
    public static Core Instance { get; } = new();

    public CelestialDataDictionary CelestialDataDictionary { get; set; }
    
    // TODO map initialization
}

