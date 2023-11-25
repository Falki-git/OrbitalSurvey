namespace OrbitalSurvey.Models;

public class SaveDataAdapter
{
    public Dictionary<MapType, MapsAdapter> Maps = new();

    public struct MapsAdapter
    {
        public string DiscoveredPixels;
        public bool IsFullyScanned;
    }
}