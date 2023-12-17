using BepInEx.Logging;
using KSP.Game;
using KSP.Rendering.Planets;
using KSP.Sim.impl;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;
using SpaceWarp.API.Assets;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace OrbitalSurvey.Managers;

public class OverlayManager
{
    private OverlayManager()
    {
        Initialize();
    }
    
    public static OverlayManager Instance { get; } = new();

    public bool OverlayActive { get; set; }
    public MapType OverlayType { get; set; }
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.OverlayManager");

    private const string _OVERLAY_SHADER = "KSP2/Environment/CelestialBody/CelestialBody_Local_Old";
    private const string _OVERLAY_TEXTURE_NAME = "_AlbedoScaledTex";
    private const string _BLACK_OCEAN_TEXTURE_NAME = "_ShorelineSDFTexture";
    
    private Transform _celestialBody;
    private Texture2D _allBlack;
    private Texture _oceanTextureBackup;
    
    // original Map3d textures are stored here for when overlays will be turned off
    private Dictionary<string, Texture> _textureBackup = new();
    
    private VesselComponent _activeVessel => GameManager.Instance?.Game?.ViewController?.GetActiveVehicle()?.GetSimVessel();
    public string ActiveVesselBody => _activeVessel.mainBody.Name;
    
    private void Initialize()
    {
        _allBlack = AssetManager.GetAsset<Texture2D>($"{AssetUtility.OtherAssetsAddresses["AllBlack_4096"]}");
    }

    public bool DrawOverlay(MapType mapType)
    {
        bool isSuccess;
        
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
            return false;
        
        RemoveFlightOverlay();
        
        var pqs = _celestialBody.GetComponent<PQS>();

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
        pqsRenderer.AddOverlay(new OrbitalSurveyOverlay { OverlayMaterial = newMaterial });
        SetOceanSphereMaterialToBlack();

        return true;
    }
    
    private void RefreshCelestialBody()
    {
        var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
        _celestialBody = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, ActiveVesselBody);
    }
    
    private bool RemoveFlightOverlay()
    {
        if (_activeVessel == null)
            return false;
            
        RefreshCelestialBody();
        
        PQSRenderer pqsRenderer = _celestialBody.GetComponent<PQSRenderer>();

        if (pqsRenderer._overlays?.Count > 0)
        {
            var overlay = pqsRenderer._overlays.Find(o => o is OrbitalSurveyOverlay);

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

        _oceanTextureBackup = pqsRenderer._oceanMaterial?.GetTexture(_BLACK_OCEAN_TEXTURE_NAME);

        if (_oceanTextureBackup == null)
        {
            // body doesn't have an ocean; just return
            return;
        }
        
        pqsRenderer._oceanSpereMaterial.SetTexture(_BLACK_OCEAN_TEXTURE_NAME, _allBlack);
        pqsRenderer._oceanMaterial.SetTexture(_BLACK_OCEAN_TEXTURE_NAME, _allBlack);
    }
    
    private void RevertOceanSphereMaterial()
    {
        if (_oceanTextureBackup == null)
            return;
        
        var pqsRenderer = _celestialBody.GetComponent<PQSRenderer>();
            
        //pqsRenderer._oceanSpereMaterial.SetTexture(nameOfMaterialTextureToOverride, SavedTexture);
        pqsRenderer._oceanMaterial.SetTexture(_BLACK_OCEAN_TEXTURE_NAME, _oceanTextureBackup);
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

        foreach (var body in celestialBodies)
        {
            if (!Core.Instance.CelestialDataDictionary.ContainsKey(body.Name))
                continue;

            var overlayTexture = Core.Instance.CelestialDataDictionary[body.Name].Maps[mapType].CurrentMap;

            var bodyObj = GameObject.Find(OverlayUtility.MAP3D_CELESTIAL_PATH[body.Name]);

            if (bodyObj == null)
                continue;
            
            var meshRenderer = bodyObj.GetComponent<MeshRenderer>();
        
            if (!_textureBackup.ContainsKey(body.Name))
            {
                // backup the texture so it can be restored later when the overlay is turned off
                _textureBackup.Add(body.Name, meshRenderer.material.mainTexture);
            }
        
            meshRenderer.material.SetTexture("_MainTex", overlayTexture);
        
            //disable clouds and atmosphere, if the body has them
            bodyObj.GetChild("Fluffy Clouds(Scaled)")?.TryToggleMeshRendererComponent(false);
            bodyObj.GetChild("Wispy Clouds(Scaled)")?.TryToggleMeshRendererComponent(false);
            bodyObj.GetChild("Thick Cumulus Clouds(Scaled)")?.TryToggleMeshRendererComponent(false);
            bodyObj.GetChild("Cloud(Scaled)")?.TryToggleMeshRendererComponent(false);
            bodyObj.GetChild("Atmosphere.Inner")?.TryToggleMeshRendererComponent(false);
            bodyObj.GetChild("Atmosphere.Outer")?.TryToggleMeshRendererComponent(false);
        }
    }
    
    /// <summary>
    /// Called when OnMapCelestialBodyAddedMessage is triggered.
    /// This happens when we're in Map3d scene and when close enough that the scaled space texture is loaded.
    /// We're doing this after event triggers because user can often zoom out, which destroys the scaled space object,
    /// and them zoom back in which recreates the scaled space object and then the texture needs to be applied again.
    /// This is an async delayed task because it takes a while for clouds and atmosphere to be generated. 
    /// </summary>
    public async Task DrawMap3dOverlayOnMapCelestialBodyAddedMessage(string bodyName)
    {
        if (!OverlayActive)
            return;
        
        if (!Core.Instance.CelestialDataDictionary.ContainsKey(bodyName))
        {
            _LOGGER.LogError($"Body '{bodyName}' not found in the CelestialDataDictionary.");
            return;
        }

        var overlayTexture = Core.Instance.CelestialDataDictionary[bodyName].Maps[OverlayType].CurrentMap;

        // wait for the Map3d to receive its clouds and atmosphere 
        await Task.Delay(100);
        
        var body = GameObject.Find(OverlayUtility.MAP3D_CELESTIAL_PATH[bodyName]);
        var meshRenderer = body.GetComponent<MeshRenderer>();
        
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
            
            var bodyObj = GameObject.Find(OverlayUtility.MAP3D_CELESTIAL_PATH[body.Name]);
            
            if (bodyObj == null)
                continue;
            
            var meshRenderer = bodyObj.GetComponent<MeshRenderer>();
            
            meshRenderer.material.SetTexture("_MainTex", _textureBackup[body.Name]);
        
            //enable clouds and atmosphere
            bodyObj.GetChild("Fluffy Clouds(Scaled)")?.TryToggleMeshRendererComponent(true);
            bodyObj.GetChild("Wispy Clouds(Scaled)")?.TryToggleMeshRendererComponent(true);
            bodyObj.GetChild("Thick Cumulus Clouds(Scaled)")?.TryToggleMeshRendererComponent(true);
            bodyObj.GetChild("Cloud(Scaled)")?.TryToggleMeshRendererComponent(true);
            bodyObj.GetChild("Atmosphere.Inner")?.TryToggleMeshRendererComponent(true);
            bodyObj.GetChild("Atmosphere.Outer")?.TryToggleMeshRendererComponent(true);

            _textureBackup.Remove(body.Name);
        }
    }
}