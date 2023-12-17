using BepInEx.Logging;
using KSP.Game;
using KSP.Map;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace OrbitalSurvey.Utilities;

public static class OverlayUtility
{
    public static GameInstance Game = GameManager.Instance.Game;
    
    private static MapCore _mapCore => GameManager.Instance.Game.Map._core;
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.OverlayUtility");
    
    public static readonly Dictionary<string, string> MAP3D_CELESTIAL_PATH = new()
    {
        { "Moho", "Map3D(Clone)/Map-Moho/Celestial.Moho.Scaled(Clone)"},
        { "Eve", "Map3D(Clone)/Map-Eve/Celestial.Eve.Scaled(Clone)"},
        { "Gilly", "Map3D(Clone)/Map-Gilly/Celestial.Gilly.Scaled(Clone)"},
        { "Kerbin", "Map3D(Clone)/Map-Kerbin/Celestial.Kerbin.Scaled(Clone)"},
        { "Mun", "Map3D(Clone)/Map-Mun/Celestial.Mun.Scaled(Clone)"},
        { "Minmus", "Map3D(Clone)/Map-Minmus/Celestial.Minmus.Scaled(Clone)"},
        { "Duna", "Map3D(Clone)/Map-Duna/Celestial.Duna.Scaled(Clone)"},
        { "Ike", "Map3D(Clone)/Map-Ike/Celestial.Ike.Scaled(Clone)"},
        { "Dres", "Map3D(Clone)/Map-Dres/Celestial.Dres.Scaled(Clone)"},
        { "Jool", "Map3D(Clone)/Map-Jool/Celestial.Jool.Scaled(Clone)"},
        { "Laythe", "Map3D(Clone)/Map-Laythe/Celestial.Laythe.Scaled(Clone)"},
        { "Vall", "Map3D(Clone)/Map-Vall/Celestial.Vall.Scaled(Clone)"},
        { "Tylo", "Map3D(Clone)/Map-Tylo/Celestial.Tylo.Scaled(Clone)"},
        { "Bop", "Map3D(Clone)/Map-Bop/Celestial.Bop.Scaled(Clone)"},
        { "Pol", "Map3D(Clone)/Map-Pol/Celestial.Pol.Scaled(Clone)"},
        { "Eeloo", "Map3D(Clone)/Map-Eeloo/Celestial.Eeloo.Scaled(Clone)"}
    };
    
    // Define a recursive function to search for an object by name
    public static Transform FindObjectByNameRecursively(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            // Recursively search in the child's children
            Transform found = FindObjectByNameRecursively(child, name);
            if (found != null)
            {
                return found;
            }
        }

        return null; // Return null if the object was not found in the hierarchy
    }

    [Obsolete]
    public static string GetMap3dFocusedBody()
    {
        if (_mapCore.Focused == null)
            return string.Empty;
        
        if (_mapCore.Focused.MapItemType != MapItemType.CelestialBody)
        {
            var vesselGuid = _mapCore.Focused.SimGUID;

            var vesselSimObject = Game.ViewController.Universe.ModelLookup.FindSimObject(vesselGuid);

            var toReturn = vesselSimObject?.Orbit?.referenceBody?.Name ?? string.Empty;

            if (string.IsNullOrEmpty(toReturn))
                _LOGGER.LogError($"Unable to retrieve the focused body for object {_mapCore.Focused.ItemName}.");

            return toReturn;
        }

        return _mapCore.Focused.ItemName;
    }
    
    public static void TryToggleMeshRendererComponent(this GameObject gameObject, bool newState)
    {
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = newState;
        }
    }
}