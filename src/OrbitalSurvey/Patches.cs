using HarmonyLib;
using I2.Loc;
using KSP.Modules;

namespace OrbitalSurvey;

public class Patches
{ 
    [HarmonyPatch(typeof(Module_ScienceExperiment), "InitializePAMItems"), HarmonyPrefix]
    private static bool RemovePamItemsForExperiments_Initialize(Module_ScienceExperiment __instance)
    {
        var experiment = __instance.dataScienceExperiment.ExperimentStandings.Find(
            e => e.ExperimentID.StartsWith("orbital_survey"));

        if (experiment != null)
        {
            return false;
        }

        return true;
    }
    
    [HarmonyPatch(typeof(Module_ScienceExperiment), "UpdatePAM"), HarmonyPrefix]
    private static bool RemovePamItemsForExperiments_Update(Module_ScienceExperiment __instance)
    {
        var experiment = __instance.dataScienceExperiment.ExperimentStandings.Find(
            e => e.ExperimentID.StartsWith("orbital_survey"));

        if (experiment != null)
        {
            return false;
        }

        return true;
    }
    
    [HarmonyPatch(typeof(Data_ScienceExperiment), "GetPartInfoEntries"), HarmonyPostfix]
    private static void RemoveOabExperimentDescriptions(List<OABPartData.PartInfoModuleEntry> __result, Data_ScienceExperiment __instance)
    {
        bool genericExperimentOverriden = false;
        LocalizedString nameToOverride = "OrbitalSurvey/Experiments/OabDescription/DisplayName/ToOverride";
        
        for (int i = 0; i < __result.Count; i++)
        {
            // go through the list of entries and look for experiments to override their text or to delete their entries
            while (i < __result.Count && __result[i].DisplayName.Contains(nameToOverride))
            {
                // Orbital Survey experiment found
                
                // check if the generic experiment has been defined yet, if not then perform needed overrides
                if (!genericExperimentOverriden)
                {
                    // override with generic experiment description didn't happen yet, so we do it here
                    LocalizedString newDisplayName = "OrbitalSurvey/Experiments/OabDescription/DisplayName/OverrideWith";
                    LocalizedString requirementsToOverride = "OrbitalSurvey/Experiments/OabDescription/DisplayRequirements/ToOverride";
                    LocalizedString newRequirements = "OrbitalSurvey/Experiments/OabDescription/DisplayRequirements/OverrideWith";
                    
                    var entriesToOverride = 4;
                    for (int j = 0; j < entriesToOverride; j++)
                    {
                        if (i < __result.Count)
                        {
                            __result[i].DisplayName = __result[i].DisplayName.Replace(nameToOverride, newDisplayName);
                            __result[i].DisplayName = __result[i].DisplayName.Replace(requirementsToOverride, newRequirements);
                            i++;
                        }
                    }
                    
                    genericExperimentOverriden = true;
                    continue;
                }
                
                // generic experiment has been defined, so we need to remove other experiment entries
                var entriesToRemove = 4;

                for (int j = 0; j < entriesToRemove; j++)
                {
                    if (i < __result.Count)
                    {
                        __result.RemoveAt(i);
                    }
                }
            }
        }
    }
}