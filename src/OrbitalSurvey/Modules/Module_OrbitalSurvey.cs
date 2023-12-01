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

    public override void AddDataModules()
    {
        base.AddDataModules();
        _dataOrbitalSurvey ??= new Data_OrbitalSurvey();
        DataModules.TryAddUnique(_dataOrbitalSurvey, out _dataOrbitalSurvey);
    }

    public override void OnInitialize()
    {
        base.OnInitialize();

        _dataOrbitalSurvey.Mode.OnChangedValue += OnModeChanged;

        if (PartBackingMode == PartBackingModes.Flight)
        {
            moduleIsEnabled = true; // this doesn't appear to have any purpose...
            _dataOrbitalSurvey.EnabledToggle.OnChangedValue += OnToggleChangedValue;
        }
        
        UpdatePAMVisibility(_dataOrbitalSurvey.EnabledToggle.GetValue());
        
        UpdateValues(_dataOrbitalSurvey.Mode.GetValue());
        
        // colors that work: red, yellow, grey, white, blue, black, green, lightblue
        // _dataOrbitalSurvey.Status.SetValue("Scanning...");
        _dataOrbitalSurvey.Status.SetValue(StatusStrings.Status["Scanning"]);
        
        //_dataOrbitalSurvey.State.SetValue("<color=yellow>Below ideal alt \u26a0</color>");
        _dataOrbitalSurvey.State.SetValue(StatusStrings.State["AboveMax"]);
        
        
        
        // _testAction = new ModuleAction(TestAction);
        // _dataOrbitalSurvey.AddAction("Enable Orbital Survey", _testAction);
        // var isVisible = base.part != null;
        //_dataOrbitalSurvey.SetVisible(_testAction, isVisible);

        //_dataOrbitalSurvey.MyModulePropertyTest.SetValue("setting some value");
        //_dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.MyModulePropertyTest, true);
        // _dataOrbitalSurvey.SetLabel(_dataOrbitalSurvey.MyModulePropertyTest, "new label");
        // _dataOrbitalSurvey.MyModulePropertyTest.SetValue("new value");

        // _moduleProperty = new ModuleProperty<string>("Hello World!");
        // _dataOrbitalSurvey.AddProperty("MyModulePropertyTest", _moduleProperty);
        // _dataOrbitalSurvey.SetVisible(_moduleProperty, true);
        // _dataOrbitalSurvey.MyModulePropertyTest.SetValue(());
    }
    
    public override string GetModuleDisplayName() => "This is GetModuleDisplayName()";
    
    private ModuleAction _testAction;
    private ModuleProperty<string> _moduleProperty;
    
    private void TestAction()
    {
        _logger.LogDebug("Hello World");
    }

    // This triggers always
    public override void OnUpdate(float deltaTime)
    {
        //_logger.LogDebug("OnUpdate triggered.");
        int i = 0;
    }
    
    // This triggers in flight
    public override void OnModuleFixedUpdate(float fixedDeltaTime)
    {
        if (!Core.Instance.MapsInitialized)
            return;
        
        var mode = Enum.Parse<MapType>(_dataOrbitalSurvey.Mode.GetValue());
        var body = vessel.Model.mainBody.Name;
        var map = Core.Instance.CelestialDataDictionary[body].Maps[mode];
        var percentDiscovered = (float)map.DiscoveredPixelsCount / map.TotalPixelCount;
        _dataOrbitalSurvey.PercentComplete.SetValue(percentDiscovered.ToString());
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
        _logger.LogDebug("OnEnable triggered.");
    }
    
    // This... also triggers when Flight scene is loaded? (why?)
    // It triggers when exiting the game also.
    public override void OnShutdown()
    {
        _logger.LogDebug("OnShutdown triggered.");
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
        UpdatePAMVisibility(newValue);
    }

    private void UpdatePAMVisibility(bool state)
    {
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.Mode, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.State, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.ScanningFieldOfView, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.MinimumAltitude, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.IdealAltitude, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.MaximumAltitude, state);
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.PercentComplete, state);
    }
    
    private void UpdateValues(string newMode)
    {
        var mapType = Enum.Parse<MapType>(newMode);
        
        switch (mapType)
        {
            case MapType.Visual:
                _dataOrbitalSurvey.MinimumAltitude.SetValue((Settings.VisualMinAltitude / 1000).ToString());
                _dataOrbitalSurvey.IdealAltitude.SetValue((Settings.VisualIdealAltitude / 1000).ToString());
                _dataOrbitalSurvey.MaximumAltitude.SetValue((Settings.VisualMaxAltitude / 1000).ToString());
                break;
            case MapType.Biome:
                _dataOrbitalSurvey.MinimumAltitude.SetValue((Settings.BiomeMinAltitude / 1000).ToString());
                _dataOrbitalSurvey.IdealAltitude.SetValue((Settings.BiomeIdealAltitude / 1000).ToString());
                _dataOrbitalSurvey.MaximumAltitude.SetValue((Settings.BiomeMaxAltitude / 1000).ToString());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    
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
}