using OrbitalSurvey.UI.Controls;
using UnityEngine;

namespace OrbitalSurvey.Models;

public class WaypointObject
{
    public OrbitalSurveyWaypoint Waypoint { get; set; }
    public MapMarkerControl Marker { get; set; }
    public string Body { get; set; }
    public Vector2 MapPositionPercentage { get; set; }
}