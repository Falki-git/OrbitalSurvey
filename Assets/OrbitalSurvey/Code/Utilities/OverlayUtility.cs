using System;
using System.Collections.Generic;
using KSP.Game;
using KSP.Map;
using UnityEngine;

namespace OrbitalSurvey.Utilities
{
    public static class OverlayUtility
    {
        public static GameInstance Game = GameManager.Instance.Game;

        private static MapCore _mapCore
        {
            get
            {
                GameManager.Instance.Game.Map.TryGetMapCore(out var mapCore);
                return mapCore;
            }
        }
        private static readonly ReduxLib.Logging.ILogger Logger = ReduxLib.ReduxLib.GetLogger($"OrbitalSurvey|{typeof(OverlayUtility).Name}");

        public static readonly Dictionary<string, string> MAP3D_CELESTIAL_PATH = new()
        {
            { "Moho", "Map3D(Clone)/Map-Moho/Celestial.Moho.Scaled(Clone)" },
            { "Eve", "Map3D(Clone)/Map-Eve/Celestial.Eve.Scaled(Clone)" },
            { "Gilly", "Map3D(Clone)/Map-Gilly/Celestial.Gilly.Scaled(Clone)" },
            { "Kerbin", "Map3D(Clone)/Map-Kerbin/Celestial.Kerbin.Scaled(Clone)" },
            { "Mun", "Map3D(Clone)/Map-Mun/Celestial.Mun.Scaled(Clone)" },
            { "Minmus", "Map3D(Clone)/Map-Minmus/Celestial.Minmus.Scaled(Clone)" },
            { "Duna", "Map3D(Clone)/Map-Duna/Celestial.Duna.Scaled(Clone)" },
            { "Ike", "Map3D(Clone)/Map-Ike/Celestial.Ike.Scaled(Clone)" },
            { "Dres", "Map3D(Clone)/Map-Dres/Celestial.Dres.Scaled(Clone)" },
            { "Jool", "Map3D(Clone)/Map-Jool/Celestial.Jool.Scaled(Clone)" },
            { "Laythe", "Map3D(Clone)/Map-Laythe/Celestial.Laythe.Scaled(Clone)" },
            { "Vall", "Map3D(Clone)/Map-Vall/Celestial.Vall.Scaled(Clone)" },
            { "Tylo", "Map3D(Clone)/Map-Tylo/Celestial.Tylo.Scaled(Clone)" },
            { "Bop", "Map3D(Clone)/Map-Bop/Celestial.Bop.Scaled(Clone)" },
            { "Pol", "Map3D(Clone)/Map-Pol/Celestial.Pol.Scaled(Clone)" },
            { "Eeloo", "Map3D(Clone)/Map-Eeloo/Celestial.Eeloo.Scaled(Clone)" },
            { "Drast", "Map3D(Clone)/Map-Drast/Celestial.Drast.Scaled(Clone)" }
        };

        private const string _MAP3D_ROOT = "Map3D(Clone)";

        /// <summary>
        /// Returns the scaled-space object that the Map3d overlay is painted onto, or null when
        /// that body's scaled space isn't currently loaded (the game only instantiates it for
        /// bodies near the map camera).
        /// </summary>
        /// <remarks>
        /// Map3DView names the focus item "Map-&lt;body&gt;" and parents the scaled-space instance
        /// under it, but it never renames that instance - it keeps whatever its prefab root is
        /// called, so <see cref="MAP3D_CELESTIAL_PATH"/> can't cover bodies added after this mod
        /// was written. Map3DFocusItem.View3DVisual is exactly the instance Map3DView handed it in
        /// OnMapScaledSpaceCelestialBodyInstantiated, and it's reset to null when that scaled space
        /// unloads, so it answers "is it loaded, and which object is it" in one go.
        ///
        /// Note it has to be this property and not a search for Map3DSpaceProviderTarget: the focus
        /// item's own selection widget (view3DSelection) is one of those too, and being higher in
        /// the hierarchy it wins GetComponentInChildren - giving back a collider with no renderer.
        /// </remarks>
        public static GameObject FindMap3dBodyObject(string bodyName)
        {
            var focusItem = GameObject.Find($"{_MAP3D_ROOT}/Map-{bodyName}");

            if (focusItem == null || !focusItem.TryGetComponent<Map3DFocusItem>(out var mapFocusItem))
                return null;

            var scaledSpaceVisual = mapFocusItem.View3DVisual;

            return scaledSpaceVisual == null ? null : scaledSpaceVisual.gameObject;
        }

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
                    Logger.LogError($"Unable to retrieve the focused body for object {_mapCore.Focused.ItemName}.");

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
}