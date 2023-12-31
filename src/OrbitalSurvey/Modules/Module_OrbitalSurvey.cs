using BepInEx.Logging;
using KSP.Modules;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using OrbitalSurvey.Debug;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;
using OrbitalSurvey.UI;
using OrbitalSurvey.Utilities;
using UnityEngine;

namespace OrbitalSurvey.Modules;

[DisallowMultipleComponent]
public class Module_OrbitalSurvey : PartBehaviourModule
{
    private static readonly ManualLogSource _LOGGER = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.Module");
    
    public override Type PartComponentModuleType => typeof(PartComponentModule_OrbitalSurvey);
    
    [SerializeField]
    protected Data_OrbitalSurvey _dataOrbitalSurvey;

    private ModuleAction _actionOpenGui;
    private ModuleAction _triggerExperiment;
    private ModuleAction _triggerScienceReport;

    private bool _isDebugFovEnabled;
    private bool _isDebugTriggerExperimentVisible;
    private bool _isDebugTriggerScienceReportVisible;

    public override void AddDataModules()
    {
        base.AddDataModules();
        _dataOrbitalSurvey ??= new Data_OrbitalSurvey();
        DataModules.TryAddUnique(_dataOrbitalSurvey, out _dataOrbitalSurvey);
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        
        _actionOpenGui = new ModuleAction(OnOpenMapClicked);
        _triggerExperiment = new ModuleAction(OnTriggerExperiment);
        _triggerScienceReport = new ModuleAction(OnCreateScienceReport);
        _dataOrbitalSurvey.AddAction("PartModules/OrbitalSurvey/OpenGui", _actionOpenGui, 6);
        
        // Debugging actions
        _dataOrbitalSurvey.AddAction("Trigger Experiment", _triggerExperiment, 1);
        _dataOrbitalSurvey.AddAction("Trigger Science Report", _triggerScienceReport, 1);

        if (PartBackingMode == PartBackingModes.Flight)
        {
            moduleIsEnabled = true; // this doesn't appear to have any purpose...
            _dataOrbitalSurvey.EnabledToggle.OnChangedValue += OnToggleChangedValue;
            
            var isEnabled = _dataOrbitalSurvey.EnabledToggle.GetValue();
        
            UpdateFlightPAMVisibility(isEnabled);

            if (!isEnabled)
            {
                _dataOrbitalSurvey.Status.SetValue(LocalizationStrings.STATUS[Status.Disabled]);
            }
            
            HideScienceExperimentPamProperties();    
        }
        else if (PartBackingMode == PartBackingModes.OAB)
        {
            UpdateOabPAMVisibility();
        }
    }
    
    // This triggers in flight
    public override void OnModuleFixedUpdate(float fixedDeltaTime)
    {
        if (!Core.Instance.MapsInitialized || !_dataOrbitalSurvey.EnabledToggle.GetValue())
            return;
        
        var mode = Enum.Parse<MapType>(_dataOrbitalSurvey.Mode.GetValue());
        var body = vessel.Model.mainBody.Name;
        
        // If Body doesn't exist in the dictionary (e.g. Kerbol), set to Idle and return;
        if (!Core.Instance.CelestialDataDictionary.ContainsKey(body))
        {
            _dataOrbitalSurvey.Status.SetValue(LocalizationStrings.STATUS[Status.Idle]);
            _dataOrbitalSurvey.PercentComplete.SetValue(0f);
            return;
        }
        
        var map = Core.Instance.CelestialDataDictionary[body].Maps[mode];_dataOrbitalSurvey.Status.SetValue(LocalizationStrings.STATUS[Status.Idle]);
        
        var altitude = vessel.Model.AltitudeFromRadius;
        var state = ScanUtility.GetAltitudeState(altitude, _dataOrbitalSurvey.ScanningStats);
        
        // Update Status
        if (map.IsFullyScanned)
        {
            _dataOrbitalSurvey.Status.SetValue(LocalizationStrings.STATUS[Status.Complete]);
        }
        else if (!_dataOrbitalSurvey.HasResourcesToOperate)
        {
            _dataOrbitalSurvey.Status.SetValue(LocalizationStrings.STATUS[Status.NoPower]);
        }
        else if (state is State.BelowMin or State.AboveMax)
        {
            _dataOrbitalSurvey.Status.SetValue(LocalizationStrings.STATUS[Status.Idle]);
        }
        else if (((PartComponentModule_OrbitalSurvey)ComponentModule).DataDeployable?.IsExtended == false)
        {
            _dataOrbitalSurvey.Status.SetValue(LocalizationStrings.STATUS[Status.NotDeployed]);
        }
        else
        {
            _dataOrbitalSurvey.Status.SetValue(LocalizationStrings.STATUS[Status.Scanning]);
        }
        
        // Update State
        _dataOrbitalSurvey.State.SetValue(LocalizationStrings.STATE[state]);
        
        // Update PercentComplete
        _dataOrbitalSurvey.PercentComplete.SetValue(map.PercentDiscovered);

        PerformDebugChecks();
    }

    // This... also triggers when Flight scene is loaded? (why?)
    // It triggers when exiting the game also.
    public override void OnShutdown()
    {
        _LOGGER.LogDebug($"OnShutdown triggered.");
        _dataOrbitalSurvey.EnabledToggle.OnChangedValue -= OnToggleChangedValue;
    }

    private void OnToggleChangedValue(bool newValue)
    {
        _LOGGER.LogDebug($"OnToggleChangedValue triggered. New value is {newValue.ToString()}");
        ((PartComponentModule_OrbitalSurvey)ComponentModule).ResetLastScanTime();
        
        UpdateFlightPAMVisibility(newValue);

        _dataOrbitalSurvey.Status.SetValue(
            newValue ? LocalizationStrings.STATUS[Status.Scanning] : LocalizationStrings.STATUS[Status.Disabled]);
    }

    private void OnOpenMapClicked()
    {
        var mode = Enum.Parse<MapType>(_dataOrbitalSurvey.Mode.GetValue());
        var body = vessel.Model.mainBody.Name;

        SceneController.Instance.SelectedMapType = mode;
        SceneController.Instance.SelectedBody = body;
        SceneController.Instance.ToggleUI(true);
    }

    private void UpdateFlightPAMVisibility(bool state)
    {
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.Status, true);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.Mode, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.State, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.ScanningFieldOfView, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.MinimumAltitude, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.IdealAltitude, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.MaximumAltitude, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.PercentComplete, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.ScanningFieldOfViewDebug, false);
        _dataOrbitalSurvey.SetVisible(_actionOpenGui, state);
        _dataOrbitalSurvey.SetVisible(_triggerExperiment, false);
        _dataOrbitalSurvey.SetVisible(_triggerScienceReport, false);
    }

    private void UpdateOabPAMVisibility()
    {
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.Status, false);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.Mode, true);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.State, false);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.ScanningFieldOfView, true);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.MinimumAltitude, true);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.IdealAltitude, true);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.MaximumAltitude, true);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.PercentComplete, false);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.ScanningFieldOfViewDebug, false);
        _dataOrbitalSurvey.SetVisible(_actionOpenGui, false);
        _dataOrbitalSurvey.SetVisible(_triggerExperiment, false);
        _dataOrbitalSurvey.SetVisible(_triggerScienceReport, false);
    }
    
    // Debugging actions
    private void OnTriggerExperiment()
    {
        ComponentModule.Part.TryGetModule(typeof(PartComponentModule_ScienceExperiment), out var m);
        PartComponentModule_ScienceExperiment module = m as PartComponentModule_ScienceExperiment;

        var experiment =
            module.dataScienceExperiment.ExperimentStandings.Find(
                e => e.ExperimentID.StartsWith("orbital_survey_visual_mapping"));
        
        experiment.CurrentExperimentState = ExperimentState.RUNNING;
        experiment.ConditionMet = true;
    }

    private void OnCreateScienceReport()
    {
        ComponentModule.Part.TryGetModule(typeof(PartComponentModule_ScienceExperiment), out var m);
        PartComponentModule_ScienceExperiment module = m as PartComponentModule_ScienceExperiment;

        var experiment =
            module.dataScienceExperiment.ExperimentStandings.Find(
                e => e.ExperimentID.StartsWith("orbital_survey_visual_mapping"));

        var expDef = module.GetExperimentDefinitionByID("orbital_survey_visual_mapping_high_25");

        module.CreateScienceReports(expDef, 0);
    }
    
    // This triggers always
    public override void OnUpdate(float deltaTime)
    {
        //_logger.LogDebug("OnUpdate triggered.");
    }

    // This triggers in OAB
    public override void OnModuleOABFixedUpdate(float deltaTime)
    {
        //_logger.LogDebug("OnModuleOABFixedUpdate triggered.");
    }
    
    // This triggers when Flight scene is loaded
    // And it also triggers in OAB when part is added to the assembly? 
    protected void OnEnable()
    {
        //_LOGGER.LogDebug($"OnEnable triggered.");
    }
    
    private void PerformDebugChecks()
    {
        // Debug FOV
        if (DebugUI.Instance.DebugFovEnabled != _isDebugFovEnabled)
        {
            _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.ScanningFieldOfViewDebug, DebugUI.Instance.DebugFovEnabled);
            _isDebugFovEnabled = DebugUI.Instance.DebugFovEnabled;
        }
        
        // Debug Trigger Experiment
        if (DebugUI.Instance.DebugTriggerExperiment != _isDebugTriggerExperimentVisible)
        {
            _dataOrbitalSurvey.SetVisible(_triggerExperiment, DebugUI.Instance.DebugTriggerExperiment);
            _isDebugTriggerExperimentVisible = DebugUI.Instance.DebugTriggerExperiment;
        }
        
        // Debug Trigger Science Report
        if (DebugUI.Instance.DebugTriggerScienceReport != _isDebugTriggerScienceReportVisible)
        {
            _dataOrbitalSurvey.SetVisible(_triggerScienceReport, DebugUI.Instance.DebugTriggerScienceReport);
            _isDebugTriggerScienceReportVisible = DebugUI.Instance.DebugTriggerScienceReport;
        }
    }
    
    private void HideScienceExperimentPamProperties()
    {
        ComponentModule.Part.TryGetModule(typeof(PartComponentModule_ScienceExperiment), out var m);
        PartComponentModule_ScienceExperiment module = m as PartComponentModule_ScienceExperiment;

        var data = module.dataScienceExperiment;
        data.SetVisible(data.Location, false);
    }
    
    #region NOT USED
    
    // METHODS THAT DON'T TRIGGER
    
    // -
    public override void OnModuleUpdate(float deltaTime)
    {
        _LOGGER.LogDebug("OnModuleUpdate triggered.");
    }

    // -
    public override void OnModuleOABUpdate(float deltaTime)
    {
        _LOGGER.LogDebug("OnModuleOABUpdate triggered.");
    }
    
    // -
    public new void OnFixedUpdate(float deltaTime)
    {
        _LOGGER.LogDebug("OnFixedUpdate triggered.");
    }
    
    // What does this do?
    [ContextMenu("Extend")]
    protected virtual bool Extend()
    {
        _LOGGER.LogDebug("Extend triggered.");
        return true;
    }
    
    public override string GetModuleDisplayName() => "This is GetModuleDisplayName()";
    
    #endregion
}