using HarmonyLib;
using KSP.Game;
using KSP.Game.Missions;
using KSP.Game.Missions.Definitions;
using KSP.Game.Missions.State;
using KSP.Game.Science;
using KSP.Messages;
using KSP.Modules;
using KSP.Rendering.Planets;
using KSP.Sim.Definitions;
using OrbitalSurvey.Debug;
using OrbitalSurvey.Managers;
using UnityEngine;

namespace OrbitalSurvey.Utilities;

public class DebugPatches
{
    [HarmonyPatch(typeof(PQSScienceOverlay), "Update"), HarmonyPrefix]
    private static bool PQSScienceOverlay_Update(PQSScienceOverlay __instance)
    { 
        if (__instance._scienceRegionsProvider != null && __instance._pqs != null && __instance._overlayTexture == null)
        {
            string bodyName = __instance._pqs.CoreCelestialBodyData.Data.bodyName;
            CelestialBodyBakedScienceRegionMap bakedMap = __instance._scienceRegionsProvider.GetBakedMap(bodyName);
            if (bakedMap != null)
            {
                Color32[] array = new Color32[bakedMap.Width * bakedMap.Height];
                __instance._overlayTexture = new Texture2D(bakedMap.Width, bakedMap.Height, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point
                };

                string debugBody = DebugManager.Instance.RegionBody;
                
                for (int i = 0; i < bakedMap.MapData.Length; i++)
                {
                    // Original method:
                    //array[i] = ScienceRegionsHelper.ScienceRegionsVisualizationPalette[(int)bakedMap.MapData[i]];
                    var debugRegionColor = RegionsManager.Instance.Data[debugBody][bakedMap.MapData[i]].Color;
                    array[i] = debugRegionColor;
                }
                __instance._overlayTexture.SetPixelData<Color32>(array, 0, 0);
                __instance._overlayTexture.Apply();
                //__instance._overlayMaterial.SetTexture(__instance._overlayTextureParameterId, __instance._overlayTexture);
            }
        }
        //__instance._overlayMaterial.SetFloat(__instance._strengthParameterId, __instance.Strength);

        return false;
    }
    
    [HarmonyPatch(typeof(PQSScienceOverlay), "Awake"), HarmonyPostfix]
    private static void PQSScienceOverlay_Awake(PQSScienceOverlay __instance)
    {
        if (__instance._overlayMaterial == null)
        {
            __instance._overlayMaterial =
                new Material(Shader.Find("KSP2/Environment/CelestialBody/CelestialBody_Local_Old"));
        }
    }
    
    /*
    [HarmonyPatch(typeof(PropertyCondition), "CheckCurrentValue"), HarmonyPrefix]
    private static bool CheckCurrentValue(PropertyCondition __instance, ref bool __result)
    {
	    bool flag = false;
			if (__instance.property != null)
			{
				if (__instance.property.baseType() == typeof(double))
				{
					double num = (__instance.isInput ? __instance.property.GetValueDouble(__instance.parentMission, __instance.Inputstring) : __instance.property.GetValueDouble());
					switch (__instance.propOperator)
					{
					case PropertyOperator.LESSER:
						flag = num < __instance.TestWatchedValue;
						break;
					case PropertyOperator.EQUAL:
						flag = num == __instance.TestWatchedValue;
						break;
					case PropertyOperator.GREATER:
						flag = num > __instance.TestWatchedValue;
						break;
					default:
						flag = false;
						break;
					}
				}
				if (__instance.property.baseType() == typeof(bool))
				{
					bool flag2 = (__instance.isInput ? __instance.property.GetValueBool(__instance._parentMissionID, __instance.Inputstring) : __instance.property.GetValueBool());
					PropertyOperator propertyOperator = __instance.propOperator;
					if (propertyOperator - PropertyOperator.LESSER <= 2)
					{
						flag = flag2;
					}
				}
				if (__instance.property.baseType() == typeof(string))
				{
					string text = (__instance.isInput ? __instance.property.GetValueString(__instance.Inputstring) : __instance.property.GetValueString());
					switch (__instance.propOperator)
					{
					case PropertyOperator.LESSER:
						flag = text.CompareTo(__instance.TestWatchedstring) < 0;
						break;
					case PropertyOperator.EQUAL:
						flag = text.CompareTo(__instance.TestWatchedstring) == 0;
						break;
					case PropertyOperator.GREATER:
						flag = text.CompareTo(__instance.TestWatchedstring) > 0;
						break;
					default:
						flag = false;
						break;
					}
				}
				else if (__instance.property.baseType().IsEnum || __instance.property.baseType() == typeof(int))
				{
					int num2 = (__instance.isInput ? __instance.property.GetValueInt(__instance.parentMission, __instance.Inputstring) : __instance.property.GetValueInt());
					switch (__instance.propOperator)
					{
					case PropertyOperator.LESSER:
						flag = num2 < __instance.TestWatchedInt;
						break;
					case PropertyOperator.EQUAL:
						flag = num2 == __instance.TestWatchedInt;
						break;
					case PropertyOperator.GREATER:
						flag = num2 > __instance.TestWatchedInt;
						break;
					default:
						flag = false;
						break;
					}
				}
			}
			//return flag;
			__result = flag;
			return false;
    }
    
    [HarmonyPatch(typeof(MissionData), "Update"), HarmonyPrefix]
    private static bool Update(MissionData __instance)
    {
	    __instance.missionStages[__instance.currentStageIndex].Update();
	    if (__instance.CurrentStageUpdatesExceptionBranches())
	    {
		    foreach (MissionBranch missionBranch in __instance.ExceptionBranches)
		    {
			    missionBranch.Update();
		    }
	    }
	    if (__instance.pendingCompletionTest)
	    {
		    __instance.TestStageCompletion();
	    }
	    if (__instance.CanTestPreRequisiteBranches())
	    {
		    foreach (MissionBranch missionBranch2 in __instance.PreRequisiteBranches)
		    {
			    missionBranch2.Update();
		    }
	    }

	    return false;
    }
    */
}