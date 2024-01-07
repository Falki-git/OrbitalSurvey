using BepInEx.Logging;
using I2.Loc;
using KSP.Game;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.ResourceSystem;
using Newtonsoft.Json;
using OrbitalSurvey.Models;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;
// ReSharper disable HeapView.BoxingAllocation

namespace OrbitalSurvey.Modules;

[Serializable]
public class Data_OrbitalSurvey : ModuleData
{
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.Data_OrbitalSurvey");
    
    public override Type ModuleType => typeof(Module_OrbitalSurvey);

    [LocalizedField("PartModules/OrbitalSurvey/Status")]
    [PAMDisplayControl(SortIndex = 1)]  
    public ModuleProperty<string> Status = new ("Idle");
    
    [KSPState]
    [LocalizedField("PartModules/OrbitalSurvey/Enabled")]
    [PAMDisplayControl(SortIndex = 2)]
    public ModuleProperty<bool> EnabledToggle = new ModuleProperty<bool>(false);
    
    [LocalizedField("PartModules/OrbitalSurvey/Mode")] [PAMDisplayControl(SortIndex = 3)]
    public ModuleProperty<string> Mode = new ModuleProperty<string>("");
    
    [LocalizedField("PartModules/OrbitalSurvey/State")]
    [PAMDisplayControl(SortIndex = 4)]  
    public ModuleProperty<string> State = new ("");

    [LocalizedField("PartModules/OrbitalSurvey/PercentComplete")]
    [PAMDisplayControl(SortIndex = 5)]
    [JsonIgnore]
    public ModuleProperty<float> PercentComplete = new (0f, true, val => $"{val:P0}");
    
    [LocalizedField("PartModules/OrbitalSurvey/ScanningFOV")]
    [PAMDisplayControl(SortIndex = 7)]
    [JsonIgnore]
    public ModuleProperty<float> ScanningFieldOfView = new (0, true, val => $"{val:N0}°");
    
    [LocalizedField("PartModules/OrbitalSurvey/MinAltitude")]
    [PAMDisplayControl(SortIndex = 8)]
    [JsonIgnore]
    public ModuleProperty<float> MinimumAltitude = new (0, true, val => $"{((float)val/1000):N0} km");
        
    [LocalizedField("PartModules/OrbitalSurvey/IdealAltitude")]
    [PAMDisplayControl(SortIndex = 9)]
    [JsonIgnore]
    public ModuleProperty<float> IdealAltitude = new (0, true, val => $"{((float)val/1000):N0} km" );
    
    [LocalizedField("PartModules/OrbitalSurvey/MaxAltitude")]
    [PAMDisplayControl(SortIndex = 10)]
    [JsonIgnore]
    public ModuleProperty<float> MaximumAltitude = new (0, true, val => $"{((float)val/1000):N0} km");
    
    [LocalizedField("PartModules/OrbitalSurvey/ScanningFOVDebug")]
    [PAMDisplayControl(SortIndex = 1)]
    [SteppedRange(1f, 45f, 1f)]
    public ModuleProperty<float> ScanningFieldOfViewDebug = new (1f, false, val => $"{val:N0}°");
    
    // Set through Patch Manager
    [KSPDefinition] public string ModeValue;
    [KSPDefinition] public float ScanningFieldOfViewValue;
    [KSPDefinition] public float MinimumAltitudeValue;
    [KSPDefinition] public float IdealAltitudeValue;
    [KSPDefinition] public float MaximumAltitudeValue;

    
    private Status _statusValue;
    public Status StatusValue
    {
        get => _statusValue;
        set
        {
            _statusValue = value;
            // update PAM
            Status.SetValue(LocalizationStrings.STATUS[value]);
        }
    }
    
    private State _stateValue;
    public State StateValue
    {
        get => _stateValue;
        set
        {
            _stateValue = value;
            // update PAM
            State.SetValue(LocalizationStrings.STATE[value]);
        }
    }

    public ScanningStats ScanningStats;

    public override void OnPartBehaviourModuleInit()
    {
        // We need to set the property values like this because there's an issue with stock code where readOnly properties
        // with [KSPDefinition] aren't being set as readOnly, so they spawn in Flight gamestate as sliders.
        // If we set [JsonIgnore] to properties, then they spawn as readOnly, but Patch Manager isn't registering them.
        // So, we use the "*Value" meta values to set the properties 
        Mode.SetValue(new LocalizedString(ModeValue));
        ScanningFieldOfView.SetValue(ScanningFieldOfViewValue);
        MinimumAltitude.SetValue(MinimumAltitudeValue);
        IdealAltitude.SetValue(IdealAltitudeValue);
        MaximumAltitude.SetValue(MaximumAltitudeValue);
        
        InitializeScanningStats();
    }

    public void InitializeScanningStats()
    {
        if (ScanningStats != null)
            return;
        
        // Setting ScanningStats that will be used by BehaviourModule and PartComponentModule
        ScanningStats = new ScanningStats
        {
            FieldOfView = ScanningFieldOfViewValue,
            MinAltitude = MinimumAltitudeValue,
            IdealAltitude = IdealAltitudeValue,
            MaxAltitude = MaximumAltitudeValue
        };
    }
    

    /// <summary>
    /// Add OAB module description on all eligible parts
    /// </summary>
    public override List<OABPartData.PartInfoModuleEntry> GetPartInfoEntries(Type partBehaviourModuleType,
        List<OABPartData.PartInfoModuleEntry> delegateList)
    {
        if (partBehaviourModuleType == ModuleType)
        {
            // add module description
            delegateList.Add(new OABPartData.PartInfoModuleEntry("", (_) => LocalizationStrings.OAB_DESCRIPTION["ModuleDescription"]));
            
            // MapType header
            var entry = new OABPartData.PartInfoModuleEntry(new LocalizedString(ModeValue),
                _ =>
                {
                    // Subentries
                    var subEntries = new List<OABPartData.PartInfoModuleSubEntry>();
                    subEntries.Add(new OABPartData.PartInfoModuleSubEntry(
                        LocalizationStrings.PARTMODULES["ScanningFOV"],
                        $"{ScanningFieldOfViewValue.ToString("N0")}°"
                    ));
                    subEntries.Add( new OABPartData.PartInfoModuleSubEntry(
                        LocalizationStrings.PARTMODULES["MinAltitude"],
                        $"{(MinimumAltitudeValue / 1000).ToString("N0")} km"
                    ));
                    subEntries.Add(new OABPartData.PartInfoModuleSubEntry(
                        LocalizationStrings.PARTMODULES["IdealAltitude"],
                        $"{(IdealAltitudeValue / 1000).ToString("N0")} km"
                    ));
                    subEntries.Add(new OABPartData.PartInfoModuleSubEntry(
                        LocalizationStrings.PARTMODULES["MaxAltitude"],
                        $"{(MaximumAltitudeValue / 1000).ToString("N0")} km"
                    ));

                    if (UseResources)
                    {
                        subEntries.Add(new OABPartData.PartInfoModuleSubEntry(
                            LocalizationStrings.OAB_DESCRIPTION["ElectricCharge"],
                            $"{RequiredResource.Rate.ToString("N3")} /s"
                        ));                            
                    }
                    
                    return subEntries;
                });
            delegateList.Add(entry);
        }

        return delegateList;
    }

    public override void SetupResourceRequest(ResourceFlowRequestBroker resourceFlowRequestBroker)
    {
        if (UseResources)
        {
            ResourceDefinitionID resourceIDFromName = GameManager.Instance.Game.ResourceDefinitionDatabase.GetResourceIDFromName(this.RequiredResource.ResourceName);
            if (resourceIDFromName == ResourceDefinitionID.InvalidID)
            {
                _LOGGER.LogError($"There are no resources with name {this.RequiredResource.ResourceName}");
                return;
            }
            RequestConfig = new ResourceFlowRequestCommandConfig();
            RequestConfig.FlowResource = resourceIDFromName;
            RequestConfig.FlowDirection = FlowDirection.FLOW_OUTBOUND;
            RequestConfig.FlowUnits = 0.0;
            RequestHandle = resourceFlowRequestBroker.AllocateOrGetRequest("ModuleOrbitalSurvey", default(ResourceFlowRequestHandle));
            resourceFlowRequestBroker.SetCommands(this.RequestHandle, 1.0, new ResourceFlowRequestCommandConfig[] { this.RequestConfig });
        }
    }

    [KSPDefinition]
    [Tooltip("Whether the module consumes resources")]
    public bool UseResources = true;
    
    public bool HasResourcesToOperate = true;
    
    [KSPDefinition]
    [Tooltip("Resource required to operate this module if it consumes resources")]
    public PartModuleResourceSetting RequiredResource;

    public ResourceFlowRequestCommandConfig RequestConfig;

    [JsonIgnore]
    public PartComponentModule_OrbitalSurvey PartComponentModule;
}