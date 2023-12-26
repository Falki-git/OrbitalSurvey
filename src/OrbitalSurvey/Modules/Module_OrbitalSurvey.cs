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

    private bool _isDebugFovEnabled;

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
        _dataOrbitalSurvey.AddAction("PartModules/OrbitalSurvey/OpenGui", _actionOpenGui, 6);
        _dataOrbitalSurvey.AddAction("PartModules/OrbitalSurvey/TriggerExperiment", _triggerExperiment, 7);

        _dataOrbitalSurvey.Mode.OnChangedValue += OnModeChanged;

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
        
        UpdateValues(_dataOrbitalSurvey.Mode.GetValue());

        
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
        var state = ScanUtility.GetAltitudeState(mode, altitude);
        
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
        _dataOrbitalSurvey.Mode.OnChangedValue -= OnModeChanged;
        _dataOrbitalSurvey.EnabledToggle.OnChangedValue -= OnToggleChangedValue;
    }

    private void OnModeChanged(string newMode)
    {
        _LOGGER.LogDebug(($"Mode.OnChangedValue triggered. New value is {newMode}"));
        UpdateValues(newMode);
    }

    private void OnToggleChangedValue(bool newValue)
    {
        _LOGGER.LogDebug($"OnToggleChangedValue triggered. New value is {newValue.ToString()}");
        ((PartComponentModule_OrbitalSurvey)ComponentModule).LastScanTime = ScanUtility.UT;
        
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
        _dataOrbitalSurvey.SetVisible(_triggerExperiment, state);
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
    }
    
    private void UpdateValues(string newMode)
    {
        var mapType = Enum.Parse<MapType>(newMode);
        
        _dataOrbitalSurvey.ScanningFieldOfView.SetValue(Settings.ModeScanningStats[mapType].FieldOfView);
        _dataOrbitalSurvey.MinimumAltitude.SetValue(Settings.ModeScanningStats[mapType].MinAltitude / 1000);
        _dataOrbitalSurvey.IdealAltitude.SetValue(Settings.ModeScanningStats[mapType].IdealAltitude / 1000);
        _dataOrbitalSurvey.MaximumAltitude.SetValue(Settings.ModeScanningStats[mapType].MaxAltitude / 1000);

        _dataOrbitalSurvey.RequiredResource.Rate = Settings.EcConsumptionRate[mapType];
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
        if (DebugUI.Instance.DebugFovEnabled && !_isDebugFovEnabled)
        {
            _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.ScanningFieldOfViewDebug, true);
            _isDebugFovEnabled = true;
        }
        
        if (!DebugUI.Instance.DebugFovEnabled && _isDebugFovEnabled)
        {
            _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.ScanningFieldOfViewDebug, false);
            _isDebugFovEnabled = false;
        }
    }
    
    private void HideScienceExperimentPamProperties()
    {
        ComponentModule.Part.TryGetModule(typeof(PartComponentModule_ScienceExperiment), out var m);
        PartComponentModule_ScienceExperiment module = m as PartComponentModule_ScienceExperiment;
        
        //_dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.ScanningFieldOfView, true);

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

    private void TestBed()
    {
        // _testAction = new ModuleAction(TestAction);
        // _dataOrbitalSurvey.AddAction("Enable Orbital Survey", _testAction);
        // var isVisible = base.part != null;
        // _dataOrbitalSurvey.SetVisible(_testAction, isVisible);

        // _dataOrbitalSurvey.MyModulePropertyTest.SetValue("setting some value");
        // _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.MyModulePropertyTest, true);
        // _dataOrbitalSurvey.SetLabel(_dataOrbitalSurvey.MyModulePropertyTest, "new label");
        // _dataOrbitalSurvey.MyModulePropertyTest.SetValue("new value");

        // _moduleProperty = new ModuleProperty<string>("Hello World!");
        // _dataOrbitalSurvey.AddProperty("MyModulePropertyTest", _moduleProperty);
        // _dataOrbitalSurvey.SetVisible(_moduleProperty, true);
        // _dataOrbitalSurvey.MyModulePropertyTest.SetValue(());
        
        
        // colors that work: red, yellow, grey, white, blue, black, green, lightblue
        //_dataOrbitalSurvey.Status.SetValue(StatusStrings.STATUS[Status.Scanning]);
        
        //_dataOrbitalSurvey.State.SetValue("<color=yellow>Below ideal alt \u26a0</color>");
        //_dataOrbitalSurvey.State.SetValue(StatusStrings.STATE[State.AboveMax]);
    }
    
    public override string GetModuleDisplayName() => "This is GetModuleDisplayName()";
    
    /*
    private ModuleAction _testAction;
    private ModuleProperty<string> _moduleProperty;
    */
    
    private void TestAction()
    {
        _LOGGER.LogDebug("Hello World");
    }
    
    #endregion
}