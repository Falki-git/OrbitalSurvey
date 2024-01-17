using BepInEx.Logging;
using JetBrains.Annotations;
using KSP.Sim.impl;
using OrbitalSurvey.Models;
using OrbitalSurvey.Modules;
using OrbitalSurvey.UI;
using OrbitalSurvey.Utilities;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace OrbitalSurvey.Managers;

/// <summary>
/// Used to keep track of vessels with OrbitalSurvey module for the purpose of displaying them in the GUI
/// </summary>
public class VesselManager : MonoBehaviour
{
    public static VesselManager Instance { get; set; }

    public List<VesselStats> OrbitalSurveyVessels = new();
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.VesselManager");

    public double LastRefreshTime;
    private double _timeSinceLastRefresh => Utility.UT - LastRefreshTime;

    private bool _isVisualModuleUpdatedThisLoop;
    private bool _isBiomeModuleUpdatedThisLoop;
    
    public delegate void VesselRegistered(VesselStats vesselStats);
    public delegate void VesselUnRegistered(VesselStats vesselStats);
    public delegate void VisualModuleChanged(VesselStats vessel, ModuleStats module);
    public delegate void BiomeModuleChanged(VesselStats vessel, ModuleStats module);
    public delegate void VesselChangedBodies(VesselStats vessel);
    public event VesselRegistered OnVesselRegistered;
    public event VesselUnRegistered OnVesselUnRegistered;
    public event VisualModuleChanged OnVisualModuleChanged;
    public event BiomeModuleChanged OnBiomeModuleChanged;
    public event VesselChangedBodies OnVesselChangedBody;
    
    private void Start()
    {
        Instance = this;
    }
    
    public void SetLastRefreshTimeToNow() => LastRefreshTime = Utility.UT;

    [PublicAPI]
    public void RegisterModule(VesselComponent vessel, Data_OrbitalSurvey dataModule)
    {
        if (vessel == null)
        {
            _LOGGER.LogError($"Vessel is 'null'. Registration cannot proceed!");
            return;
        }
        
        if (dataModule == null)
        {
            _LOGGER.LogError($"Data_OrbitalSurvey module for Vessel {vessel.Name} is 'null'. Registration cannot proceed!");
            return;
        }
        
        var moduleStats = new ModuleStats()
        {
            DataModule = dataModule,
            Enabled = dataModule.EnabledToggle.GetValue(),
            Status = dataModule.StatusValue,
            State = dataModule.StateValue,
            Mode = LocalizationStrings.MODE_TYPE_TO_MAP_TYPE[dataModule.ModeValue],
        };

        VesselStats vesselStats;
        bool isNewVessel = false;
        
        // check if vessel already exists (case: multiple modules on the same vessel)
        var existingVesselStats = OrbitalSurveyVessels.Find(vesselStats => vesselStats.Vessel == vessel);
        if (existingVesselStats != null)
        {
            // existing vessel
            vesselStats = existingVesselStats;
        }
        else
        {
            // new vessel
            vesselStats = new VesselStats()
            {
                Vessel = vessel,
                Name = vessel.Name,
                Body = vessel.Orbit.referenceBody.bodyName,
                GeographicCoordinates = (vessel.Latitude, vessel.Longitude)
            };
            OrbitalSurveyVessels.Add(vesselStats);
            isNewVessel = true;
        }
        
        vesselStats.ModuleStats.Add(moduleStats);

        if (isNewVessel)
        {
            OnVesselRegistered?.Invoke(vesselStats);
            _LOGGER.LogInfo($"New vessel '{vesselStats.Name}' registered. Number of modules: {vesselStats.ModuleStats.Count}.");
        }
    }
    
    /// <summary>
    /// This doesn't appear to be working since the vessel is null at the point when Unregister needs to be called
    /// </summary>
    [Obsolete]
    [PublicAPI]
    public void UnRegisterModule(VesselComponent vessel, Data_OrbitalSurvey dataModule)
    {
        if (vessel == null)
        {
            _LOGGER.LogError($"Vessel is null! Unregistration will not continue.");
            return;
        }
        
        var foundVessel = OrbitalSurveyVessels.Find(vesselStats => vesselStats.Vessel == vessel);
        if (foundVessel == null)
        {
            _LOGGER.LogError($"Vessel '{vessel.Name}' is not registered! Unregistration will not continue.");
            return;
        }

        var foundModule = foundVessel.ModuleStats.Find(moduleStats => moduleStats.DataModule == dataModule);
        if (foundModule == null)
        {
            _LOGGER.LogError($"Data_OrbitalSurvey module on vessel '{vessel.Name}' is not registered! Unregistration will not continue.");
            return;
        }

        if (foundVessel.ModuleStats.Count > 1)
        {
            // there are more than 1 modules on this vessel (case: part was destroyed)
            foundVessel.ModuleStats.Remove(foundModule);
            _LOGGER.LogInfo($"Data_OrbitalSurvey module on vessel '{vessel.Name}' has been unregistered.");
        }
        else
        {
            // only one module is on this vessel, need to unregister the entire vessel
            UnRegisterVessel(foundVessel);
        }
    }

    public void UnRegisterVessel(VesselStats vesselStats)
    {
        OrbitalSurveyVessels.Remove(vesselStats);
        OnVesselUnRegistered?.Invoke(vesselStats);
        _LOGGER.LogInfo($"Vessel '{vesselStats.Name}' has been unregistered.");
    }
    
    private void Update()
    {
        if (!SceneController.Instance.ShowMainGui ||
            _timeSinceLastRefresh < (double)Settings.GuiRefreshInterval.Value)
            return;

        // go through the list of all vessels
        for (int i = 0; i < OrbitalSurveyVessels.Count; i++)
        {
            var vesselStats = OrbitalSurveyVessels[i];

            _isVisualModuleUpdatedThisLoop = false;
            _isBiomeModuleUpdatedThisLoop = false;
            
            // check if vessel is destroyed/recovered
            if (vesselStats.Vessel == null || vesselStats.Vessel.Name == "NULL")
            {
                UnRegisterVessel(vesselStats);
                i--;
                continue;
            }
            
            // vessel is not destroyed, update vessel values
            vesselStats.Name = vesselStats.Vessel.Name;
            if (vesselStats.Body != vesselStats.Vessel.Orbit.referenceBody.bodyName)
            {
                vesselStats.Body = vesselStats.Vessel.Orbit.referenceBody.bodyName;
                OnVesselChangedBody?.Invoke(vesselStats);
                _LOGGER.LogInfo($"Vessel '{vesselStats.Name}' changed reference body to '{vesselStats.Body}'.");
            }
            vesselStats.GeographicCoordinates = (vesselStats.Vessel.Latitude, vesselStats.Vessel.Longitude);
            vesselStats.MapLocationPercent = ScanUtility.GetMapGuiCoordinatesFromGeographicCoordinates(
                vesselStats.Vessel.Latitude, vesselStats.Vessel.Longitude);

            // go through the list of all modules. Each vessel can have multiple parts, hence multiple modules
            for (int j = 0; j < vesselStats.ModuleStats.Count; j++)
            {
                var moduleStats = vesselStats.ModuleStats[j];
                
                // check if module is destroyed (case: part was destroyed)
                if (moduleStats.DataModule == null) // TODO check if there's another condition, like the above vessel.Name == "NULL"
                {
                    vesselStats.ModuleStats.Remove(moduleStats);

                    // check if this was the last module. If so, unregister the entire vessel
                    if (vesselStats.ModuleStats.Count == 0)
                    {
                        UnRegisterVessel(vesselStats);
                        i--;
                        break;
                    }
                    
                    j--;
                    continue;
                }
                
                // check if module is still attached to this vessel (case: decoupled or undocked from the original vessel)
                var moduleVessel = moduleStats.DataModule.PartComponentModule.Part.PartOwner.SimulationObject.Vessel;
                if (moduleVessel != vesselStats.Vessel)
                {
                    // module's current vessel isn't the one that is registered
                    // need to register the new vessel and...
                    // unregister this module from the previous vessel and...
                    // unregister the previous vessel itself if this was the last module on it... oh boy
                    
                    // register the new vessel
                    RegisterModule(moduleVessel, moduleStats.DataModule);
                    
                    // unregister the module from the previous vessel
                    vesselStats.ModuleStats.Remove(moduleStats);
                    
                    // check if this was the last module. If so, unregister the entire previous vessel
                    if (vesselStats.ModuleStats.Count == 0)
                    {
                        UnRegisterVessel(vesselStats);
                        
                        // this was the last module; break and continue with the next vessel
                        i--;
                        break;
                    }
                    
                    // continue with the next module
                    j--;
                    continue;
                }
                
                // part/module is not destroyed and it's still attached to the same vessel, update module values
                if (moduleStats.DataModule.EnabledToggle.GetValue() != moduleStats.Enabled)
                {
                    moduleStats.Enabled = moduleStats.DataModule.EnabledToggle.GetValue();
                    
                    if (moduleStats.Mode == MapType.Visual)
                    {
                        _isVisualModuleUpdatedThisLoop = true;
                    }
                    else if (moduleStats.Mode == MapType.Biome)
                    {
                        _isBiomeModuleUpdatedThisLoop = true;
                    }
                }
                if (moduleStats.DataModule.StatusValue != moduleStats.Status)
                {
                    moduleStats.Status = moduleStats.DataModule.StatusValue;
                    
                    if (moduleStats.Mode == MapType.Visual)
                    {
                        _isVisualModuleUpdatedThisLoop = true;
                    }
                    else if (moduleStats.Mode == MapType.Biome)
                    {
                        _isBiomeModuleUpdatedThisLoop = true;
                    }
                }
                if (moduleStats.DataModule.StateValue != moduleStats.State)
                {
                    moduleStats.State = moduleStats.DataModule.StateValue;
                    
                    if (moduleStats.Mode == MapType.Visual)
                    {
                        _isVisualModuleUpdatedThisLoop = true;
                    }
                    else if (moduleStats.Mode == MapType.Biome)
                    {
                        _isBiomeModuleUpdatedThisLoop = true;
                    }
                }
            }

            // VISUAL: check if a visual module was updated this loop and then invoke the change for the first module found
            if (_isVisualModuleUpdatedThisLoop)
            {
                var visualModule = vesselStats.ModuleStats.Find(m => m.Mode == MapType.Visual);
                vesselStats.InvokeVisualModuleChanged(visualModule);
            }
            
            // BIOME: check if a module was updated this loop and then invoke the change for the first module found
            if (_isBiomeModuleUpdatedThisLoop)
            {
                var biomeModule = vesselStats.ModuleStats.Find(m => m.Mode == MapType.Biome);
                vesselStats.InvokeBiomeModuleChanged(biomeModule);
            }
        }
        
        SetLastRefreshTimeToNow();
    }

    public void ClearAllSubscriptions()
    {
        OnVesselRegistered = null;
        OnVesselUnRegistered = null;
        OnVisualModuleChanged = null;
        OnBiomeModuleChanged = null;
        OnVesselChangedBody = null;

        foreach (var vessel in OrbitalSurveyVessels)
        {
            vessel.ClearAllSubscriptions();
        }
    }
    
    
    public class VesselStats
    {
        public VesselComponent Vessel;
        public List<ModuleStats> ModuleStats = new();
        public bool IsActiveVessel => Vessel.SimulationObject.IsActiveVessel;

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnNameChanged?.Invoke(_name);
                }
            }
        }

        private string _body;
        public string Body // => Vessel.Orbit.referenceBody.bodyName;
        {
            get => _body;
            set
            {
                if (_body != value)
                {
                    _body = value;
                    OnBodyChanged?.Invoke(_body);
                }
            }
        }
        
        private (double latitude, double longitude) _geographicCoordinates;
        public (double Latitude, double Longitude) GeographicCoordinates
        {
            get => _geographicCoordinates;
            set
            {
                if (_geographicCoordinates != value)
                {
                    _geographicCoordinates = value;
                    OnGeographicCoordinatesChanged?.Invoke(_geographicCoordinates);
                }
            }
        }
        
        private (float percentX, float percentY) _mapLocationPercent;
        public (float PercentX, float PercentY) MapLocationPercent
        {
            get => _mapLocationPercent;
            set
            {
                if (_mapLocationPercent != value)
                {
                    _mapLocationPercent = value;
                    OnMapGuiPositionChanged?.Invoke(_mapLocationPercent);
                }
            }
        }

        public void InvokeVisualModuleChanged(ModuleStats module) => OnVisualModuleChanged?.Invoke(module);
        public void InvokeBiomeModuleChanged(ModuleStats module) => OnBiomeModuleChanged?.Invoke(module);
        
        public delegate void NameChanged(string name);
        public delegate void BodyChanged(string body);
        public delegate void GeographicCoordinatesChanged((double Latitude, double Longitude) geographicCoordinates);
        public delegate void MapGuiPositionChanged((float percentX, float percentY) mapGuiPositionChanged);
        public delegate void VisualModuleChanged(ModuleStats module);
        public delegate void BiomeModuleChanged(ModuleStats module);
        public event NameChanged OnNameChanged;
        public event BodyChanged OnBodyChanged;
        public event GeographicCoordinatesChanged OnGeographicCoordinatesChanged;
        public event MapGuiPositionChanged OnMapGuiPositionChanged;
        public event VisualModuleChanged OnVisualModuleChanged;
        public event BiomeModuleChanged OnBiomeModuleChanged;

        public void ClearAllSubscriptions()
        {
            OnGeographicCoordinatesChanged = null;
            OnMapGuiPositionChanged = null;
            OnVisualModuleChanged = null;
            OnBiomeModuleChanged = null;

            foreach (var module in ModuleStats)
            {
                module.ClearAllSubscriptions();
            }
        }
    }

    public class ModuleStats
    {
        public Data_OrbitalSurvey DataModule;
        
        private bool _enabled;
        public bool Enabled // => DataModule.EnabledToggle.GetValue();
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnEnabledChanged?.Invoke(_enabled);
                }
            }
        }
        
        private MapType _mode;
        public MapType Mode // => LocalizationStrings.MODE_TYPE_TO_MAP_TYPE[DataModule.ModeValue];
        {
            get;
            set;
        }

        private Status _status;
        public Status Status // => DataModule.StatusValue;
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnStatusChanged?.Invoke(_status);
                }
            }
        }
        
        private State _state;
        public State State // => DataModule.StateValue;
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnStateChanged?.Invoke(_state);
                }
            }
        }
        
        public delegate void EnabledChanged(bool enabled);
        //public delegate void ModeChanged(MapType mode); // Mode cannot be changed
        public delegate void StatusChanged(Status status);
        public delegate void StateChanged(State state);
        public event EnabledChanged OnEnabledChanged;
        //public event ModeChanged OnModeChanged; // Mode cannot be changed
        public event StatusChanged OnStatusChanged;
        public event StateChanged OnStateChanged;

        public void ClearAllSubscriptions()
        {
            OnEnabledChanged = null;
            OnStatusChanged = null;
            OnStateChanged = null;
        }
    }
}

