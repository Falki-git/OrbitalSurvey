using System.Collections.Generic;
using System.Threading.Tasks;
using KSP.Game;
using KSP.Rendering.Planets;
using KSP.Sim.impl;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;
using UnityEngine;

namespace OrbitalSurvey.Managers
{
    public class OverlayManager
    {
        private OverlayManager()
        {
            Initialize();
        }

        public static OverlayManager Instance { get; } = new();

        public bool OverlayActive { get; set; }
        public MapType OverlayType { get; set; }

        private static readonly ReduxLib.Logging.ILogger Logger =
            ReduxLib.ReduxLib.GetLogger($"OrbitalSurvey|{typeof(OverlayManager).Name}");

        private const string _OVERLAY_SHADER = "KSP2/Environment/CelestialBody/CelestialBody_Local_Old";
        private const string _OVERLAY_TEXTURE_NAME = "_AlbedoScaledTex";
        private const string _BLACK_OCEAN_TEXTURE_NAME = "_ShorelineSDFTexture";

        private Transform _celestialBody;
        private Texture2D _allBlack;
        private Texture _oceanTextureBackup;

        // original Map3d textures are stored here for when overlays will be turned off
        private Dictionary<string, Texture> _textureBackup = new();

        private VesselComponent _activeVessel =>
            GameManager.Instance?.Game?.ViewController?.GetActiveVehicle()?.GetSimVessel();

        public string ActiveVesselBody => _activeVessel.mainBody.Name;

        private void Initialize()
        {
            _allBlack = AssetUtility.Instance.GetTextureAsset(AssetType.Other, "AllBlack_4096");
        }

        public bool DrawOverlay(MapType mapType)
        {
            bool isSuccess;

            Logger.LogDebug($"DrawOverlay({mapType}) requested. GameState=" +
                            $"{Utility.GameState?.GameState.ToString() ?? "null"}, body=" +
                            $"{(_activeVessel == null ? "no active vessel" : ActiveVesselBody)}.");

            isSuccess = DrawFlightOverlay(mapType);
            DrawMap3dOverlayOnAllLoadedBodies(mapType);

            OverlayActive = true;
            OverlayType = mapType;

            return isSuccess;
        }

        public bool RemoveOverlay()
        {
            bool isSuccess;
            isSuccess = RemoveFlightOverlay();
            RemoveMap3dOverlayOnAllLoadedBodies();

            return isSuccess;
        }

        ///// Flight Overlay /////

        private bool DrawFlightOverlay(MapType mapType)
        {
            if (_activeVessel == null)
            {
                Logger.LogWarning("There is no active vessel, so the flight overlay can't be applied.");
                return false;
            }

            // Also refreshes _celestialBody.
            RemoveFlightOverlay();

            if (_celestialBody == null)
                return false;

            var pqs = _celestialBody.GetComponent<PQS>();

            if (pqs == null)
            {
                // The game only builds a PQS for bodies with quadsphere terrain, so a mesh-only
                // body has no surface material for the overlay to be cloned from.
                Logger.LogWarning($"'{ActiveVesselBody}' has no PQS, so the flight overlay " +
                                  "can't be applied to it.");
                return false;
            }

            var sourceMaterial = pqs.data.materialSettings.surfaceMaterial;
            Material newMaterial = new Material(sourceMaterial);
            string[] shaderKeywords = sourceMaterial.shaderKeywords;
            string[] array = new string[shaderKeywords.Length];
            shaderKeywords.CopyTo(array, 0);
            newMaterial.shaderKeywords = array;
            newMaterial.shader = Shader.Find(_OVERLAY_SHADER);

            var mapTexture = Core.Instance.CelestialDataDictionary[ActiveVesselBody].Maps[mapType].CurrentMap;

            newMaterial.SetTexture(_OVERLAY_TEXTURE_NAME, mapTexture);

            PQSRenderer pqsRenderer = _celestialBody.GetComponent<PQSRenderer>();

            if (pqsRenderer == null)
            {
                Logger.LogWarning($"'{ActiveVesselBody}' has no PQSRenderer, so the flight overlay " +
                                  "can't be applied to it.");
                return false;
            }

            pqsRenderer.AddOverlay(new OrbitalSurveyOverlay { OverlayMaterial = newMaterial });
            SetOceanSphereMaterialToBlack();

            // The overlay draws through PQSRenderer's command buffer, so a material that silently
            // came out wrong (missing shader, unset texture) produces no visual and no error.
            // Report what was actually built so a body that renders nothing can be compared
            // against one that works.
            var overlayCount = ReflectionUtility
                .GetPrivateField<List<IPQSOverlay>>(pqsRenderer, "_overlays")?.Count ?? -1;

            Logger.LogDebug(
                $"Flight overlay applied to '{ActiveVesselBody}'. " +
                $"sourceMaterial='{sourceMaterial.name}' sourceShader='{sourceMaterial.shader?.name ?? "null"}' " +
                $"overlayShader='{newMaterial.shader?.name ?? "NULL - Shader.Find failed"}' " +
                $"mapTexture={(mapTexture == null ? "null" : $"{mapTexture.width}x{mapTexture.height}")} " +
                $"pqsOverlayCount={overlayCount} " +
                $"renderer.enabled={pqsRenderer.enabled} pqsActive={_celestialBody.gameObject.activeInHierarchy}");

            return true;
        }

        private void RefreshCelestialBody()
        {
            _celestialBody = null;

            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");

            if (celestialRoot == null)
            {
                Logger.LogWarning("'#PhysicsSpace/#Celestial' was not found in the scene, " +
                                  "so the flight overlay can't be applied.");
                return;
            }

            _celestialBody = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, ActiveVesselBody);

            if (_celestialBody == null)
            {
                // CelestialBodyBehavior.OnLocalSpaceViewInstantiated only renames the local-space
                // object to the body name when that body has a PQS. A mesh-only body keeps its
                // prefab clone name, so there's nothing here to find.
                Logger.LogWarning($"No object named '{ActiveVesselBody}' was found under " +
                                  "'#PhysicsSpace/#Celestial', so the flight overlay can't be applied to it.");
            }
        }

        private bool RemoveFlightOverlay()
        {
            if (_activeVessel == null)
                return false;

            RefreshCelestialBody();

            if (_celestialBody == null)
                return false;

            PQSRenderer pqsRenderer = _celestialBody.GetComponent<PQSRenderer>();

            if (pqsRenderer == null)
            {
                Logger.LogWarning($"'{ActiveVesselBody}' has no PQSRenderer, so there is no flight overlay to remove.");
                return false;
            }

            var overlays = ReflectionUtility.GetPrivateField<List<IPQSOverlay>>(pqsRenderer, "_overlays");
            if (overlays?.Count > 0)
            {
                var overlay = overlays.Find(o => o is OrbitalSurveyOverlay);

                if (overlay != null)
                {
                    pqsRenderer.RemoveOverlay(overlay);
                    RevertOceanSphereMaterial();
                    OverlayActive = false;
                    return true;
                }
            }

            return false;
        }

        private void SetOceanSphereMaterialToBlack()
        {
            var pqsRenderer = _celestialBody.GetComponent<PQSRenderer>();

            var oceanMaterial = ReflectionUtility.GetPrivateField<Material>(pqsRenderer, "_oceanMaterial");
            _oceanTextureBackup = oceanMaterial?.GetTexture(_BLACK_OCEAN_TEXTURE_NAME);

            if (_oceanTextureBackup == null)
            {
                // body doesn't have an ocean; just return
                return;
            }

            ReflectionUtility.GetPrivateField<Material>(pqsRenderer, "_oceanSpereMaterial").SetTexture(_BLACK_OCEAN_TEXTURE_NAME, _allBlack);
            oceanMaterial.SetTexture(_BLACK_OCEAN_TEXTURE_NAME, _allBlack);
        }

        private void RevertOceanSphereMaterial()
        {
            if (_oceanTextureBackup == null)
                return;

            var pqsRenderer = _celestialBody.GetComponent<PQSRenderer>();

            //ReflectionUtility.GetPrivateField<Material>(pqsRenderer, "_oceanSpereMaterial").SetTexture(_BLACK_OCEAN_TEXTURE_NAME, _oceanTextureBackup);
            ReflectionUtility.GetPrivateField<Material>(pqsRenderer, "_oceanMaterial").SetTexture(_BLACK_OCEAN_TEXTURE_NAME, _oceanTextureBackup);
            _oceanTextureBackup = null;
        }

        ///// Map3dOverlay /////

        private void DrawMap3dOverlayOnAllLoadedBodies(MapType mapType)
        {
            if (Utility.GameState?.GameState != GameState.Map3DView)
                return;

            var celestialBodies = GameManager.Instance.Game?.UniverseModel?.GetAllCelestialBodies();
            if (celestialBodies == null)
                return;

            var applied = 0;
            var notFound = new List<string>();

            foreach (var body in celestialBodies)
            {
                if (!Core.Instance.CelestialDataDictionary.ContainsKey(body.Name))
                    continue;

                // Null whenever this body's scaled space isn't loaded, which is the normal case
                // for everything that isn't near the map camera.
                var bodyObj = OverlayUtility.FindMap3dBodyObject(body.Name);

                if (bodyObj == null)
                {
                    notFound.Add(body.Name);
                    continue;
                }

                var meshRenderer = bodyObj.GetComponent<MeshRenderer>();

                if (meshRenderer == null)
                {
                    Logger.LogWarning($"The scaled-space object for '{body.Name}' has no MeshRenderer, " +
                                      "so the Map3d overlay can't be applied to it.");
                    continue;
                }

                var overlayTexture = Core.Instance.CelestialDataDictionary[body.Name].Maps[mapType].CurrentMap;

                if (!_textureBackup.ContainsKey(body.Name))
                {
                    // backup the texture so it can be restored later when the overlay is turned off
                    _textureBackup.Add(body.Name, meshRenderer.material.mainTexture);
                }

                applied++;

                meshRenderer.material.SetTexture("_MainTex", overlayTexture);

                //disable clouds and atmosphere, if the body has them
                bodyObj.GetChild("Fluffy Clouds(Scaled)")?.TryToggleMeshRendererComponent(false);
                bodyObj.GetChild("Wispy Clouds(Scaled)")?.TryToggleMeshRendererComponent(false);
                bodyObj.GetChild("Thick Cumulus Clouds(Scaled)")?.TryToggleMeshRendererComponent(false);
                bodyObj.GetChild("Cloud(Scaled)")?.TryToggleMeshRendererComponent(false);
                bodyObj.GetChild("Atmosphere.Inner")?.TryToggleMeshRendererComponent(false);
                bodyObj.GetChild("Atmosphere.Outer")?.TryToggleMeshRendererComponent(false);
            }

            Logger.LogDebug($"Map3D {mapType} overlay applied to {applied} body/bodies." +
                            (notFound.Count > 0
                                ? $" No scaled-space object found for: {string.Join(", ", notFound)}."
                                : string.Empty));
        }

        /// <summary>
        /// Called when OnMapCelestialBodyAddedMessage is triggered.
        /// This happens when we're in Map3d scene and when close enough that the scaled space texture is loaded.
        /// We're doing this after event triggers because user can often zoom out, which destroys the scaled space object,
        /// and them zoom back in which recreates the scaled space object and then the texture needs to be applied again.
        /// This is an async delayed task because it takes a while for clouds and atmosphere to be generated. 
        /// </summary>
        /// <param name="bodyName">Name of celestial body</param>
        /// <param name="milisecondsDelay">Specifies an async delay to apply the overlay. Defaults at 100 ms. Used when
        ///     OnMapCelestialBodyAddedMessage is triggered since it takes a bit for clouds and atmosphere to load (needed
        ///     to correctly apply the overlay)</param>
        public async Task DrawMap3dOverlayOnMapCelestialBodyAddedMessage(string bodyName, int milisecondsDelay = 100)
        {
            if (!Core.Instance.CelestialDataDictionary.ContainsKey(bodyName))
            {
                Logger.LogError($"Body '{bodyName}' not found in the CelestialDataDictionary.");
                return;
            }

            if (!OverlayActive &&
                (!Settings.ShowMapOverlayAlways.Value ||
                 (Settings.ShowMapOverlayAlways.Value && Core.Instance.CelestialDataDictionary[bodyName]
                     .Maps[MapType.Visual].IsFullyScanned)))
                return;

            var overlayTexture = Core.Instance.CelestialDataDictionary[bodyName]
                .Maps[OverlayActive ? OverlayType : MapType.Visual].CurrentMap;

            // wait for the Map3d to receive its clouds and atmosphere
            await Task.Delay(milisecondsDelay);

            var body = OverlayUtility.FindMap3dBodyObject(bodyName);

            if (body == null)
            {
                Logger.LogWarning($"The scaled-space object for '{bodyName}' was not found, " +
                                  "so the Map3d overlay can't be applied to it.");
                return;
            }

            var meshRenderer = body.GetComponent<MeshRenderer>();

            if (meshRenderer == null)
            {
                Logger.LogWarning($"The scaled-space object for '{bodyName}' has no MeshRenderer, " +
                                  "so the Map3d overlay can't be applied to it.");
                return;
            }

            if (!_textureBackup.ContainsKey(bodyName))
            {
                _textureBackup.Add(bodyName, meshRenderer.material.mainTexture);
            }

            meshRenderer.material.SetTexture("_MainTex", overlayTexture);

            //disable clouds and atmosphere
            body.GetChild("Fluffy Clouds(Scaled)")?.TryToggleMeshRendererComponent(false);
            body.GetChild("Wispy Clouds(Scaled)")?.TryToggleMeshRendererComponent(false);
            body.GetChild("Thick Cumulus Clouds(Scaled)")?.TryToggleMeshRendererComponent(false);
            body.GetChild("Cloud(Scaled)")?.TryToggleMeshRendererComponent(false);
            body.GetChild("Atmosphere.Inner")?.TryToggleMeshRendererComponent(false);
            body.GetChild("Atmosphere.Outer")?.TryToggleMeshRendererComponent(false);
        }

        public void RemoveMap3dOverlayOnAllLoadedBodies()
        {
            if (Utility.GameState?.GameState != GameState.Map3DView)
                return;

            var celestialBodies = GameManager.Instance.Game?.UniverseModel?.GetAllCelestialBodies();
            if (celestialBodies == null)
                return;

            foreach (var body in celestialBodies)
            {
                if (!_textureBackup.ContainsKey(body.Name))
                    continue;

                var bodyObj = OverlayUtility.FindMap3dBodyObject(body.Name);

                if (bodyObj == null)
                    continue;

                var meshRenderer = bodyObj.GetComponent<MeshRenderer>();

                if (meshRenderer == null)
                    continue;

                meshRenderer.material.SetTexture("_MainTex", _textureBackup[body.Name]);

                //enable clouds and atmosphere
                bodyObj.GetChild("Fluffy Clouds(Scaled)")?.TryToggleMeshRendererComponent(true);
                bodyObj.GetChild("Wispy Clouds(Scaled)")?.TryToggleMeshRendererComponent(true);
                bodyObj.GetChild("Thick Cumulus Clouds(Scaled)")?.TryToggleMeshRendererComponent(true);
                bodyObj.GetChild("Cloud(Scaled)")?.TryToggleMeshRendererComponent(true);
                bodyObj.GetChild("Atmosphere.Inner")?.TryToggleMeshRendererComponent(true);
                bodyObj.GetChild("Atmosphere.Outer")?.TryToggleMeshRendererComponent(true);

                _textureBackup.Remove(body.Name);

                if (Settings.ShowMapOverlayAlways.Value)
                {
                    DrawMap3dOverlayOnMapCelestialBodyAddedMessage(body.Name, 0);
                }
            }
        }
    }
}