using Newtonsoft.Json;
using OrbitalSurvey.UI.Controls;
using UnityEngine;

namespace OrbitalSurvey.Models;

[Serializable]
public class WaypointModel
{
    public OrbitalSurveyWaypoint Waypoint { get; set; }
    [JsonIgnore]
    public MapMarkerControl Marker { get; set; }
    public string Body { get; set; }
    public Vector2 MapPositionPercentage { get; set; }
}