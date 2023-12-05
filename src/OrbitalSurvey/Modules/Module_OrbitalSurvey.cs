using BepInEx.Logging;
using KSP.Sim.Definitions;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;
using UnityEngine;

namespace OrbitalSurvey.Modules;

[DisallowMultipleComponent]
public class Module_OrbitalSurvey : PartBehaviourModule
{
    private static readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.Module");
    
    public override Type PartComponentModuleType => typeof(PartComponentModule_OrbitalSurvey);
    
    [SerializeField]
    protected Data_OrbitalSurvey _dataOrbitalSurvey;

    private bool _isDebugFovEnabled;

    public override void AddDataModules()
    {
        base.AddDataModules();
        _dataOrbitalSurvey ??= new Data_OrbitalSurvey();
        DataModules.TryAddUnique(_dataOrbitalSurvey, out _dataOrbitalSurvey);
    }

    public override void OnInitialize()
    {
        _logger.LogDebug("OnInitialized triggered.");
        base.OnInitialize();

        _dataOrbitalSurvey.Mode.OnChangedValue += OnModeChanged;

        if (PartBackingMode == PartBackingModes.Flight)
        {
            moduleIsEnabled = true; // this doesn't appear to have any purpose...
            _dataOrbitalSurvey.EnabledToggle.OnChangedValue += OnToggleChangedValue;
            
            var isEnabled = _dataOrbitalSurvey.EnabledToggle.GetValue();
        
            UpdateFlightPAMVisibility(isEnabled);
        
            if (!isEnabled)
                _dataOrbitalSurvey.Status.SetValue(StatusStrings.STATUS[Status.Disabled]);
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
        if (!Core.Instance.MapsInitialized)
            return;
        
        if (!_dataOrbitalSurvey.EnabledToggle.GetValue())
            return;
        
        var mode = Enum.Parse<MapType>(_dataOrbitalSurvey.Mode.GetValue());
        var body = vessel.Model.mainBody.Name;
        var map = Core.Instance.CelestialDataDictionary[body].Maps[mode];
        
        var altitude = vessel.Model.AltitudeFromRadius;
        var state = ScanUtility.GetAltitudeState(mode, altitude);
        
        // Update Status
        if (map.IsFullyScanned)
        {
            _dataOrbitalSurvey.Status.SetValue(StatusStrings.STATUS[Status.Complete]);
        }
        else if (state is State.BelowMin or State.AboveMax)
        {
            _dataOrbitalSurvey.Status.SetValue(StatusStrings.STATUS[Status.Idle]);
        }
        else
        {
            _dataOrbitalSurvey.Status.SetValue(StatusStrings.STATUS[Status.Scanning]);
        }
        
        // Update State
        _dataOrbitalSurvey.State.SetValue(StatusStrings.STATE[state]);
        
        // Update PercentComplete
        _dataOrbitalSurvey.PercentComplete.SetValue(map.PercentDiscovered);

        PerformDebugChecks();
    }

    // This... also triggers when Flight scene is loaded? (why?)
    // It triggers when exiting the game also.
    public override void OnShutdown()
    {
        _logger.LogDebug($"OnShutdown triggered.");
        _dataOrbitalSurvey.Mode.OnChangedValue -= OnModeChanged;
        _dataOrbitalSurvey.EnabledToggle.OnChangedValue -= OnToggleChangedValue;
    }

    private void OnModeChanged(string newMode)
    {
        _logger.LogDebug(($"Mode.OnChangedValue triggered. New value is {newMode}"));
        UpdateValues(newMode);
    }

    private void OnToggleChangedValue(bool newValue)
    {
        _logger.LogDebug($"OnToggleChangedValue triggered. New value is {newValue.ToString()}");
        ((PartComponentModule_OrbitalSurvey)ComponentModule).LastScanTime = ScanUtility.UT;
        
        UpdateFlightPAMVisibility(newValue);

        _dataOrbitalSurvey.Status.SetValue(
            newValue ? StatusStrings.STATUS[Status.Scanning] : StatusStrings.STATUS[Status.Disabled]);
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
    }
    
    private void UpdateValues(string newMode)
    {
        var mapType = Enum.Parse<MapType>(newMode);
        
        switch (mapType)
        {
            case MapType.Visual:
                _dataOrbitalSurvey.ScanningFieldOfView.SetValue(Settings.VisualFOV);
                _dataOrbitalSurvey.MinimumAltitude.SetValue((int)(Settings.VisualMinAltitude / 1000));
                _dataOrbitalSurvey.IdealAltitude.SetValue((float)Settings.VisualIdealAltitude / 1000);
                _dataOrbitalSurvey.MaximumAltitude.SetValue((float)Settings.VisualMaxAltitude / 1000);
                break;
            case MapType.Biome:
                _dataOrbitalSurvey.ScanningFieldOfView.SetValue(Settings.BiomeFOV);
                _dataOrbitalSurvey.MinimumAltitude.SetValue((int)(Settings.BiomeMinAltitude / 1000));
                _dataOrbitalSurvey.IdealAltitude.SetValue((float)Settings.BiomeIdealAltitude / 1000);
                _dataOrbitalSurvey.MaximumAltitude.SetValue((float)Settings.BiomeMaxAltitude / 1000);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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
        _logger.LogDebug($"OnEnable triggered.");
    }
    
    private void PerformDebugChecks()
    {
        if (DEBUG_UI.Instance.DebugFovEnabled && !_isDebugFovEnabled)
        {
            _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.ScanningFieldOfViewDebug, true);
            _isDebugFovEnabled = true;
        }
        
        if (!DEBUG_UI.Instance.DebugFovEnabled && _isDebugFovEnabled)
        {
            _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.ScanningFieldOfViewDebug, false);
            _isDebugFovEnabled = false;
        }
    }
    
    #region NOT USED
    
    // METHODS THAT DON'T TRIGGER
    
    // -
    public override void OnModuleUpdate(float deltaTime)
    {
        _logger.LogDebug("OnModuleUpdate triggered.");
    }

    // -
    public override void OnModuleOABUpdate(float deltaTime)
    {
        _logger.LogDebug("OnModuleOABUpdate triggered.");
    }
    
    // -
    public void OnFixedUpdate(float deltaTime)
    {
        _logger.LogDebug("OnFixedUpdate triggered.");
    }
    
    // What does this do?
    [ContextMenu("Extend")]
    protected virtual bool Extend()
    {
        _logger.LogDebug("Extend triggered.");
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
    
    private ModuleAction _testAction;
    private ModuleProperty<string> _moduleProperty;
    
    private void TestAction()
    {
        _logger.LogDebug("Hello World");
    }
    
    #endregion
}