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
    public class GroundTrackingRenderer : MonoBehaviour
    {
        public static GroundTrackingRenderer Instance { get; private set; }

        private bool _isMapViewActive;
        private float _nextDiagLog;
        private float _nextGeomLog;

        private readonly Dictionary<VesselManager.VesselStats, PyramidData> _pyramids = new();

        private ReduxLib.Logging.ILogger _logger;

        private static readonly Color VISUAL_FILL = new Color(0.00f, 0.85f, 0.25f, 0.20f);
        private static readonly Color BIOME_FILL  = new Color(0.30f, 0.70f, 1.00f, 0.20f);
        private static readonly Color VISUAL_LINE = new Color(0.00f, 0.85f, 0.25f, 0.70f);
        private static readonly Color BIOME_LINE  = new Color(0.30f, 0.70f, 1.00f, 0.70f);

        private const float LINE_WIDTH_BASE     = 0.012f;
        private const int   PYRAMID_VERT_COUNT = 5;
        private const int   LATERAL_POS_COUNT  = 7;  // c0→apex→c1→apex→c2→apex→c3
        private const int   BASE_POS_COUNT     = 4;  // rectangle: c0,c1,c2,c3 (loop closes c3→c0)

        private void Awake()
        {
            _logger = ReduxLib.ReduxLib.GetLogger("OrbitalSurvey.ScanningConeRenderer");
            _logger.LogInfo("Awake");
            Instance = this;
        }

        private void OnDestroy()
        {
            _logger?.LogInfo("OnDestroy");
            if (Instance == this) Instance = null;
            DestroyAll();
        }

        public void SetMapViewActive(bool active)
        {
            _logger.LogInfo($"SetMapViewActive: {active}");
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
                _logger.LogInfo($"[DIAG] vessels={vessels.Count}");
                foreach (var v in vessels)
                    foreach (var m in v.ModuleStats)
                        _logger.LogInfo($"[DIAG]   {v.Name}: status={m.DataModule?.StatusValue} mode={m.Mode}");
            }
    
            var activeScanners = VesselManager.Instance.OrbitalSurveyVessels
                .Where(v => v.ModuleStats.Any(m => m.DataModule?.StatusValue == Status.Scanning))
                .ToList();

            foreach (var v in _pyramids.Keys.Except(activeScanners).ToList())
            {
                DestroyPyramid(_pyramids[v]);
                _pyramids.Remove(v);
            }

            foreach (var vs in activeScanners)
            {
                try { UpdatePyramid(vs, mapCore, spaceProvider, mapRoot); }
                catch (Exception ex) { _logger.LogError($"UpdatePyramid exception: {ex}"); }
            }
        }

        private void UpdatePyramid(
            VesselManager.VesselStats vs,
            MapCore mapCore,
            Map3DSpaceProvider spaceProvider,
            Transform mapRoot)
        {
            var vessel = vs.Vessel;
            var body   = vessel.mainBody;

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

            var nadirDir               = (vesselLocalPos - bodyLocalPos).normalized;
            var distVesselToCenter     = (vesselLocalPos - bodyLocalPos).magnitude;

            // physTotalDist stays sea-level-based (used for scan arc width calculation).
            var physTotalDist  = body.radius + vessel.AltitudeFromSeaLevel;

            // Terrain at the nadir point may be above sea level.  Use AltitudeFromTerrain
            // (altitude above actual surface) to derive the terrain elevation so the base
            // rectangle sits on the visible terrain mesh, not below it.
            var terrainElevation = Math.Max(0.0, vessel.AltitudeFromSeaLevel - vessel.AltitudeFromTerrain);
            var effectiveSurfaceR = body.radius + terrainElevation;
            var bodyRadiusLocal   = (float)(distVesselToCenter * effectiveSurfaceR / physTotalDist);
            var surfacePoint      = bodyLocalPos + nadirDir * bodyRadiusLocal;

            var scanningModule = vs.ModuleStats.First(m => m.DataModule?.StatusValue == Status.Scanning);
            var scanStats      = scanningModule.DataModule.ScanningStats;
            var scanArcMeters  = ScanUtility.GetScanRadius(body.radius, vessel.AltitudeFromSeaLevel, scanStats);

            // scanArcMeters / physTotalDist gives the arc angle in map-space units relative to distVesselToCenter.
            var halfWidth = (float)(scanArcMeters * distVesselToCenter / physTotalDist);

            // Periodic diagnostics — runs every 3 s so the log captures live geometry values.
            if (Time.time >= _nextGeomLog)
            {
                _nextGeomLog = Time.time + 3f;
                _logger.LogInfo(
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

        private PyramidData CreatePyramid(Transform mapRoot, Color fillColor, Color lineColor)
        {
            _logger.LogInfo("CreatePyramid");

            var go   = new GameObject("OrbitalSurvey_ScanPyramid");
            go.layer = LayerMask.NameToLayer("Map");
            go.transform.SetParent(mapRoot, worldPositionStays: false);

            // Fill mesh — 4 triangular side faces, no base cap
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.material          = CreateFillMaterial(fillColor, out var fillTex);
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

            // Base rectangle outline on a child GO — LineRenderer is [DisallowMultipleComponent]
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
                FillTexture      = fillTex,
                LateralMaterial  = lateralMat,
                BaseMaterial     = baseMat,
            };
        }

        private static void UpdatePyramidGeometry(
            PyramidData p, Vector3 apex,
            Vector3 nadirDir, Vector3 bodyLocalPos, float bodyRadiusLocal, float halfWidth,
            bool isBiome)
        {
            // --- Per-frame visual updates (cheap uniform sets) ---
            var fillColor = isBiome ? BIOME_FILL  : VISUAL_FILL;
            var lineColor = isBiome ? BIOME_LINE  : VISUAL_LINE;

            // Pulse fill alpha with a slow sine (0.6–1.0 × base alpha).
            // Updated via the 1×1 texture pixel because Unlit/Transparent ignores mat.color.
            float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 1.5f);
            p.FillTexture.SetPixel(0, 0, new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a * pulse));
            p.FillTexture.Apply();

            // Refresh line color (handles live mode-change without recreating the pyramid).
            p.LateralMaterial.color      = lineColor;
            p.Lateral.startColor         = lineColor;
            p.Lateral.endColor           = lineColor;
            p.BaseMaterial.color         = lineColor;
            p.BaseOutline.startColor     = lineColor;
            p.BaseOutline.endColor       = lineColor;

            // --- Geometry ---
            // halfWidth = GetScanRadius in map-local units; scan square half-side = halfWidth/2.
            var halfSide = halfWidth * 0.5f;

            // Surface-local east and north directions at the nadir (sub-vessel) point.
            // "East" = tangent direction of increasing longitude (in the equatorial/orbital plane).
            // Using Vector3.forward (world Z ≈ orbital north) as the reference:
            //   east  = Cross(forward, outward)  → in the orbital (x-y) plane for equatorial orbits
            //   north = Cross(outward,  east)    → toward the planet's north pole
            // Both lie in the surface tangent plane; together they define the scan rectangle.
            var outward = nadirDir; // from body centre toward nadir surface point
            var east = Vector3.Cross(Vector3.forward, outward);
            if (east.sqrMagnitude < 0.001f) east = Vector3.Cross(Vector3.up, outward);
            east.Normalize();
            var north = Vector3.Cross(outward, east); north.Normalize();

            var surfaceCenter = bodyLocalPos + outward * bodyRadiusLocal;

            // Tiny z-fight clearance (0.1 % of body radius) — terrain elevation in UpdatePyramid
            // already accounts for terrain above sea level; this just prevents depth aliasing.
            var surfaceLift = outward * (bodyRadiusLocal * 0.001f);

            // Four corners of the scan square, placed in the surface tangent plane.
            var c0 = surfaceCenter + ( east + north) * halfSide + surfaceLift;
            var c1 = surfaceCenter + (-east + north) * halfSide + surfaceLift;
            var c2 = surfaceCenter + (-east - north) * halfSide + surfaceLift;
            var c3 = surfaceCenter + ( east - north) * halfSide + surfaceLift;

            // Base rectangle outline (loop=true closes c3→c0)
            p.BasePositions[0] = c0; p.BasePositions[1] = c1;
            p.BasePositions[2] = c2; p.BasePositions[3] = c3;
            p.BaseOutline.SetPositions(p.BasePositions);

            // Fill mesh — 4 triangular faces apex→corners
            p.Vertices[0] = apex; p.Vertices[1] = c0; p.Vertices[2] = c1;
            p.Vertices[3] = c2;   p.Vertices[4] = c3;
            p.Mesh.vertices = p.Vertices;
            p.Mesh.RecalculateBounds();
            p.Mesh.RecalculateNormals();

            // Lateral edges: c0→apex→c1→apex→c2→apex→c3
            p.LateralPositions[0] = c0;   p.LateralPositions[1] = apex; p.LateralPositions[2] = c1;
            p.LateralPositions[3] = apex; p.LateralPositions[4] = c2;
            p.LateralPositions[5] = apex; p.LateralPositions[6] = c3;
            p.Lateral.SetPositions(p.LateralPositions);
        }

        private void DestroyAll()
        {
            _logger?.LogInfo("DestroyAll");
            foreach (var p in _pyramids.Values) DestroyPyramid(p);
            _pyramids.Clear();
        }

        private static void DestroyPyramid(PyramidData p)
        {
            if (p.Go              != null) Destroy(p.Go);
            if (p.FillMaterial    != null) Destroy(p.FillMaterial);
            if (p.FillTexture     != null) Destroy(p.FillTexture);
            if (p.LateralMaterial != null) Destroy(p.LateralMaterial);
            if (p.BaseMaterial    != null) Destroy(p.BaseMaterial);
        }

        private static Material CreateFillMaterial(Color fillColor, out Texture2D tex)
        {
            // 1×1 texture whose pixel holds the fill color. Pulsing updates this pixel each frame;
            // Unlit/Transparent in KSP2 reads only _MainTex, not _Color, so the texture is the
            // only reliable way to control transparency.
            tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, fillColor);
            tex.Apply();

            var shader = Shader.Find("Unlit/Transparent") ?? Shader.Find("Sprites/Default");
            var mat    = new Material(shader);
            mat.mainTexture = tex;
            mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            mat.SetInt("_Cull", 0);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay;
            return mat;
        }

        private static Material CreateOutlineMaterial(Color lineColor, out Material outMat)
        {
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            outMat = new Material(shader);
            outMat.color       = lineColor;
            outMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            outMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay;
            return outMat;
        }

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
            public Texture2D    FillTexture;
            public Material     LateralMaterial;
            public Material     BaseMaterial;
        }
    }
}
