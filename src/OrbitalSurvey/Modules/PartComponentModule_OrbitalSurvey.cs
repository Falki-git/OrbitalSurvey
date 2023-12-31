using BepInEx.Logging;
using KSP.Game;
using KSP.Modules;
using KSP.Sim.impl;
using KSP.Sim.ResourceSystem;
using OrbitalSurvey.Debug;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;
using Logger = BepInEx.Logging.Logger;
using OrbitalSurvey.Utilities;

namespace OrbitalSurvey.Modules;

public class PartComponentModule_OrbitalSurvey : PartComponentModule
{
    public override Type PartBehaviourModuleType => typeof(Module_OrbitalSurvey);
    public double LastScanTime;
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.PartComponentModule");
    private Data_OrbitalSurvey _dataOrbitalSurvey;
    private double _timeSinceLastScan => Utility.UT - LastScanTime;
    
    private FlowRequestResolutionState _returnedRequestResolutionState;
    private bool _hasOutstandingRequest;
    private Data_ScienceExperiment _dataScienceExperiment; 
    private PartComponentModule_ScienceExperiment _moduleScienceExperiment;

    private bool _isDebugCustomFovEnabled;

    // This triggers when Flight scene is loaded. It triggers for active vessels also.
    public override void OnStart(double universalTime)
    {
        //_LOGGER.LogDebug("OnStart triggered.");

        if (!DataModules.TryGetByType<Data_OrbitalSurvey>(out _dataOrbitalSurvey))
        {
            _LOGGER.LogError("Unable to find a Data_OrbitalSurvey in the PartComponentModule for " + base.Part.PartName);
            return;
        }

        if (string.IsNullOrEmpty(_dataOrbitalSurvey.Mode.GetValue()))
        {
            _dataOrbitalSurvey.Mode.SetValue(MapType.Visual.ToString());
        }

        _dataOrbitalSurvey.SetupResourceRequest(base.resourceFlowRequestBroker);

        Part.TryGetModule(typeof(PartComponentModule_ScienceExperiment), out var m);
        _moduleScienceExperiment = m as PartComponentModule_ScienceExperiment;
        _dataScienceExperiment = _moduleScienceExperiment?.dataScienceExperiment;

    LastScanTime = Utility.UT;
    }

    // This starts triggering when vessel is placed in Flight. Does not trigger in OAB.
    // Keeps triggering in every scene once it's in Flight 
    public override void OnUpdate(double universalTime, double deltaUniversalTime)
    {
        ResourceConsumptionUpdate(deltaUniversalTime);
        DoScan(universalTime);
    }

    private void DoScan(double universalTime)
    {
        if (_dataOrbitalSurvey.EnabledToggle.GetValue() &&
            _timeSinceLastScan >= (double)Settings.TimeBetweenScans.Value)
        {
            // if EC is spent, skip scanning
            if (!_dataOrbitalSurvey.HasResourcesToOperate)
            {
                LastScanTime = universalTime;
                return;
            }
            
            var vessel = base.Part.PartOwner.SimulationObject.Vessel;
            var body = vessel.mainBody.Name;
            var mapType = Enum.Parse<MapType>(_dataOrbitalSurvey.Mode.GetValue());

            // check if debugging scanning FOV needs to be applied or removed
            if (DebugUI.Instance.DebugFovEnabled != _isDebugCustomFovEnabled)
            {
                _dataOrbitalSurvey.ScanningStats.FieldOfView = DebugUI.Instance.DebugFovEnabled ?
                    _dataOrbitalSurvey.ScanningFieldOfViewDebug.GetValue() :  _dataOrbitalSurvey.ScanningFieldOfView.GetValue();
                _isDebugCustomFovEnabled = DebugUI.Instance.DebugFovEnabled;
            }
            
            PerformRetroactiveScanningIfNeeded(vessel, body, mapType, _dataOrbitalSurvey.ScanningStats);
            
            // proceed with a normal scan
            var altitude = vessel.AltitudeFromRadius;
            var longitude = vessel.Longitude;
            var latitude = vessel.Latitude;
            
            Core.Instance.DoScan(body, mapType, longitude, latitude, altitude, _dataOrbitalSurvey.ScanningStats);
            
            // check is experiment needs to trigger and if so, trigger it
            Core.Instance.CheckIfExperimentNeedsToTrigger(_moduleScienceExperiment, body, mapType);
            
            LastScanTime = universalTime;

            // FOR DEBUGGING PURPOSES
            if (DebugUI.Instance.BufferAnalyticsScan)
            {
                DebuggingRetroactiveScanning(double.Parse(DebugUI.Instance.UT));
                DebugUI.Instance.BufferAnalyticsScan = false;
            }
        }
    }

    private void PerformRetroactiveScanningIfNeeded(VesselComponent vessel, string body, MapType mapType, ScanningStats scanningStats)
    {
        double latitude, longitude, altitude;
        
        // if time since last scan begins to rise up (due to low performance), reduce the frequency of scans 
        var retroactiveTimeBetweenScans = ScanUtility.GetRetroactiveTimeBetweenScans(_timeSinceLastScan);

        // for large time warp factors time between updates will be a lot larger, so we need to do a bit of catching up
        // we'll iterate through each "time between scans" from the last scan time until we're caught up to the present
        while (_timeSinceLastScan > retroactiveTimeBetweenScans)
        {
            OrbitUtility.GetOrbitalParametersAtUT(vessel, LastScanTime + retroactiveTimeBetweenScans,
                out latitude, out longitude, out altitude);
        
            Core.Instance.DoScan(body, mapType, longitude, latitude, altitude, scanningStats, true);
            LastScanTime += retroactiveTimeBetweenScans;
        }
    }

    private void DebuggingRetroactiveScanning(double ut)
    {
        var vessel = base.Part.PartOwner.SimulationObject.Vessel;
        var body = vessel.mainBody.Name;
        var mapType = Enum.Parse<MapType>(_dataOrbitalSurvey.Mode.GetValue());
        
        double latitude, longitude, altitude;
        OrbitUtility.GetOrbitalParametersAtUT(vessel, ut, out latitude, out longitude, out altitude);
        
        Core.Instance.DoScan(body, mapType, longitude, latitude, altitude, _dataOrbitalSurvey.ScanningStats);
        
        Core.Instance.CelestialDataDictionary[body].Maps[mapType].UpdateCurrentMapAsPerDiscoveredPixels();
    }
    
    // Handles EC consumption
    private void ResourceConsumptionUpdate(double deltaTime)
    {
        if (_dataOrbitalSurvey.UseResources)
        {
            if (GameManager.Instance.Game.SessionManager.IsDifficultyOptionEnabled("InfinitePower"))
            {
                _dataOrbitalSurvey.HasResourcesToOperate = true;
                if (base.resourceFlowRequestBroker.IsRequestActive(_dataOrbitalSurvey.RequestHandle))
                {
                    base.resourceFlowRequestBroker.SetRequestInactive(_dataOrbitalSurvey.RequestHandle);
                    return;
                }
            }
            else
            {
                if (this._hasOutstandingRequest)
                {
                    this._returnedRequestResolutionState = base.resourceFlowRequestBroker.GetRequestState(_dataOrbitalSurvey.RequestHandle);
                    _dataOrbitalSurvey.HasResourcesToOperate = this._returnedRequestResolutionState.WasLastTickDeliveryAccepted;
                }
                this._hasOutstandingRequest = false;
                if (!_dataOrbitalSurvey.EnabledToggle.GetValue() && base.resourceFlowRequestBroker.IsRequestActive(_dataOrbitalSurvey.RequestHandle))
                {
                    base.resourceFlowRequestBroker.SetRequestInactive(_dataOrbitalSurvey.RequestHandle);
                    _dataOrbitalSurvey.HasResourcesToOperate = false;
                }
                else if (_dataOrbitalSurvey.EnabledToggle.GetValue() && base.resourceFlowRequestBroker.IsRequestInactive(_dataOrbitalSurvey.RequestHandle))
                {
                    base.resourceFlowRequestBroker.SetRequestActive(_dataOrbitalSurvey.RequestHandle);
                }
                if (_dataOrbitalSurvey.EnabledToggle.GetValue())
                {
                    _dataOrbitalSurvey.RequestConfig.FlowUnits = (double)_dataOrbitalSurvey.RequiredResource.Rate;
                    base.resourceFlowRequestBroker.SetCommands(_dataOrbitalSurvey.RequestHandle, 1.0, new ResourceFlowRequestCommandConfig[] { _dataOrbitalSurvey.RequestConfig });
                    this._hasOutstandingRequest = true;
                    return;
                }
            }
        }
        else
        {
            _dataOrbitalSurvey.HasResourcesToOperate = true;
        }
    }

    public override void OnShutdown()
    {
        _LOGGER.LogDebug("OnShutdown triggered.");
    }

    // -
    public override void OnFinalizeCreation(double universalTime)
    {
        _LOGGER.LogDebug("OnFinalizeCreation triggered.");
    }
}