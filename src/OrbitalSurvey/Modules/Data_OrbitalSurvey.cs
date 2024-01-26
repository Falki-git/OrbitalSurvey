using BepInEx.Logging;
using I2.Loc;
using KSP.Game;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.ResourceSystem;
using KSP.UI.Binding;
using Newtonsoft.Json;
using OrbitalSurvey.Managers;
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
    
    [LocalizedField("PartModules/OrbitalSurvey/Mode")]
    [PAMDisplayControl(SortIndex = 3)]
    public ModuleProperty<string> Mode = new ModuleProperty<string>("");
    
    [LocalizedField("PartModules/OrbitalSurvey/State")]
    [PAMDisplayControl(SortIndex = 4)]  
    public ModuleProperty<string> State = new ("");

    [LocalizedField("PartModules/OrbitalSurvey/PercentComplete")]
    [PAMDisplayControl(SortIndex = 5)]
    [JsonIgnore]
    public ModuleProperty<float> PercentComplete = new (0f, true, val => $"{val:P0}");
    
    [LocalizedField("PartModules/OrbitalSurvey/ScanningFOV")]
    [PAMDisplayControl(SortIndex = 6)]
    [JsonIgnore]
    public ModuleProperty<float> ScanningFieldOfView = new (0, true, val => $"{val:N0}°");
    
    [LocalizedField("PartModules/OrbitalSurvey/BodyCategory")]
    [PAMDisplayControl(SortIndex = 7)]
    public ModuleProperty<string> BodyCategory = new ("");
    
    [LocalizedField("PartModules/OrbitalSurvey/BodyCategory/Preview")]
    [PAMDisplayControl(SortIndex = 7)]
    public ModuleProperty<string> BodyCategoryOabDropdown = new ("");
    
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

    public ScanningStats ScanningStats { get; set; } = new();

    public override void OnPartBehaviourModuleInit()
    {
        // We need to set the property values like this because there's an issue with stock code where readOnly properties
        // with [KSPDefinition] aren't being set as readOnly, so they spawn in Flight gamestate as sliders.
        // If we set [JsonIgnore] to properties, then they spawn as readOnly, but Patch Manager isn't registering them.
        // So, we use the "*Value" meta values to set the properties 
        Mode.SetValue(new LocalizedString(ModeValue));
        ScanningFieldOfView.SetValue(ScanningFieldOfViewValue);

        SetBodyCategoryOabDropdownItems();
    }
    
    public void InitializeScanningStats()
    {
        ScanningStats.FieldOfView = ScanningFieldOfViewValue;
    }

    public void SetScanningStats(string body, string category, ScanningAltitudes altitudes)
    {
        // Setting ScanningStats that will be used by BehaviourModule and PartComponentModule
        ScanningStats.Body = body;
        ScanningStats.Category = category;
        ScanningStats.MinAltitude = altitudes.MinAltitude;
        ScanningStats.IdealAltitude = altitudes.IdealAltitude;
        ScanningStats.MaxAltitude = altitudes.MaxAltitude;

        BodyCategory.SetValue($"{CelestialCategoryManager.Instance.CategoryLocalization[category]} ({body})");
        MinimumAltitude.SetValue(altitudes.MinAltitude);
        IdealAltitude.SetValue(altitudes.IdealAltitude);
        MaximumAltitude.SetValue(altitudes.MaxAltitude);
    }

    /// <summary>
    /// Handles BodyCategory dropdown list value change
    /// </summary>
    /// <param name="category"></param>
    public void SetOabScanningStats(string category)
    {
        var mapType = LocalizationStrings.MODE_TYPE_TO_MAP_TYPE[ModeValue];
        var stats = CelestialCategoryManager.Instance.GetOabScanningStats(category, mapType);
        SetScanningStats("", category, stats);
    }
    
    /// <summary>
    /// Initializes the Body Category selector in OAB
    /// </summary>
    private void SetBodyCategoryOabDropdownItems()
    {
        var bodyCategoryDropdown = new DropdownItemList();
        foreach (var (key, value) in CelestialCategoryManager.Instance.CategoryLocalization)
        {
            bodyCategoryDropdown.Add(key, new DropdownItem() { key = key, text = value });    
        }
        SetDropdownData(BodyCategoryOabDropdown, bodyCategoryDropdown);

        // set initial values
        var initialValue = CelestialCategoryManager.Instance.CategoryLocalization.First();
        BodyCategoryOabDropdown.SetValue(initialValue.Key);
        SetOabScanningStats(initialValue.Key);
    }
    

    /// <summary>
    /// Add OAB module description on all eligible parts
    /// </summary>
    public override List<OABPartData.PartInfoModuleEntry> GetPartInfoEntries(Type partBehaviourModuleType,
        List<OABPartData.PartInfoModuleEntry> delegateList)
    {
        // Example:
        // Scans celestial bodies from orbit
        // Visual
        // | Field Of View: 5°
        // | Small Category (max 150 km radius)
        // | | Min Altitude: 60 km
        // | | Ideal Altitude: 170 kn
        // | | Max Altitude: 220 kn
        // | Medium Category (max 350 km radius)
        // | | Min Altitude: 100 km
        // | | Ideal Altitude: 300 kn
        // | | Max Altitude: 500 kn
        // | Large Category (max 10000 km radius)
        // | | Min Altitude: 500 km
        // | | Ideal Altitude: 800 kn
        // | | Max Altitude: 1100 kn
        // | Electric Charge: 1.000 /s
        
        if (partBehaviourModuleType == ModuleType)
        {
            // module description
            delegateList.Add(new OABPartData.PartInfoModuleEntry("", (_) => LocalizationStrings.OAB_DESCRIPTION["ModuleDescription"]));
            
            // MapType header ("Visual" or "Region")
            var entry = new OABPartData.PartInfoModuleEntry(new LocalizedString(ModeValue),
                _ =>
                {
                    var subEntries = new List<OABPartData.PartInfoModuleSubEntry>();
                    
                    // Field Of View
                    subEntries.Add(new OABPartData.PartInfoModuleSubEntry(
                        LocalizationStrings.PARTMODULES["ScanningFOV"],
                        $"{ScanningFieldOfViewValue.ToString("N0")}°"
                    ));
                    
                    var mapType = LocalizationStrings.MODE_TYPE_TO_MAP_TYPE[ModeValue];
                    var allCategoryAltitudes = CelestialCategoryManager.Instance.GetCategoryAltitudesForGivenMapType(mapType);
                    
                    // Categories and Altitudes ("Small Category" -> "Min/Ideal/Max altitudes") 
                    foreach (var categoryAltitudes in allCategoryAltitudes)
                    {
                        // Altitudes ("Min Altitude: 60 km", "Ideal Altitude: 170 kn", "Max Altitude: 220 kn")
                        var categoryAltitudesSubEntries = new List<OABPartData.PartInfoModuleSubEntry>();

                        var minAltitude = categoryAltitudes.altitudes.MinAltitude;
                        var idealAltitude = categoryAltitudes.altitudes.IdealAltitude;
                        var maxAltitude = categoryAltitudes.altitudes.MaxAltitude;

                        categoryAltitudesSubEntries.Add(new OABPartData.PartInfoModuleSubEntry(
                            LocalizationStrings.PARTMODULES["MinAltitude"],
                            $"{(minAltitude / 1000f).ToString("N0")} km"));
                        categoryAltitudesSubEntries.Add(new OABPartData.PartInfoModuleSubEntry(
                            LocalizationStrings.PARTMODULES["IdealAltitude"],
                            $"{(idealAltitude / 1000f).ToString("N0")} km"));
                        categoryAltitudesSubEntries.Add(new OABPartData.PartInfoModuleSubEntry(
                            LocalizationStrings.PARTMODULES["MaxAltitude"],
                            $"{(maxAltitude / 1000f).ToString("N0")} km"));

                        // Category description ("Small Category (max 150 km radius)")
                        var maxRadius = CelestialCategoryManager.Instance.MaxRadiusDefinition[categoryAltitudes.category] / 1000f;
                        var categoryDescriptionLocalized = LocalizationStrings.OAB_DESCRIPTION["CategoryDescription"];
                        var categoryDescription = string.Format(categoryDescriptionLocalized, categoryAltitudes.category, maxRadius.ToString("N0"));
                        
                        var categorySubEntry = new OABPartData.PartInfoModuleSubEntry(categoryDescription, categoryAltitudesSubEntries);
                    
                        subEntries.Add(categorySubEntry);
                    }

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