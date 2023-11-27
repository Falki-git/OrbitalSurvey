using BepInEx.Logging;
using KSP.Sim.Definitions;
using OrbitalSurvey.Utilities;
using UnityEngine;
using UnityEngine.PlayerLoop;

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

    // This triggers in flight
    public override void OnModuleFixedUpdate(float fixedDeltaTime)
    {
        //_logger.LogDebug("OnModuleFixedUpdate triggered.");
    }

    // This triggers in OAB
    public override void OnModuleOABFixedUpdate(float deltaTime)
    {
        //_logger.LogDebug("OnModuleOABFixedUpdate triggered.");
    }

    public void OnFixedUpdate(float deltaTime) // -
    {
        _logger.LogDebug("OnFixedUpdate triggered.");
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
    
    [ContextMenu("Extend")]
    protected virtual bool Extend()
    {
        _logger.LogDebug("Extend triggered.");
        return true;
    }

    private void OnModeChanged(string newMode)
    {
        _logger.LogDebug(($"Mode.OnChangedValue triggered. New value is {newMode}"));
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
        _dataOrbitalSurvey.SetVisible(_dataOrbitalSurvey.ScanningFieldOfView, state);
    }
}