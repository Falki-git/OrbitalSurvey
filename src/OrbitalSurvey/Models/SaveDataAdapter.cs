using UnityEngine;

namespace OrbitalSurvey.Models;

public class SaveDataAdapter
{
    public string SessionGuidString;
    public Vector3? WindowPosition;
    public Dictionary<string, Dictionary<MapType, MapsAdapter>> Bodies = new();
    public List<OrbitalSurveySerializedWaypoint> Waypoints = new();

    public struct MapsAdapter
    {
        public string DiscoveredPixels;
        public bool IsFullyScanned;
        public ExperimentLevel ExperimentLevel;
    }
}