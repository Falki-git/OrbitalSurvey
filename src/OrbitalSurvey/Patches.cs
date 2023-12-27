using HarmonyLib;
using I2.Loc;
using KSP.Messages;
using KSP.Modules;

namespace OrbitalSurvey;

public class Patches
{ 
    /// <summary>
    /// This removes PAM entries in Flight scene for all experiments.
    /// We don't need those as experiments are triggered automatically when certain map percentages are discovered 
    /// </summary>
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
    /*
    /// <summary>
    /// Also used to remove experiments from PAM
    /// </summary>
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
    */
    /// <summary>
    /// This removes experiment descriptions on parts in OAB
    /// Since there are experiments for 25%, 50%, 75% and 100% and then for High and Low orbits, there would be too much
    /// spam to show them all.
    /// First experiment that is found is skipped, following experiments are deleted.  
    /// </summary>
    [HarmonyPatch(typeof(Data_ScienceExperiment), "GetPartInfoEntries"), HarmonyPostfix]
    private static void RemoveOabExperimentDescriptions(List<OABPartData.PartInfoModuleEntry> __result, Data_ScienceExperiment __instance)
    {
        bool firstExperimentFound = false;
        LocalizedString experimentName = "OrbitalSurvey/Experiments/OabDescription/DisplayName/VisualMapping";
        
        for (int i = 0; i < __result.Count; i++)
        {
            // go through the list of entries and look for experiments to delete
            while (i < __result.Count && __result[i].DisplayName.Contains(experimentName))
            {
                // Orbital Survey experiment found
                
                // check if this is the first experiment; if it it we leave it intact
                if (!firstExperimentFound)
                {
                    // we'll advance the index by 4 to skip through descriptions of the found experiment
                    i += 4;
                    firstExperimentFound = true;
                    continue;
                }
                
                // first experiment was already found, so we need to remove other found experiments
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
    
    [HarmonyPatch(typeof(Module_ScienceExperiment), "OnScienceSituationChanged"), HarmonyPrefix]
    private static bool Test(MessageCenterMessage msg, Module_ScienceExperiment __instance)
    {
        return false;
        
        VesselScienceSituationChangedMessage vesselScienceSituationChangedMessage = msg as VesselScienceSituationChangedMessage;
        if (vesselScienceSituationChangedMessage != null && __instance._vesselComponent != null && vesselScienceSituationChangedMessage.Vessel.GlobalId.Equals(__instance._vesselComponent.GlobalId))
        {
            __instance.UpdatePAM();
        }

        return false;
    }
}