using System;
using System.Collections.Generic;
using System.Linq;
using KSP.Game;
using KSP.Map;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;
using UnityEngine;

namespace OrbitalSurvey.UI
{
    /// <summary>
    /// Renders a scanning-cone pyramid in the map view for each vessel that is actively scanning.
    /// The apex sits at the vessel's map position; the four base corners are projected onto the
    /// planet surface at the current scan footprint width, aligned with the orbital ground track.
    /// Attached to the map's root transform by OrbitalSurveyPlugin when the map view opens,
    /// and destroyed when it closes.
    /// </summary>
    public class GroundTrackingRenderer : MonoBehaviour
    {
        public static GroundTrackingRenderer Instance { get; private set; }

        private bool _isMapViewActive;
        private float _nextDiagLog;
        private float _nextGeomLog;

        // Keyed by VesselStats so pyramids survive scan-mode changes on the same vessel.
        private readonly Dictionary<VesselManager.VesselStats, PyramidData> _pyramids = new();

        // Pre-allocated to avoid per-frame heap allocations (replacing LINQ Where/Except).
        private readonly List<VesselManager.VesselStats> _activeScanners = new();
        private readonly List<VesselManager.VesselStats> _toRemove = new();

        private ReduxLib.Logging.ILogger _logger;

        private static readonly Color VISUAL_FILL = new Color(0.00f, 0.85f, 0.25f, 0.20f);
        private static readonly Color BIOME_FILL  = new Color(0.30f, 0.70f, 1.00f, 0.20f);
        private static readonly Color VISUAL_LINE = new Color(0.00f, 0.85f, 0.25f, 0.70f);
        private static readonly Color BIOME_LINE  = new Color(0.30f, 0.70f, 1.00f, 0.70f);

        private const float LINE_WIDTH_BASE    = 0.012f;
        private const int   PYRAMID_VERT_COUNT = 5;
        private const int   LATERAL_POS_COUNT  = 7;  // c0→apex→c1→apex→c2→apex→c3
        private const int   BASE_POS_COUNT     = 4;  // rectangle: c0,c1,c2,c3 (loop closes c3→c0)

        private void Awake()
        {
            _logger = ReduxLib.ReduxLib.GetLogger($"OrbitalSurvey|{GetType().Name}");
            _logger.LogDebug("Awake");
            Instance = this;
        }

        private void OnDestroy()
        {
            _logger?.LogDebug("OnDestroy");
            if (Instance == this) Instance = null;
            DestroyAll();
        }

        /// <summary>
        /// Called by MessageListener on MapViewEnteredMessage / MapViewLeftMessage.
        /// Tears down all pyramids immediately on deactivation because map GameObjects
        /// are invalid outside the map view.
        /// </summary>
        public void SetMapViewActive(bool active)
        {
            _logger.LogDebug($"SetMapViewActive: {active}");
            _isMapViewActive = active;
            if (!active) DestroyAll();
        }

        private void Update()
        {
            if (!_isMapViewActive) return;
            if (!GameManager.Instance.Game.Map.TryGetMapCore(out var mapCore) ||
                mapCore.map3D == null || !mapCore.map3D.ViewAndCameraInitialized)
                return;

            var spaceProvider = mapCore.map3D.GetSpaceProvider();
            var mapRoot       = mapCore.map3D.transform;

            if (Time.time >= _nextDiagLog)
            {
                _nextDiagLog = Time.time + 5f;
                var vessels = VesselManager.Instance.OrbitalSurveyVessels;
                _logger.LogDebug($"[DIAG] vessels={vessels.Count}");
                foreach (var v in vessels)
                    foreach (var m in v.ModuleStats)
                        _logger.LogDebug($"[DIAG]   {v.Name}: status={m.DataModule?.StatusValue} mode={m.Mode}");
            }

            // Build the active-scanner list for this frame.
            _activeScanners.Clear();
            foreach (var v in VesselManager.Instance.OrbitalSurveyVessels)
            {
                foreach (var m in v.ModuleStats)
                {
                    if (m.DataModule?.StatusValue == Status.Scanning)
                    {
                        _activeScanners.Add(v);
                        break;
                    }
                }
            }

            // Remove pyramids whose vessels stopped scanning.
            _toRemove.Clear();
            foreach (var key in _pyramids.Keys)
                if (!_activeScanners.Contains(key))
                    _toRemove.Add(key);
            foreach (var key in _toRemove)
            {
                DestroyPyramid(_pyramids[key]);
                _pyramids.Remove(key);
            }

            foreach (var vs in _activeScanners)
            {
                try { UpdatePyramid(vs, mapCore, spaceProvider, mapRoot); }
                catch (Exception ex) { _logger.LogError($"UpdatePyramid exception: {ex}"); }
            }
        }

        /// <summary>
        /// Resolves map-space positions for the vessel and its parent body, computes the
        /// scan footprint size, then creates or updates the pyramid for this vessel.
        /// </summary>
        private void UpdatePyramid(
            VesselManager.VesselStats vs,
            MapCore mapCore,
            Map3DSpaceProvider spaceProvider,
            Transform mapRoot)
        {
            var vessel = vs.Vessel;
            var body   = vessel.mainBody;

            // Prefer map selectable item transforms (Unity-driven, consistent with other
            // map visuals) and fall back to the space provider for items not currently tracked.
            Vector3 vesselLocalPos, bodyLocalPos;
            bool vesselFound = mapCore.map3D.AllMapSelectableItems.TryGetValue(vessel.SimulationObject.GlobalId, out var vesselItem);
            if (vesselFound)
                vesselLocalPos = mapRoot.InverseTransformPoint(vesselItem.transform.position);
            else
                vesselLocalPos = (Vector3)spaceProvider.TranslateSimPositionToMapPosition(vessel.CenterOfMass);

            bool bodyFound = mapCore.map3D.AllMapSelectableItems.TryGetValue(body.SimulationObject.GlobalId, out var bodyItem);
            if (bodyFound)
                bodyLocalPos = mapRoot.InverseTransformPoint(bodyItem.transform.position);
            else
                bodyLocalPos = (Vector3)spaceProvider.TranslateSimPositionToMapPosition(body.Position);

            var nadirDir           = (vesselLocalPos - bodyLocalPos).normalized;
            var distVesselToCenter = (vesselLocalPos - bodyLocalPos).magnitude;

            // physTotalDist is the physical orbital radius (sea-level based) used as the
            // denominator when converting physical measurements to map-local units.
            var physTotalDist = body.radius + vessel.AltitudeFromSeaLevel;

            // Terrain at the nadir point may be above sea level. Derive terrain elevation
            // so the base rectangle sits on the visible terrain mesh, not below it.
            var terrainElevation  = Math.Max(0.0, vessel.AltitudeFromSeaLevel - vessel.AltitudeFromTerrain);
            var effectiveSurfaceR = body.radius + terrainElevation;

            // Scale the effective surface radius to map-local units using the map/physical ratio.
            var bodyRadiusLocal = (float)(distVesselToCenter * effectiveSurfaceR / physTotalDist);
            var surfacePoint    = bodyLocalPos + nadirDir * bodyRadiusLocal;

            var scanningModule = vs.ModuleStats.First(m => m.DataModule?.StatusValue == Status.Scanning);
            var scanStats      = scanningModule.DataModule.ScanningStats;
            var scanArcMeters  = ScanUtility.GetScanRadius(body.radius, vessel.AltitudeFromSeaLevel, scanStats);

            // Convert the physical scan arc to map-local units using the same ratio.
            var halfWidth = (float)(scanArcMeters * distVesselToCenter / physTotalDist);

            if (Time.time >= _nextGeomLog)
            {
                _nextGeomLog = Time.time + 3f;
                _logger.LogDebug(
                    $"[GEOM] vesselFound={vesselFound} bodyFound={bodyFound} " +
                    $"vessel={vesselLocalPos:F3} body={bodyLocalPos:F3} " +
                    $"dist={distVesselToCenter:F5} physTotal={physTotalDist:F0}m " +
                    $"bodyRadius={body.radius:F0}m altASL={vessel.AltitudeFromSeaLevel:F0}m " +
                    $"altTerrain={vessel.AltitudeFromTerrain:F0}m terrainElev={terrainElevation:F0}m " +
                    $"bodyR_local={bodyRadiusLocal:F5} surface={surfacePoint:F3} " +
                    $"FOV={scanStats.FieldOfView}° minAlt={scanStats.MinAltitude:F0}m " +
                    $"idealAlt={scanStats.IdealAltitude:F0}m maxAlt={scanStats.MaxAltitude:F0}m " +
                    $"scanArc={scanArcMeters:F1}m half={halfWidth:F5} " +
                    $"mapScale={spaceProvider.Map3DScaleInv:F0}");
            }

            bool isBiome = IsBiomeScanning(vs);
            if (!_pyramids.TryGetValue(vs, out var pyramid))
            {
                pyramid       = CreatePyramid(mapRoot, isBiome ? BIOME_FILL : VISUAL_FILL, isBiome ? BIOME_LINE : VISUAL_LINE);
                _pyramids[vs] = pyramid;
            }

            UpdatePyramidGeometry(pyramid, vesselLocalPos, nadirDir, bodyLocalPos, bodyRadiusLocal, halfWidth, isBiome);
        }

        private static bool IsBiomeScanning(VesselManager.VesselStats vs)
            => vs.ModuleStats.Any(m => m.Mode == MapType.Biome && m.DataModule?.StatusValue == Status.Scanning);

        /// <summary>
        /// Creates the Unity GameObjects, mesh, and line renderers for one pyramid.
        /// The base outline lives on a child GameObject because LineRenderer is
        /// [DisallowMultipleComponent] — only one per GameObject.
        /// </summary>
        private PyramidData CreatePyramid(Transform mapRoot, Color fillColor, Color lineColor)
        {
            _logger.LogDebug("CreatePyramid");

            var go   = new GameObject("OrbitalSurvey_ScanPyramid");
            go.layer = LayerMask.NameToLayer("Map");
            go.transform.SetParent(mapRoot, worldPositionStays: false);

            // Fill mesh: vertex 0 = apex, vertices 1-4 = base corners.
            // Triangle winding: 0,1,2 / 0,2,3 / 0,3,4 / 0,4,1 covers all four side faces.
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.material          = CreateFillMaterial(fillColor);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows    = false;

            var mesh = new Mesh { name = "ScanPyramid" };
            mf.mesh = mesh;
            var vertices  = new Vector3[PYRAMID_VERT_COUNT];
            var triangles = new int[] { 0, 1, 2,  0, 2, 3,  0, 3, 4,  0, 4, 1 };
            mesh.vertices  = vertices;
            mesh.triangles = triangles;

            // Lateral outline: c0→apex→c1→apex→c2→apex→c3
            var lateral           = go.AddComponent<LineRenderer>();
            lateral.positionCount = LATERAL_POS_COUNT;
            lateral.startWidth    = LINE_WIDTH_BASE;
            lateral.endWidth      = LINE_WIDTH_BASE;
            lateral.loop          = false;
            lateral.useWorldSpace = false;
            lateral.material      = CreateOutlineMaterial(lineColor, out var lateralMat);
            lateral.startColor    = lineColor;
            lateral.endColor      = lineColor;

            // Base outline on a separate child GO (LineRenderer is [DisallowMultipleComponent]).
            var baseGo   = new GameObject("OrbitalSurvey_ScanBase");
            baseGo.layer = LayerMask.NameToLayer("Map");
            baseGo.transform.SetParent(go.transform, worldPositionStays: false);

            var baseOutline           = baseGo.AddComponent<LineRenderer>();
            baseOutline.positionCount = BASE_POS_COUNT;
            baseOutline.loop          = true;
            baseOutline.startWidth    = LINE_WIDTH_BASE;
            baseOutline.endWidth      = LINE_WIDTH_BASE;
            baseOutline.useWorldSpace = false;
            baseOutline.material      = CreateOutlineMaterial(lineColor, out var baseMat);
            baseOutline.startColor    = lineColor;
            baseOutline.endColor      = lineColor;

            return new PyramidData
            {
                Go               = go,
                Mesh             = mesh,
                Vertices         = vertices,
                Lateral          = lateral,
                LateralPositions = new Vector3[LATERAL_POS_COUNT],
                BaseOutline      = baseOutline,
                BasePositions    = new Vector3[BASE_POS_COUNT],
                FillMaterial     = mr.material,
                LateralMaterial  = lateralMat,
                BaseMaterial     = baseMat,
            };
        }

        /// <summary>
        /// Updates colors, orientation, and vertex positions of an existing pyramid each frame.
        /// </summary>
        private static void UpdatePyramidGeometry(
            PyramidData p, Vector3 apex,
            Vector3 nadirDir, Vector3 bodyLocalPos, float bodyRadiusLocal, float halfWidth,
            bool isBiome)
        {
            // --- Per-frame visual updates ---
            var fillColor = isBiome ? BIOME_FILL  : VISUAL_FILL;
            var lineColor = isBiome ? BIOME_LINE  : VISUAL_LINE;

            // Pulse the fill alpha via material.color — no texture or Apply() needed because
            // Sprites/Default respects _Color directly.
            float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 1.5f);
            p.FillMaterial.color = new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a * pulse);

            // Guard: only write the six line-color properties when the scan mode actually changes.
            if (isBiome != p.IsBiome)
            {
                p.IsBiome                    = isBiome;
                p.LateralMaterial.color      = lineColor;
                p.Lateral.startColor         = lineColor;
                p.Lateral.endColor           = lineColor;
                p.BaseMaterial.color         = lineColor;
                p.BaseOutline.startColor     = lineColor;
                p.BaseOutline.endColor       = lineColor;
            }

            // --- Ground-track orientation ---
            // We derive the orbital plane normal h = r × Δr in body-relative map space.
            // Using body-relative position (relPos = apex - bodyLocalPos) keeps values
            // well-conditioned regardless of which object the camera is focused on: when
            // focused on the vessel, absolute map positions are small and the planet is far;
            // when focused on the planet, the inverse is true — but relPos always equals
            // the orbital radius vector and has consistent magnitude either way.
            //
            // Map item transforms update at a lower rate than Update(), so some frames
            // Δr ≈ 0. We skip those frames and keep the last good AngularMomentum.
            // For a Keplerian orbit h is constant, so we only need to refresh it every 1 s.
            // NextRotationUpdate starts at 0, so the first valid measurement is taken as
            // soon as the vessel's position actually changes, then the 1 s throttle kicks in.
            var relPos = apex - bodyLocalPos;
            if (Time.time >= p.NextRotationUpdate)
            {
                if (p.HasPrevPos)
                {
                    var dr = relPos - p.PrevRelPos;
                    var h  = Vector3.Cross(relPos, dr);
                    if (h.sqrMagnitude > 1e-30f)
                    {
                        p.AngularMomentum    = h.normalized;
                        p.NextRotationUpdate = Time.time + 1f;
                    }
                }
                p.PrevRelPos = relPos;
                p.HasPrevPos = true;
            }

            // Derive along/across each frame from the cached plane normal so the pyramid
            // tracks the correct orientation as the vessel moves between 1 s updates.
            // along = h × r̂ gives the prograde (velocity) direction; across is 90° to it.
            var outward = nadirDir;
            Vector3 along, across;
            if (p.AngularMomentum.sqrMagnitude > 1e-20f)
            {
                along  = Vector3.Cross(p.AngularMomentum, outward).normalized;
                across = Vector3.Cross(outward, along).normalized;
            }
            else
            {
                // Fallback used only on the very first frame before any valid h is measured.
                across = Vector3.Cross(Vector3.forward, outward);
                if (across.sqrMagnitude < 0.001f) across = Vector3.Cross(Vector3.up, outward);
                across.Normalize();
                along = Vector3.Cross(outward, across).normalized;
            }

            // --- Geometry ---
            var halfSide = halfWidth * 0.5f;

            var surfaceCenter = bodyLocalPos + outward * bodyRadiusLocal;

            // Tiny z-fight clearance (0.1 % of body radius) — terrain elevation in UpdatePyramid
            // already accounts for terrain above sea level; this just prevents depth aliasing.
            var surfaceLift = outward * (bodyRadiusLocal * 0.001f);

            // Four corners of the scan square in the surface tangent plane.
            var c0 = surfaceCenter + ( across + along) * halfSide + surfaceLift;
            var c1 = surfaceCenter + (-across + along) * halfSide + surfaceLift;
            var c2 = surfaceCenter + (-across - along) * halfSide + surfaceLift;
            var c3 = surfaceCenter + ( across - along) * halfSide + surfaceLift;

            // Base rectangle outline (loop=true closes c3→c0)
            p.BasePositions[0] = c0; p.BasePositions[1] = c1;
            p.BasePositions[2] = c2; p.BasePositions[3] = c3;
            p.BaseOutline.SetPositions(p.BasePositions);

            // Fill mesh — 4 triangular faces apex→corners
            p.Vertices[0] = apex; p.Vertices[1] = c0; p.Vertices[2] = c1;
            p.Vertices[3] = c2;   p.Vertices[4] = c3;
            p.Mesh.vertices = p.Vertices;
            p.Mesh.RecalculateBounds();

            // Lateral edges: c0→apex→c1→apex→c2→apex→c3
            p.LateralPositions[0] = c0;   p.LateralPositions[1] = apex; p.LateralPositions[2] = c1;
            p.LateralPositions[3] = apex; p.LateralPositions[4] = c2;
            p.LateralPositions[5] = apex; p.LateralPositions[6] = c3;
            p.Lateral.SetPositions(p.LateralPositions);
        }

        private void DestroyAll()
        {
            _logger?.LogDebug("DestroyAll");
            foreach (var p in _pyramids.Values) DestroyPyramid(p);
            _pyramids.Clear();
        }

        private static void DestroyPyramid(PyramidData p)
        {
            if (p.Go              != null) Destroy(p.Go);
            if (p.FillMaterial    != null) Destroy(p.FillMaterial);
            if (p.LateralMaterial != null) Destroy(p.LateralMaterial);
            if (p.BaseMaterial    != null) Destroy(p.BaseMaterial);
        }

        /// <summary>
        /// Creates the semi-transparent fill material. Sprites/Default is used because it
        /// respects the _Color property (including alpha), so the pulse is a free per-frame
        /// color assignment rather than a CPU-to-GPU texture upload.
        /// </summary>
        private static Material CreateFillMaterial(Color fillColor)
        {
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Transparent");
            var mat    = new Material(shader);
            mat.color = fillColor;
            mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            mat.SetInt("_Cull", 0);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay;
            return mat;
        }

        /// <summary>
        /// Creates a line outline material and returns it both as the return value (assigned
        /// to the LineRenderer) and via <paramref name="outMat"/> (stored in PyramidData for
        /// direct color updates). The separate reference avoids going through Unity's
        /// Renderer.material getter, which creates an instance copy.
        /// </summary>
        private static Material CreateOutlineMaterial(Color lineColor, out Material outMat)
        {
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            outMat = new Material(shader);
            outMat.color       = lineColor;
            outMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            outMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay;
            return outMat;
        }

        /// <summary>
        /// All Unity objects and orientation state for one active scanning pyramid.
        /// </summary>
        private class PyramidData
        {
            public GameObject   Go;
            public Mesh         Mesh;
            public Vector3[]    Vertices;
            public LineRenderer Lateral;
            public Vector3[]    LateralPositions;
            public LineRenderer BaseOutline;
            public Vector3[]    BasePositions;
            public Material     FillMaterial;
            public Material     LateralMaterial;
            public Material     BaseMaterial;
            public bool         IsBiome;
            public Vector3      PrevRelPos;           // vessel pos relative to body centre, map space
            public bool         HasPrevPos;
            public Vector3      AngularMomentum;      // orbital plane normal (r × Δr), map space
            public float        NextRotationUpdate;   // Time.time threshold for next h recompute
        }
    }
}
