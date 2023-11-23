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
    
    private static readonly ManualLogSource _logger = Logger.CreateLogSource("OrbitalSurvey.PartComponentModule");
    private Data_OrbitalSurvey _dataOrbitalSurvey;
    private double _lastScanTime;
    private double _timeSinceLastScan => ScanUtility.UT - _lastScanTime;
    
    

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
            
            _logger.LogDebug($"'{vessel.Name}' ({body}) scanning enabled. Last scan: {_lastScanTime}.\n" + 
                    "Time since last scan: {_timeSinceLastScan}. UT: {universalTime}. DeltaUT: {deltaUniversalTime}");
            
            var altitude = vessel.AltitudeFromRadius;
            var longitude = vessel.Longitude;
            var latitude = vessel.Latitude;
            
            var mapType = Enum.Parse<MapType>(_dataOrbitalSurvey.Mode.GetValue());
            var scanningCone = _dataOrbitalSurvey.ScanningFieldOfView.GetValue();
            
            Core.Instance.DoScan(body, mapType, longitude, latitude, altitude, scanningCone);

            _lastScanTime = universalTime;
        }
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