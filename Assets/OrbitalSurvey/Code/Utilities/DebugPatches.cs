using HarmonyLib;
using KSP.Game.Science;
using KSP.Modules;
using KSP.Rendering.Planets;
using KSP.Sim.Definitions;
using OrbitalSurvey.Debug;
using OrbitalSurvey.Managers;
using UnityEngine;

namespace OrbitalSurvey.Utilities
{
    [HarmonyPatch]
    public class DebugPatches
    {
        [HarmonyPatch(typeof(PQSScienceOverlay), "Update"), HarmonyPrefix]
        private static bool PQSScienceOverlay_Update(PQSScienceOverlay __instance)
        {
            var scienceRegionsProvider = ReflectionUtility.GetPrivateField<ScienceRegionsDataProvider>(__instance, "_scienceRegionsProvider");
            var pqs = ReflectionUtility.GetPrivateField<PQS>(__instance, "_pqs");
            var overlayTexture = ReflectionUtility.GetPrivateField<Texture2D>(__instance, "_overlayTexture");

            if (scienceRegionsProvider != null && pqs != null && overlayTexture == null)
            {
                string bodyName = pqs.CoreCelestialBodyData.Data.bodyName;
                CelestialBodyBakedScienceRegionMap bakedMap = scienceRegionsProvider.GetBakedMap(bodyName);
                if (bakedMap != null)
                {
                    Color32[] array = new Color32[bakedMap.Width * bakedMap.Height];
                    var newTexture = new Texture2D(bakedMap.Width, bakedMap.Height, TextureFormat.RGBA32, false)
                    {
                        filterMode = FilterMode.Point
                    };
                    ReflectionUtility.SetPrivateField(__instance, "_overlayTexture", newTexture);

                    string debugBody = DebugManager.Instance.RegionBody;

                    for (int i = 0; i < bakedMap.MapData.Length; i++)
                    {
                        // Original method:
                        //array[i] = ScienceRegionsHelper.ScienceRegionsVisualizationPalette[(int)bakedMap.MapData[i]];
                        var debugRegionColor = RegionsManager.Instance.Data[debugBody][bakedMap.MapData[i]].Color;
                        array[i] = debugRegionColor;
                    }

                    newTexture.SetPixelData<Color32>(array, 0, 0);
                    newTexture.Apply();
                    //ReflectionUtility.GetPrivateField<Material>(__instance, "_overlayMaterial").SetTexture(ReflectionUtility.GetPrivateField<int>(__instance, "_overlayTextureParameterId"), newTexture);
                }
            }
            //ReflectionUtility.GetPrivateField<Material>(__instance, "_overlayMaterial").SetFloat(ReflectionUtility.GetPrivateField<int>(__instance, "_strengthParameterId"), __instance.Strength);

            return false;
        }

        [HarmonyPatch(typeof(PQSScienceOverlay), "Awake"), HarmonyPostfix]
        private static void PQSScienceOverlay_Awake(PQSScienceOverlay __instance)
        {
            if (ReflectionUtility.GetPrivateField<Material>(__instance, "_overlayMaterial") == null)
            {
                ReflectionUtility.SetPrivateField(__instance, "_overlayMaterial",
                    new Material(Shader.Find("KSP2/Environment/CelestialBody/CelestialBody_Local_Old")));
            }
        }
    }
}