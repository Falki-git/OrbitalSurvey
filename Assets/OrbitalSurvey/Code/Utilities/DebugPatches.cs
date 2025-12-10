using HarmonyLib;
using KSP.Game.Science;
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
}