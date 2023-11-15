using KSP.Sim;
using KSP.Sim.Definitions;
using UnityEngine;
using UnityEngine.Serialization;

namespace OrbitalSurvey.Modules;

[Serializable]
public class Data_OrbitalSurvey : ModuleData
{
    public override Type ModuleType => typeof(Module_OrbitalSurvey);

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