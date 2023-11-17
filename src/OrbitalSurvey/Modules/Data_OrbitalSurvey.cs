using KSP.Api;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.UI.Binding;
using OrbitalSurvey.Models;
using UnityEngine;
using UnityEngine.Serialization;
// ReSharper disable HeapView.BoxingAllocation

namespace OrbitalSurvey.Modules;

[Serializable]
public class Data_OrbitalSurvey : ModuleData
{
    // this.SetVisible = sets a property visible on PAM?
    
    public override Type ModuleType => typeof(Module_OrbitalSurvey);

    [KSPState]
    [LocalizedField("PartModules/OrbitalSurvey/Enabled")]
    [PAMDisplayControl(SortIndex = 1)]
    public ModuleProperty<bool> EnabledToggle = new ModuleProperty<bool>(false);
    
    [KSPState]
    [LocalizedField("PartModules/OrbitalSurvey/Mode")]
    [PAMDisplayControl(SortIndex = 2)]  
    public ModuleProperty<string> Mode = new ModuleProperty<string>("");

    [KSPState]
    [LocalizedField("PartModules/OrbitalSurvey/ScanningFOV")]
    [PAMDisplayControl(SortIndex = 3)]
    [SteppedRange(1f, 45f, 1f)]
    public ModuleProperty<float> ScanningFieldOfView = new ModuleProperty<float>(10f, false, new ToStringDelegate((val) => $"{((float)val):F0}°" ));

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
    
    
    
    
    // TEMPORARY STUFF
    
    [KSPState]
    [FormerlySerializedAs("MyData")] [Tooltip("Some tooltip")]
    public MyComplexClassProperty myComplexClassProperty;

    [KSPState]
    [Tooltip("Some state string value tooltip")]
    public string MyStateString = "Some state string value";
    
    [KSPDefinition]
    [Tooltip("Some definition string value tooltip")]
    public string MyDefinitionString = "Some definition string value";

    public int someIntField;

    public string AddPropertyTest;

    [KSPDefinition]
    public ModuleProperty<string> MyModulePropertyTest = new("initial value");

}