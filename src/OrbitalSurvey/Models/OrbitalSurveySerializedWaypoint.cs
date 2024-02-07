using SpaceWarp.API.Game.Waypoints;

namespace OrbitalSurvey.Models;

public class OrbitalSurveySerializedWaypoint: SerializedWaypoint
{
    public OrbitalSurveySerializedWaypoint(
        string name, string bodyName, double latitude, double longitude, double altitude, WaypointState state,
        WaypointColor waypointColor)
        : base(name, bodyName, latitude, longitude, altitude, state)
    {
        this.WaypointColor = waypointColor;
    }

    public WaypointColor WaypointColor { get; }
    
    /// <summary>
    /// Deserializes the waypoint, creating an actual waypoint from it
    /// </summary>
    /// <returns>A newly created waypoint from the serialized waypoint's parameters</returns>
    public OrbitalSurveyWaypoint Deserialize() => new OrbitalSurveyWaypoint(base.Latitude, base.Longitude, base.Altitude,
        base.BodyName, base.Name, base.State, this.WaypointColor);
}