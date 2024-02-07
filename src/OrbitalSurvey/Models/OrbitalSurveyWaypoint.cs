using SpaceWarp.API.Game.Waypoints;

namespace OrbitalSurvey.Models;

public class OrbitalSurveyWaypoint: Waypoint
{
    public OrbitalSurveyWaypoint(double latitude, double longitude, double? altitudeFromRadius = null,
        string bodyName = null, string name = null, WaypointState waypointState = WaypointState.Visible,
        WaypointColor waypointColor = WaypointColor.Yellow)
        : base(latitude, longitude, altitudeFromRadius, bodyName, name, waypointState)
    {
        WaypointColor = waypointColor;
    }

    public WaypointColor WaypointColor { get; set; }
    
    public virtual OrbitalSurveySerializedWaypoint Serialize()
    {
        return new OrbitalSurveySerializedWaypoint(this.Name, this.BodyName, this.Latitude, this.Longitude, this.AltitudeFromRadius, this.State, this.WaypointColor);
    }
}