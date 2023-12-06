using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.UI.Binding;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;

// ReSharper disable HeapView.BoxingAllocation

namespace OrbitalSurvey.Modules;

[Serializable]
public class Data_OrbitalSurvey : ModuleData
{
    public override Type ModuleType => typeof(Module_OrbitalSurvey);

    [LocalizedField("PartModules/OrbitalSurvey/Status")]
    [PAMDisplayControl(SortIndex = 1)]  
    public ModuleProperty<string> Status = new ("Idle");
    
    [KSPState]
    [LocalizedField("PartModules/OrbitalSurvey/Enabled")]
    [PAMDisplayControl(SortIndex = 2)]
    public ModuleProperty<bool> EnabledToggle = new ModuleProperty<bool>(false);
    
    [KSPState]
    [LocalizedField("PartModules/OrbitalSurvey/Mode")]
    [PAMDisplayControl(SortIndex = 3)]  
    public ModuleProperty<string> Mode = new ModuleProperty<string>("Visual");
    
    [LocalizedField("PartModules/OrbitalSurvey/State")]
    [PAMDisplayControl(SortIndex = 4)]  
    public ModuleProperty<string> State = new ("");

    [KSPState]
    [LocalizedField("PartModules/OrbitalSurvey/PercentComplete")]
    [PAMDisplayControl(SortIndex = 5)]
    public ModuleProperty<float> PercentComplete = new (0f, true, val => $"{val:P0}");
    
    [KSPState]
    [LocalizedField("PartModules/OrbitalSurvey/ScanningFOV")]
    [PAMDisplayControl(SortIndex = 7)]
    public ModuleProperty<float> ScanningFieldOfView = new (1f, true, val => $"{val:N0}°");
    
    [KSPState]
    [LocalizedField("PartModules/OrbitalSurvey/MinAltitude")]
    [PAMDisplayControl(SortIndex = 8)]
    public ModuleProperty<float> MinimumAltitude = new (1f, true, val => $"{val:N0} km");
    
    [KSPState]
    [LocalizedField("PartModules/OrbitalSurvey/IdealAltitude")]
    [PAMDisplayControl(SortIndex = 9)]
    public ModuleProperty<float> IdealAltitude = new (0f, true, val => $"{val:N0} km" );
    
    [KSPState]
    [LocalizedField("PartModules/OrbitalSurvey/MaxAltitude")]
    [PAMDisplayControl(SortIndex = 10)]
    public ModuleProperty<float> MaximumAltitude = new (0f, true, val => $"{val:N0} km");
    
    
    
    [LocalizedField("PartModules/OrbitalSurvey/ScanningFOVDebug")]
    [PAMDisplayControl(SortIndex = 7)]
    [SteppedRange(1f, 45f, 1f)]
    public ModuleProperty<float> ScanningFieldOfViewDebug = new (1f, false, val => $"{val:N0}°");

    public override void OnPartBehaviourModuleInit()
    {
        // Initialize Mode dropdown values
        var scanningModesDropdown = new DropdownItemList();

        foreach (MapType mapType in Enum.GetValues(typeof(MapType)))
        {
            scanningModesDropdown.Add(
                mapType.ToString(), new DropdownItem() {key = mapType.ToString(), text = mapType.ToString()});
        }
        
        SetDropdownData(Mode, scanningModesDropdown);
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
            delegateList.Add(new OABPartData.PartInfoModuleEntry("", (_) => LocalizationStrings.OAB_DESCRIPTION));

            foreach (MapType mapType in Enum.GetValues(typeof(MapType)))
            {
                // MapType header
                var entry = new OABPartData.PartInfoModuleEntry(mapType.ToString(),
                    _ =>
                    {
                        // stats for each MapType
                        var subEntries = new List<OABPartData.PartInfoModuleSubEntry>();
                        subEntries.Add(new OABPartData.PartInfoModuleSubEntry(
                            LocalizationStrings.PARTMODULES["ScanningFOV"],
                            $"{Settings.ModeScanningStats[mapType].FieldOfView:N0}°"
                        ));
                        subEntries.Add( new OABPartData.PartInfoModuleSubEntry(
                            LocalizationStrings.PARTMODULES["MinAltitude"],
                            $"{(Settings.ModeScanningStats[mapType].MinAltitude / 1000):N0} km"
                        ));
                        subEntries.Add(new OABPartData.PartInfoModuleSubEntry(
                            LocalizationStrings.PARTMODULES["IdealAltitude"],
                            $"{(Settings.ModeScanningStats[mapType].IdealAltitude / 1000):N0} km"
                        ));
                        subEntries.Add(new OABPartData.PartInfoModuleSubEntry(
                            LocalizationStrings.PARTMODULES["MaxAltitude"],
                            $"{(Settings.ModeScanningStats[mapType].MaxAltitude / 1000):N0} km"
                        ));
                        return subEntries;
                    });
                delegateList.Add(entry);
            }
        }

        return delegateList;
    }
    
    
    // TEMPORARY STUFF
    
    // [KSPState]
    // [FormerlySerializedAs("MyData")] [Tooltip("Some tooltip")]
    // public MyComplexClassProperty myComplexClassProperty;
    
    // [KSPState]
    // [Tooltip("Some state string value tooltip")]
    // public string MyStateString = "Some state string value";
    
    // [KSPDefinition]
    // [Tooltip("Some definition string value tooltip")]
    // public string MyDefinitionString = "Some definition string value";

    //public int someIntField;

    //public string AddPropertyTest;

    //[KSPDefinition]
    //public ModuleProperty<string> MyModulePropertyTest = new("initial value");

}