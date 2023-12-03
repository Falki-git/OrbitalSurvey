using KSP.Game;
using KSP.Rendering.Planets;
using KSP.Sim.impl;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;
using SpaceWarp.API.Assets;
using UnityEngine;

namespace OrbitalSurvey.Managers;

public class OverlayManager
{
    private OverlayManager()
    {
        Initialize();
    }
    
    public static OverlayManager Instance { get; } = new();

    private const string _OVERLAY_SHADER = "KSP2/Environment/CelestialBody/CelestialBody_Local_Old";
    private const string _OVERLAY_TEXTURE_NAME = "_AlbedoScaledTex";
    private const string _BLACK_OCEAN_TEXTURE_NAME = "_ShorelineSDFTexture";
    
    private OrbitalSurveyOverlay _overlay;
    private Transform _celestialBody;
    private Texture2D _allBlack;
    private Texture _oceanTextureBackup;
    
    private VesselComponent _activeVessel => GameManager.Instance?.Game?.ViewController?.GetActiveVehicle()?.GetSimVessel();
    public string ActiveVesselBody => _activeVessel.mainBody.Name;
    
    private void Initialize()
    {
        _allBlack = AssetManager.GetAsset<Texture2D>($"{AssetUtility.OtherAssetsAddresses["AllBlack_4096"]}");
    }

    public bool DrawOverlay(MapType mapType)
    {
        if (_activeVessel == null)
            return false;
        
        RemoveOverlay();
        
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
    
    public bool RemoveOverlay()
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
                return true;
            }
        }

        return false;
    }

    private void RefreshCelestialBody()
    {
        var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
        _celestialBody = Utility.FindObjectByNameRecursively(celestialRoot.transform, ActiveVesselBody);
    }
    
    private void SetOceanSphereMaterialToBlack()
    {
        var pqsRenderer = _celestialBody.GetComponent<PQSRenderer>();

        _oceanTextureBackup = pqsRenderer._oceanMaterial.GetTexture(_BLACK_OCEAN_TEXTURE_NAME);
            
        pqsRenderer._oceanSpereMaterial.SetTexture(_BLACK_OCEAN_TEXTURE_NAME, _allBlack);
        pqsRenderer._oceanMaterial.SetTexture(_BLACK_OCEAN_TEXTURE_NAME, _allBlack);
    }
    
    private void RevertOceanSphereMaterial()
    {
        var pqsRenderer = _celestialBody.GetComponent<PQSRenderer>();
            
        //pqsRenderer._oceanSpereMaterial.SetTexture(nameOfMaterialTextureToOverride, SavedTexture);
        pqsRenderer._oceanMaterial.SetTexture(_BLACK_OCEAN_TEXTURE_NAME, _oceanTextureBackup);
    }
}