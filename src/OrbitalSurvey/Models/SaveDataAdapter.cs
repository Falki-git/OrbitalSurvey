namespace OrbitalSurvey.Models;

public class SaveDataAdapter
{
    public string SessionGuidString;
    public Dictionary<string, Dictionary<MapType, MapsAdapter>> Bodies = new();

    public struct MapsAdapter
    {
        public string DiscoveredPixels;
        public bool IsFullyScanned;
    }
}