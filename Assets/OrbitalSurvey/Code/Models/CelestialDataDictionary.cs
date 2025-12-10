namespace OrbitalSurvey.Models;

public class CelestialDataDictionary : Dictionary<string, CelestialData>
{
    public new void Add(string body, CelestialData celestialData) => base.Add(body, celestialData);

    public new CelestialData this[string body]
    {
        get => base[body];
        set => base[body] = value;
    }
}