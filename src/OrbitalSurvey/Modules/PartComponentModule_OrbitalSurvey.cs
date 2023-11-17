using BepInEx.Logging;
using KSP.Sim.impl;
using OrbitalSurvey.Managers;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;
using OrbitalSurvey.Utilities;

namespace OrbitalSurvey.Modules;

public class PartComponentModule_OrbitalSurvey : PartComponentModule
{
    private static readonly ManualLogSource _logger = Logger.CreateLogSource("OrbitalSurvey.PartComponentModule");
    private Data_OrbitalSurvey _dataOrbitalSurvey;

    private float _lastScanTime = Time.time;
    private float _timeSinceLastScan => Time.time - _lastScanTime;
    
    public override Type PartBehaviourModuleType => typeof(Module_OrbitalSurvey);

    // This triggers when Flight scene is loaded. It triggers for active vessels also.
    public override void OnStart(double universalTime)
    {
        _logger.LogDebug("OnStart triggered.");

        if (!DataModules.TryGetByType<Data_OrbitalSurvey>(out _dataOrbitalSurvey))
        {
            _logger.LogError("Unable to find a Data_OrbitalSurvey in the PartComponentModule for " + base.Part.PartName);
            return;
        }

        _dataOrbitalSurvey.Mode.OnChangedValue += OnModeChanged;
    }

    // This starts triggering when vessel is placed in Flight. Does not trigger in OAB.
    // Keeps triggering in every scene once it's in Flight 
    public override void OnUpdate(double universalTime, double deltaUniversalTime)
    {
        if (_dataOrbitalSurvey.EnabledToggle.GetValue() && _timeSinceLastScan >= Settings.TIME_BETWEEN_SCANS)
        {
            _logger.LogDebug($"Scanning is enabled. Time since last scan: {_timeSinceLastScan}. UT: {universalTime}. DeltaUT: {deltaUniversalTime}");
            
            // TODO do scan here

            _lastScanTime = Time.time;
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
    
    private void OnModeChanged(string mode)
    {
        // TODO check if we need this
    }
}