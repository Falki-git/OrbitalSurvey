using HarmonyLib;
using I2.Loc;
using KSP.Game.Science;
using KSP.Modules;
using KSP.Sim.impl;

namespace OrbitalSurvey.Utilities;

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
    
    /// <summary>
    /// This removes experiment descriptions on parts in OAB
    /// Since there are experiments for 25%, 50%, 75% and 100% and then for High and Low orbits, there would be too much
    /// spam to show them all.
    /// First experiment that is found is skipped, following experiments are deleted.  
    /// </summary>
    [HarmonyPatch(typeof(Data_ScienceExperiment), "GetPartInfoEntries"), HarmonyPostfix]
    private static void RemoveOabExperimentDescriptions(List<OABPartData.PartInfoModuleEntry> __result, Data_ScienceExperiment __instance)
    {
        LocalizedString visualMappingExperimentName = "OrbitalSurvey/Experiments/OabDescription/DisplayName/VisualMapping";
        LocalizedString biomeMappingExperimentName = "OrbitalSurvey/Experiments/OabDescription/DisplayName/BiomeMapping";
        
        // if a language is selected that isn't supported the retrieved string will be null, so just return
        if (string.IsNullOrEmpty(visualMappingExperimentName) || string.IsNullOrEmpty(biomeMappingExperimentName))
        {
	        return;
        }
     
        bool firstExperimentFound = false;
        
        // Visual Mapping
        for (int i = 0; i < __result.Count; i++)
        {
            // go through the list of entries and look for experiments to delete
            while (i < __result.Count && __result[i].DisplayName.Contains(visualMappingExperimentName))
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
        
        firstExperimentFound = false;
        
        // Biome Mapping
        for (int i = 0; i < __result.Count; i++)
        {
	        // go through the list of entries and look for experiments to delete
	        while (i < __result.Count && __result[i].DisplayName.Contains(biomeMappingExperimentName))
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
    
    /// <summary>
    /// Reimplementation of the original method with the following log spam being omitted:
    /// "[Unity] [Simulation] Science Experiment {experiment name} moved to a valid location, but something else is preventing the experiment from being ready." 
    /// </summary>
    [HarmonyPatch(typeof(PartComponentModule_ScienceExperiment), "RefreshLocationsValidity"), HarmonyPrefix]
    private static bool RefreshLocationsValidity(PartComponentModule_ScienceExperiment __instance)
    {
        if (__instance._vesselComponent == null || __instance._vesselComponent.mainBody == null || __instance._vesselComponent.VesselScienceRegionSituation.ResearchLocation == null)
		{
			return false;
		}
		__instance._currentLocation = new ResearchLocation(true, __instance._vesselComponent.mainBody.bodyName, __instance._vesselComponent.VesselScienceRegionSituation.ResearchLocation.ScienceSituation, __instance._vesselComponent.VesselScienceRegionSituation.ResearchLocation.ScienceRegion);
		int count = __instance.dataScienceExperiment.ExperimentStandings.Count;
		while (count-- > 0)
		{
			ExperimentDefinition experimentDefinition = __instance.dataScienceExperiment.ExperimentStandings[count].ExperimentDefinition;
			__instance.dataScienceExperiment.ExperimentStandings[count].CurrentSituationIsValid = experimentDefinition.IsLocationValid(__instance._currentLocation, out __instance.dataScienceExperiment.ExperimentStandings[count].RegionRequired) && __instance._vesselComponent.VesselScienceRegionSituation.SituationScalar > 0f && __instance._vesselComponent.VesselScienceRegionSituation.ScienceRegionScalar > 0f;
			__instance._currentLocation.RequiresRegion = (__instance.dataScienceExperiment.ExperimentStandings[count].CurrentSituationIsValid ? __instance.dataScienceExperiment.ExperimentStandings[count].RegionRequired : __instance._currentLocation.RequiresRegion);
			if (__instance.dataScienceExperiment.ExperimentStandings[count].CurrentSituationIsValid)
			{
				if (__instance.dataScienceExperiment.ExperimentStandings[count].CurrentExperimentState == ExperimentState.RUNNING)
				{
					__instance.StopExperiment(__instance.dataScienceExperiment.Experiments[count].ExperimentDefinitionID, true, false);
				}
				else if (__instance.dataScienceExperiment.ExperimentStandings[count].CurrentExperimentState != ExperimentState.RUNNING && __instance.dataScienceExperiment.ExperimentStandings[count].CurrentExperimentState != ExperimentState.PAUSED)
				{
					__instance.SetExperimentState(count, ExperimentState.READY);
				}
			}
			else if (__instance.dataScienceExperiment.ExperimentStandings[count].CurrentExperimentState == ExperimentState.RUNNING)
			{
				__instance.dataScienceExperiment.ExperimentStandings[count].CurrentExperimentContext = ScienceRegionsHelper.GetRegionDisplayName(__instance.dataScienceExperiment.LastKnownValidSituation.ResearchLocation.ScienceRegion);
				__instance.StopExperiment(__instance.dataScienceExperiment.ExperimentStandings[count].ExperimentID, false, false);
				__instance.TrySendStateChangeMessage(count, true);
			}
			else
			{
				__instance.dataScienceExperiment.ExperimentStandings[count].CurrentExperimentState = ExperimentState.INVALIDLOCATION;
			}
			switch (__instance.dataScienceExperiment.ExperimentStandings[count].CurrentExperimentState)
			{
			case ExperimentState.NONE:
			case ExperimentState.LOCATIONCHANGED:
				__instance.SetExperimentState(count, ExperimentState.READY);
				continue;
			case ExperimentState.INVALIDLOCATION:
				//GlobalLog.LogF(LogFilter.Simulation, "Science Experiment {0} moved to an invalid location, pausing.", new object[] { experimentDefinition.DisplayName });
				continue;
			}
			//GlobalLog.LogF(LogFilter.Simulation, "Science Experiment {0} moved to a valid location, but something else is preventing the experiment from being ready.", new object[] { experimentDefinition.DisplayName });
		}

		return false;
    }

    #region Potentialpatches
    
    /*
    [HarmonyPatch(typeof(Module_ScienceExperiment), "OnScienceSituationChanged"), HarmonyPrefix]
    private static bool OnScienceSituationChanged(MessageCenterMessage msg, Module_ScienceExperiment __instance)

    [HarmonyPatch(typeof(PartComponentModule_ScienceExperiment), "IsExperimentAllowed"), HarmonyPrefix]
    private static bool IsExperimentAllowed(PartComponentModule_ScienceExperiment __instance, ref bool __result, int experimentIndex, bool notify = true)
    */
    
    #endregion
}