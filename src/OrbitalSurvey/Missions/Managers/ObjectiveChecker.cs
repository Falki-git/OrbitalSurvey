using KSP.Game;
using KSP.Sim.impl;
using SpaceWarp.API.Logging;
using UnityEngine;
using ILogger = SpaceWarp.API.Logging.ILogger;

namespace OrbitalSurvey.Missions.Managers;

public class ObjectiveChecker: MonoBehaviour
{
    protected ObjectiveChecker()
    {
        Logger = new BepInExLogger(BepInEx.Logging.Logger.CreateLogSource($"OrbitalSurvey.{GetType().Name}"));
    }
    
    public static ObjectiveChecker Instance { get; set; }
    
    public VesselComponent ActiveVessel;
    
    internal ILogger Logger { get; }
    
    public void Initialize()
    { }
    
    private void Start()
    {
        Instance = this;
    }

    public double[] DebugDistance = new double[3];
    
    private void Update()
    {
        // check if there's an active vessel
        if ((ActiveVessel = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle()?.GetSimVessel()) == null)
            return;

        // check if there's an active Orbital Survey mission where the active vessel is
        if (!MissionManager.Instance.ActiveMissions.ContainsKey(ActiveVessel.mainBody.bodyName))
            return;

        // check if the active vessel has landed
        //if (ActiveVessel.Situation is not VesselSituations.Landed and not VesselSituations.Splashed)
        //    return;
        
        var activeMission = MissionManager.Instance.ActiveMissions[ActiveVessel.mainBody.bodyName];

        // check the distance to all the not-completed optional objectives
        for (int i = 0; i < activeMission.Objectives.Count; i++)
        {
            var optionalObjective = activeMission.Objectives[i];
            
            var distance = GetDistanceBetweenGeoCoordinates(ActiveVessel.Latitude, ActiveVessel.Longitude,
                optionalObjective.Latitude, optionalObjective.Longitude, ActiveVessel.mainBody.radius);

            DebugDistance[i] = distance;
        }
        
    }

    /// <summary>
    /// Calculates the surface distance between two geographic coordinates
    /// </summary>
    /// <param name="latitude1">Latitude of the first point</param>
    /// <param name="longitude1">Longitude of the first point</param>
    /// <param name="latitude2">Latitude of the second point</param>
    /// <param name="longitude2">Longitude of the second point</param>
    /// <param name="radius">Radius of the reference body</param>
    /// <returns>Distance in meters</returns>
    /// <source>https://stackoverflow.com/a/365853/2160147</source>
    public double GetDistanceBetweenGeoCoordinates(double latitude1, double longitude1, double latitude2, double longitude2, double radius)
    {
        var dLat = (float)((latitude2 - latitude1) * Mathf.Deg2Rad);
        var dLon = (float)((longitude2 - longitude1) * Mathf.Deg2Rad);

        var latRad1 = (float)(latitude1 * Mathf.Deg2Rad);
        var latRad2 = (float)(latitude2 * Mathf.Deg2Rad);

        var a = MathF.Sin(dLat/2) * MathF.Sin(dLat/2) +
                MathF.Sin(dLon/2) * MathF.Sin(dLon/2) * MathF.Cos(latRad1) * MathF.Cos(latRad2); 
        var c = 2 * MathF.Atan2(MathF.Sqrt(a), MathF.Sqrt(1-a)); 
        return radius * c;
    }
    
    
}