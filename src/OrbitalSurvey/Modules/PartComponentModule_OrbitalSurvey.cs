using BepInEx.Logging;
using KSP.Sim.impl;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;
using OrbitalSurvey.Utilities;

namespace OrbitalSurvey.Modules;

public class PartComponentModule_OrbitalSurvey : PartComponentModule
{
    public override Type PartBehaviourModuleType => typeof(Module_OrbitalSurvey);
    public double LastScanTime;
    
    private static readonly ManualLogSource _logger = Logger.CreateLogSource("OrbitalSurvey.PartComponentModule");
    private Data_OrbitalSurvey _dataOrbitalSurvey;
    private double _timeSinceLastScan => ScanUtility.UT - LastScanTime;
    
    

    // This triggers when Flight scene is loaded. It triggers for active vessels also.
    public override void OnStart(double universalTime)
    {
        _logger.LogDebug("OnStart triggered.");

        if (!DataModules.TryGetByType<Data_OrbitalSurvey>(out _dataOrbitalSurvey))
        {
            _logger.LogError("Unable to find a Data_OrbitalSurvey in the PartComponentModule for " + base.Part.PartName);
            return;
        }

        if (string.IsNullOrEmpty(_dataOrbitalSurvey.Mode.GetValue()))
        {
            _dataOrbitalSurvey.Mode.SetValue(MapType.Visual.ToString());
        }

        //_dataOrbitalSurvey.Mode.OnChangedValue += OnModeChanged;
    }

    // This starts triggering when vessel is placed in Flight. Does not trigger in OAB.
    // Keeps triggering in every scene once it's in Flight 
    public override void OnUpdate(double universalTime, double deltaUniversalTime)
    {
        if (_dataOrbitalSurvey.EnabledToggle.GetValue() &&
            _timeSinceLastScan >= Settings.TIME_BETWEEN_SCANS)
        {
            var vessel = base.Part.PartOwner.SimulationObject.Vessel;
            var body = vessel.mainBody.Name;
            var mapType = Enum.Parse<MapType>(_dataOrbitalSurvey.Mode.GetValue());
            var scanningCone = _dataOrbitalSurvey.ScanningFieldOfView.GetValue();
            
            _logger.LogDebug($"'{vessel.Name}' ({body}) scanning enabled. Last scan: {LastScanTime}.\n" + 
                    $"T since last scan: {_timeSinceLastScan}. UT: {universalTime}. dUT: {deltaUniversalTime}");
            
            double latitude, longitude, altitude;

            // for large time warp factors time between updates will be a lot larger, so we need to do a bit of catching up
            // we'll iterate through each "time between scans" from the last scan time until we're caught up to the present
            while (_timeSinceLastScan > Settings.TIME_BETWEEN_RETROACTIVE_SCANS)
            {
                
                OrbitUtility.GetOrbitalParametersAtUT(vessel, LastScanTime + Settings.TIME_BETWEEN_RETROACTIVE_SCANS,
                    out latitude, out longitude, out altitude);
        
                Core.Instance.DoScan(body, mapType, longitude, latitude, altitude, scanningCone);
                LastScanTime += Settings.TIME_BETWEEN_RETROACTIVE_SCANS;
            }
            
            // proceed with a normal scan
            altitude = vessel.AltitudeFromRadius;
            longitude = vessel.Longitude;
            latitude = vessel.Latitude;
            
            Core.Instance.DoScan(body, mapType, longitude, latitude, altitude, scanningCone);

            LastScanTime = universalTime;

            // FOR DEBUGGING PURPOSES
            if (DEBUG_UI.Instance.BufferAnalyticsScan)
            {
                RetroactiveScanning(double.Parse(DEBUG_UI.Instance.UT));
                DEBUG_UI.Instance.BufferAnalyticsScan = false;
            }
        }
    }

    private void RetroactiveScanning(double ut)
    {
        var vessel = base.Part.PartOwner.SimulationObject.Vessel;
        var body = vessel.mainBody.Name;
        var mapType = Enum.Parse<MapType>(_dataOrbitalSurvey.Mode.GetValue());
        var scanningCone = _dataOrbitalSurvey.ScanningFieldOfView.GetValue();
        
        double latitude, longitude, altitude;
        OrbitUtility.GetOrbitalParametersAtUT(vessel, ut, out latitude, out longitude, out altitude);
        
        Core.Instance.DoScan(body, mapType, longitude, latitude, altitude, scanningCone);
        
        Core.Instance.CelestialDataDictionary[body].Maps[mapType].UpdateCurrentMapAsPerDiscoveredPixels();
    }

    public override void OnShutdown()
    {
        _logger.LogDebug("OnShutdown triggered.");
    }

    // -
    public override void OnFinalizeCreation(double universalTime)
    {
        _logger.LogDebug("OnFinalizeCreation triggered.");
    }
}