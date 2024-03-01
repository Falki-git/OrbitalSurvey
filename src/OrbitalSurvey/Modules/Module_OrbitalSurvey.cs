using BepInEx.Logging;
using KSP.Game;
using KSP.Game.Science;
using KSP.Messages;
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
        _LOGGER.LogDebug($"OnInitialize triggered. Vessel '{_part?.partOwner?.SimObjectComponent?.Name ?? "n/a"}'.");
        
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
                _dataOrbitalSurvey.StatusValue = Status.Disabled;
            }
            
            HideScienceExperimentPamProperties();    
        }
        else if (PartBackingMode == PartBackingModes.OAB)
        {
            UpdateOabPAMVisibility();
            _dataOrbitalSurvey.BodyCategoryOabDropdown.OnChangedValue += OnBodyCategoryOabChanged;
        }
    }

    // This triggers in flight
    public override void OnModuleFixedUpdate(float fixedDeltaTime)
    {
        if (!Core.Instance.MapsInitialized || !_dataOrbitalSurvey.EnabledToggle.GetValue())
            return;
        
        // Logic for updating Status, State and MapPercentage moved to PartComponentModule_OrbitalSurvey.UpdateStatusAndState
        
        PerformDebugChecks();
    }
    
    private void OnToggleChangedValue(bool newValue)
    {
        _LOGGER.LogDebug($"OnToggleChangedValue triggered. New value is {newValue.ToString()}");
        ((PartComponentModule_OrbitalSurvey)ComponentModule).ResetLastScanTime();
        
        UpdateFlightPAMVisibility(newValue);

        _dataOrbitalSurvey.StatusValue = newValue ? Status.Scanning : Status.Disabled;

        SetInitialBodyCategoryValues();
    }

    private void SetInitialBodyCategoryValues()
    {
        var body = ComponentModule.Part.PartOwner.SimulationObject.Vessel.mainBody.Name;
        var mapType = LocalizationStrings.MODE_TYPE_TO_MAP_TYPE[_dataOrbitalSurvey.ModeValue];
        var stats = CelestialCategoryManager.Instance.GetScanningStats(body, mapType);
        _dataOrbitalSurvey.SetScanningStats(body, stats.category, stats.altitudes);
    }

    private void OnOpenMapClicked()
    {
        var mode = LocalizationStrings.MODE_TYPE_TO_MAP_TYPE[_dataOrbitalSurvey.ModeValue];
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
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.BodyCategory, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.BodyCategoryOabDropdown, false);
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
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.BodyCategory, false);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.BodyCategoryOabDropdown, true);
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

        string body = ComponentModule.Part.PartOwner.SimulationObject.Vessel.mainBody.Name;
        
        var experimentDefinition =
            GameManager.Instance.Game.ScienceManager.ScienceExperimentsDataStore.GetExperimentDefinition("orbital_survey_visual_mapping_high_25");

        var celestialScalar = GameManager.Instance.Game.ScienceManager.ScienceRegionsDataProvider
            ._cbToScienceRegions[body].SituationData.CelestialBodyScalar;
        var highOrbitScalar = GameManager.Instance.Game.ScienceManager.ScienceRegionsDataProvider.
            _cbToScienceRegions[body].SituationData.HighOrbitScalar;
        
        ResearchReport researchReport = new ResearchReport(
            experimentID: experimentDefinition.ExperimentID,
            displayName: experimentDefinition.DataReportDisplayName,
            module._currentLocation,
            ScienceReportType.DataType,
            initialScienceValue: experimentDefinition.DataValue * celestialScalar * highOrbitScalar,
            flavorText: experimentDefinition.DataFlavorDescriptions[0].LocalizationTag
            );
        
        module._storageComponent.StoreResearchReport(researchReport);
        
        ResearchReportAcquiredMessage message;
        if (GameManager.Instance.Game.Messages.TryCreateMessage(out message))
        {
            GameManager.Instance.Game.Messages.Publish(message);
        }

        NotificationUtility.Instance.NotifyExperimentComplete(
            module.Part.PartOwner.SimulationObject.Orbit.referenceBody.Name,
            ExperimentLevel.Quarter
        );
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
    
    private void OnBodyCategoryOabChanged(string category)
    {
        _dataOrbitalSurvey.SetOabScanningStats(category);
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
    
    // This... also triggers when Flight scene is loaded? (why?)
    // It triggers when exiting the game also.
    // It triggers for only active vessel it appears
    public override void OnShutdown()
    {
        _LOGGER.LogDebug($"OnShutdown triggered. Vessel '{part?.partOwner?.SimObjectComponent?.Name ?? "n/a"}'");
        _dataOrbitalSurvey.EnabledToggle.OnChangedValue -= OnToggleChangedValue;
        _dataOrbitalSurvey.BodyCategoryOabDropdown.OnChangedValue -= OnBodyCategoryOabChanged;
    }
    
    #region NOT USED
    
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