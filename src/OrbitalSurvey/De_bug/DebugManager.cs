using System.Reflection;
using BepInEx.Logging;
using KSP.Game;
using KSP.Game.Missions;
using KSP.Game.Missions.Definitions;
using KSP.Game.Missions.State;
using KSP.Game.Science;
using KSP.Messages;
using KSP.Rendering.Planets;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Game.Waypoints;
using UnityEngine;

namespace OrbitalSurvey.Debug
{
    internal class DebugManager
    {
        public MyIPQS MyOverlay;
        public Texture2D MyCustomTexture;
        public Material MyCustomMaterial;
        public Texture2D BiomeMask;
        public Texture SavedTexture;
        private Texture _textureBackup;
        private static readonly ManualLogSource _LOGGER = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.DEBUG_Manager");

        private static DebugManager _instance;
        internal static DebugManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DebugManager();

                return _instance;
            }
        }
        
        private PQSScienceOverlay _pqsScienceOverlay = new();
        public string RegionBody;

        public void AddCustPaintedTexOverlay(string colorName, string body = "Kerbin")
        {
            RemoveCustomOverlay(body);

            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, body);
            var pqs = celes.GetComponent<PQS>();
            var scaledMaterial = pqs._scaledMaterial;
            var scaledTexture = scaledMaterial.mainTexture;
            var scaledRenderer = pqs.ScaledRenderer;

            var colorToApply = Color.white;

            switch (colorName.ToLowerInvariant())
            {
                case "red": colorToApply = Color.red; break;
                case "green": colorToApply = Color.green; break;
                case "blue": colorToApply = Color.blue; break;
                case "white": colorToApply = Color.white; break;
                case "black": colorToApply = Color.black; break;
                case "yellow": colorToApply = Color.yellow; break;
                case "cyan": colorToApply = Color.cyan; break;
                case "gray": colorToApply = Color.gray; break;
                case "magenta": colorToApply = Color.magenta; break;
                case "clear": colorToApply = Color.clear; break;
                default: colorToApply = Color.white; break;
            }

            var newTexture = new Texture2D(500, 500, TextureFormat.ARGB32, true);
            for (int y = 0; y < newTexture.height; y++)
            {
                for (int x = 0; x < newTexture.width; x++)
                {
                    newTexture.SetPixel(x, y, colorToApply);
                }
            }
            newTexture.Apply();

            //var sourceMaterial = scaledRenderer.material;
            //var sourceMaterial = pqs.data.materialSettings.scaledSpaceMaterial;
            var sourceMaterial = pqs.data.materialSettings.surfaceMaterial;
            Material newMaterial = new Material(sourceMaterial);
            string[] shaderKeywords = sourceMaterial.shaderKeywords;
            string[] array = new string[shaderKeywords.Length];
            shaderKeywords.CopyTo(array, 0);
            newMaterial.shaderKeywords = array;
            //newMaterial.shader = Shader.Find("KSP2/Environment/CelestialBody/CelestialBody_Local");
            newMaterial.shader = Shader.Find("KSP2/Environment/CelestialBody/CelestialBody_Local_Old");

            //newMaterial.SetTexture("_MainTex", newTexture);
            //newMaterial.mainTexture = newTexture;
            newMaterial.SetTexture("_AlbedoScaledTex", newTexture);
            newMaterial.SetTexture("_ShorelineTex", newTexture);
            newMaterial.SetTexture("_NormalScaledTex", newTexture);
            newMaterial.SetTexture("_PackedScaledTex", newTexture);
            newMaterial.SetTexture("_BiomeMaskTex", newTexture);
            newMaterial.SetTexture("_SubzoneMaskTex", newTexture);
            newMaterial.SetTexture("_GlobalGradienceTex", newTexture);
            newMaterial.SetTexture("_GlobalCurvatureTex", newTexture);

            PQSRenderer pqsRenderer = celes.GetComponent<PQSRenderer>();
            MyOverlay = new MyIPQS { OverlayMaterial = newMaterial };
            pqsRenderer.AddOverlay(MyOverlay);
        }

        public void RemoveCustomOverlay(string body = "Kerbin")
        {
            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, body);

            PQSRenderer pqsRenderer = celes.GetComponent<PQSRenderer>();
            if (pqsRenderer._overlays?.Count > 0)
                pqsRenderer.RemoveOverlay(MyOverlay);
        }

        public void DrawCustomOverlays(string body = "Kerbin")
        {
            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, body);
            var pqsRenderer = celes.GetComponent<PQSRenderer>();

            pqsRenderer.DrawPQSOverlays(pqsRenderer.SourceCamera);
            _LOGGER.LogDebug("DrawCustomOverlays executed");
        }

        public void LoadMyCustomAssetTexture()
        {
            //LoadCustomTexture("myKerbinOverlay.png");
            //LoadCustomTexture("kerbinCustomTransparent2.png");
            LoadCustomAssetTexture("kerbin_scaled_d.png");
        }

        public void ApplyMyCustomTextureToOverlay(string applyTextureName, string body = "Kerbin")
        {
            RemoveCustomOverlay(body);

            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, body);
            var pqs = celes.GetComponent<PQS>();
            //var scaledMaterial = pqs._scaledMaterial;
            //var scaledTexture = scaledMaterial.mainTexture;
            //var scaledRenderer = pqs.ScaledRenderer;

            var sourceMaterial = pqs.data.materialSettings.surfaceMaterial;
            //var sourceMaterial = pqs.data.materialSettings.scaledSpaceMaterial;
            Material newMaterial = new Material(sourceMaterial);
            string[] shaderKeywords = sourceMaterial.shaderKeywords;
            string[] array = new string[shaderKeywords.Length];
            shaderKeywords.CopyTo(array, 0);
            newMaterial.shaderKeywords = array;
            newMaterial.shader = Shader.Find("KSP2/Environment/CelestialBody/CelestialBody_Local_Old");

            if (!string.IsNullOrEmpty(applyTextureName))
            {
                // _AlbedoScaledTex
                newMaterial.SetTexture(applyTextureName, MyCustomTexture);
            }
            else
            {
                newMaterial.SetTexture("_AlbedoScaledTex", MyCustomTexture);
                //newMaterial.SetTexture("_MainTex", MyCustomTexture);
            }

            PQSRenderer pqsRenderer = celes.GetComponent<PQSRenderer>();
            MyOverlay = new MyIPQS { OverlayMaterial = newMaterial };
            pqsRenderer.AddOverlay(MyOverlay);
        }

        /// <summary>
        /// Doesn't work
        /// </summary>
        public void ApplyScaledSpaceMainTextureToOverlay(string applyTextureName, string body = "Kerbin")
        {
            RemoveCustomOverlay(body);

            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var pqsKerbin = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, body);

            var scaledKerbin = GameObject.Find("#ScaledSpace/Celestial.Kerbin.Scaled.prefab");
            //var scaledPlanetarBodyView = scaledKerbin.GetComponent<ScaledPlanetaryBodyView>();
            var meshrenderer = scaledKerbin.GetComponent<MeshRenderer>();

            //var sourceMaterial = scaledPlanetarBodyView.Renderer.material;
            var sourceMaterial = meshrenderer.material;
            Material newMaterial = new Material(sourceMaterial);
            string[] shaderKeywords = sourceMaterial.shaderKeywords;
            string[] array = new string[shaderKeywords.Length];
            shaderKeywords.CopyTo(array, 0);
            newMaterial.shaderKeywords = array;
            newMaterial.shader = Shader.Find("KSP2/Environment/CelestialBody/CelestialBody_Local_Old");

            if (!string.IsNullOrEmpty(applyTextureName))
            {
                // _AlbedoScaledTex
                newMaterial.SetTexture(applyTextureName, MyCustomTexture);
            }

            PQSRenderer pqsRenderer = pqsKerbin.GetComponent<PQSRenderer>();
            MyOverlay = new MyIPQS { OverlayMaterial = newMaterial };
            pqsRenderer.AddOverlay(MyOverlay);
        }

        public void ApplyGlobalHeightMap(string body = "Kerbin")
        {
            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, body);
            var pqs = celes.GetComponent<PQS>();
            var heightMapInfo = pqs.data.heightMapInfo;

            heightMapInfo.globalHeightMap = MyCustomTexture;
        }

        public void ApplyBiomeMask(string body = "Kerbin")
        {
            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, body);
            var pqs = celes.GetComponent<PQS>();
            var heightMapInfo = pqs.data.heightMapInfo;

            heightMapInfo.mask = MyCustomTexture;
        }

        public void LoadCustomAssetTexture(string filenameWithExtension)
        {
            MyCustomTexture = AssetManager.GetAsset<Texture2D>($"{OrbitalSurveyPlugin.Instance.SpaceWarpMetadata.ModID}/images/{filenameWithExtension}");
        }

        public void LoadTextureFromDisk(string textureFilename)
        {
            MyCustomTexture = Utility.ImportTexture(textureFilename);
        }

        public void LoadMunMaterial()
        {
            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, "Mun");
            var pqs = celes.GetComponent<PQS>();

            var sourceMaterial = pqs.data.materialSettings.surfaceMaterial;
            Material newMaterial = new Material(sourceMaterial);
            string[] shaderKeywords = sourceMaterial.shaderKeywords;
            string[] array = new string[shaderKeywords.Length];
            shaderKeywords.CopyTo(array, 0);
            newMaterial.shaderKeywords = array;

            MyCustomMaterial = newMaterial;
        }

        public void ApplyTexToCustMaterialToOverlay(string applyTextureName, string body = "Kerbin")
        {
            RemoveCustomOverlay(body);

            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, body);

            if (!string.IsNullOrEmpty(applyTextureName))
            {
                // _AlbedoScaledTex
                MyCustomMaterial.SetTexture(applyTextureName, MyCustomTexture);
            }
            else
            {
                MyCustomMaterial.SetTexture("_AlbedoScaledTex", MyCustomTexture);
                //newMaterial.SetTexture("_MainTex", MyCustomTexture);
            }

            PQSRenderer pqsRenderer = celes.GetComponent<PQSRenderer>();
            MyOverlay = new MyIPQS { OverlayMaterial = MyCustomMaterial };
            pqsRenderer.AddOverlay(MyOverlay);
        }

        public void BlackOceanSphereMaterial(string nameOfTextureToLoad, string body, string nameOfMaterialTextureToOverride)
        {
            if (string.IsNullOrEmpty(nameOfTextureToLoad))
                nameOfTextureToLoad = "allblack.png";
            if (string.IsNullOrEmpty(body))
                body = "Kerbin";
            if (string.IsNullOrEmpty(nameOfMaterialTextureToOverride))
                nameOfMaterialTextureToOverride = "_ShorelineSDFTexture";

            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, body);
            var pqsRenderer = celes.GetComponent<PQSRenderer>();

            var tex = Utility.ImportTexture(nameOfTextureToLoad);

            SavedTexture = pqsRenderer._oceanMaterial.GetTexture(nameOfMaterialTextureToOverride);
            
            pqsRenderer._oceanSpereMaterial.SetTexture(nameOfMaterialTextureToOverride, tex);
            pqsRenderer._oceanMaterial.SetTexture(nameOfMaterialTextureToOverride, tex);
        }

        public void RevertOceanSphereMaterial(string body, string nameOfMaterialTextureToOverride)
        {
            if (string.IsNullOrEmpty(body))
                body = "Kerbin";
            if (string.IsNullOrEmpty(nameOfMaterialTextureToOverride))
                nameOfMaterialTextureToOverride = "_ShorelineSDFTexture";
            
            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, body);
            var pqsRenderer = celes.GetComponent<PQSRenderer>();
            
            //pqsRenderer._oceanSpereMaterial.SetTexture(nameOfMaterialTextureToOverride, SavedTexture);
            pqsRenderer._oceanMaterial.SetTexture(nameOfMaterialTextureToOverride, SavedTexture);
        }

        public void BuildBiomeMask(string body, Color? biome0, Color? biome1, Color? biome2, Color? biome3)
        {
            if (string.IsNullOrEmpty(body))
                body = "Kerbin";
            
            Color bio0 = biome0 ?? Color.green;
            Color bio1 = biome1 ?? new Color(194f / 255f, 178 / 255f, 128 / 255f, 255);
            Color bio2 = biome2 ?? Color.white;
            Color bio3 = biome3 ?? new Color(133f / 255f, 94f / 255f, 66f / 255f);
            
            BiomeMask = new Texture2D(4096, 4096, TextureFormat.ARGB32, true);

            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, body);
            var pqs = celes.GetComponent<PQS>();

            // Get biome mask pixel colors
            for (int i = 0; i < BiomeMask.width; i++)
            {
                for (int j = 0;  j < BiomeMask.height; j++)
                {
                    var pixelColor = pqs.GetBiomeMaskAtTextureCoordinate(new Vector2(i/4096f, j/4096f));
                    int biomeIndex = pqs.GetDominantBiomeIndexFromBiomeMask(pixelColor);

                    switch (biomeIndex)
                    {
                        case 0: pixelColor = bio0; break; // red channel - grasslands
                        case 1: pixelColor = bio1; break; // green channel - sand OR ocean
                        case 2: pixelColor = bio2; break; // blue channel - snow
                        case 3: pixelColor = bio3; break; // alpha channel - mountains
                        default: pixelColor = Color.black; break;
                    }
                    BiomeMask.SetPixel(i, j, pixelColor);
                }
            }
            
            // Get ocean pixel colors
            Texture2D oceanTexUnreadable = (Texture2D)pqs.data.materialSettings.surfaceMaterial.GetTexture("_ShorelineTex");

            if (oceanTexUnreadable != null)
            {
                Texture2D oceanTexCopy = ConvertToReadableTexture(oceanTexUnreadable);

                for (int i = 0; i < BiomeMask.width; i++)
                {
                    for (int j = 0; j < BiomeMask.height; j++)
                    {
                        var pixelColor = oceanTexCopy.GetPixel(i, j);
                        if (pixelColor == Color.red)
                        {
                            BiomeMask.SetPixel(i, j, Color.blue);
                        }
                    }
                }
            }

            BiomeMask.Apply();

            // Export biome mask to plugin folder
            byte[] bytes = BiomeMask.EncodeToPNG();
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "ExportedBiomeMask.png");
            File.WriteAllBytes(path, bytes);

            MyCustomTexture = BiomeMask;
        }

        private Texture2D ConvertToReadableTexture(Texture2D texture)
        {
            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(
                                texture.width,
                                texture.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);

            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            Texture2D myTexture2D = new Texture2D(texture.width, texture.height);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            // "myTexture2D" now has the same pixels from "texture" and it's readable
            return myTexture2D;
        }

        public VesselComponent ActiveVessel => GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
        public double? CurrentLatitude => ActiveVessel?.Latitude; // -90 S to +90 N
        public double? CurrentLongitude => ActiveVessel?.Longitude; // -180 W to +180E

        public void PaintTextureAtCurrentPosition()
        {
            if (CurrentLatitude == null || CurrentLongitude == null) return;

            double widthCoord = GetPixelPercentForGivenLongitude((double)CurrentLongitude);
            double heightCoord = GetPixelPercentForGivenLatitude((double)CurrentLatitude);

            int widthPixel = (int)(4096f * widthCoord);
            int heightPixel = (int)(4096f * heightCoord);

            for (int i = widthPixel - 50; i < widthPixel + 50; i++)
            {
                for (int j = heightPixel - 50; j < heightPixel + 50; j++)
                {
                    MyCustomTexture.SetPixel(i, j, Color.red);
                }
            }
            MyCustomTexture.Apply();

            ApplyMyCustomTextureToOverlay(string.Empty);
        }

        private double GetPixelPercentForGivenLatitude(double latitude)
        {
            return (latitude + 90) / 180f;
        }

        private double GetPixelPercentForGivenLongitude(double longitude)
        {
            return (longitude + 180) / 360f;
        }

        public void AddCurrentMapOverlay(string mapBody, MapType mapType, string applyTextureName)
        {
            RemoveCustomOverlay(mapBody);

            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, mapBody);
            var pqs = celes.GetComponent<PQS>();

            var sourceMaterial = pqs.data.materialSettings.surfaceMaterial;
            Material newMaterial = new Material(sourceMaterial);
            string[] shaderKeywords = sourceMaterial.shaderKeywords;
            string[] array = new string[shaderKeywords.Length];
            shaderKeywords.CopyTo(array, 0);
            newMaterial.shaderKeywords = array;
            newMaterial.shader = Shader.Find("KSP2/Environment/CelestialBody/CelestialBody_Local_Old");

            var mapTexture = Core.Instance.CelestialDataDictionary[mapBody].Maps[mapType].CurrentMap;
            
            if (!string.IsNullOrEmpty(applyTextureName))
            {
                // _AlbedoScaledTex
                newMaterial.SetTexture(applyTextureName, mapTexture);
            }
            else
            {
                newMaterial.SetTexture("_AlbedoScaledTex", mapTexture);
                //newMaterial.SetTexture("_MainTex", MyCustomTexture);
            }

            PQSRenderer pqsRenderer = celes.GetComponent<PQSRenderer>();
            MyOverlay = new MyIPQS { OverlayMaterial = newMaterial };
            pqsRenderer.AddOverlay(MyOverlay);
        }

        public void ClearMap(string mapBody, MapType mapType)
        {
            Core.Instance.ClearMap(mapBody, mapType);
        }

        public void ExportCurrentOverlayTexture(string body, string textureNameToExport)
        {
            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, body);
            
            PQSRenderer pqsRenderer = celes.GetComponent<PQSRenderer>();
            
            Texture2D textureToExport = (Texture2D)pqsRenderer._overlays[0].OverlayMaterial.GetTexture(
                string.IsNullOrEmpty(textureNameToExport) ? "_AlbedoScaledTex" : textureNameToExport);
            
            
            
            byte[] bytes = textureToExport.EncodeToPNG();
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "ExportedTexture.png");
            File.WriteAllBytes(path, bytes);
        }
        
        public void ReplaceMap3dTexture(string bodyName, string textureName)
        {
            var body = GameObject.Find(OverlayUtility.MAP3D_CELESTIAL_PATH[bodyName]);
            var meshRenderer = body.GetComponent<MeshRenderer>();

            _textureBackup = meshRenderer.material.GetTexture(textureName);
            meshRenderer.material.SetTexture(textureName, MyCustomTexture);
        }

        public void UndoReplaceMap3dTexture(string bodyName, string textureName)
        {
            var body = GameObject.Find(OverlayUtility.MAP3D_CELESTIAL_PATH[bodyName]);
            var meshRenderer = body.GetComponent<MeshRenderer>();
            
            meshRenderer.material.SetTexture(textureName, _textureBackup);
        }

        public List<(string, double)> GetVesselElectricityPercentages()
        {
            List<(string, double)> vesselEcUnits = new();
            
            var playerId = GameManager.Instance.Game.LocalPlayer.PlayerId;
            var vessels = GameManager.Instance.Game.ViewController.Universe.GetAllOwnedVessels(playerId).ToList();
            var ecId = GameManager.Instance.Game.ResourceDefinitionDatabase.GetResourceIDFromName("ElectricCharge");

            foreach (var vessel in vessels)
            {
                vesselEcUnits.Add((vessel.Name, vessel.GetControlOwner().PartOwner.ContainerGroup.GetResourceStoredUnits(ecId)));
            }

            return vesselEcUnits;
        }
        
        public void ResetPqsScienceOverlay()
        {
            _pqsScienceOverlay = new PQSScienceOverlay();
        }

        public void GetScienceRegionsMap(string mapBody = "Kerbin")
        {
            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = OverlayUtility.FindObjectByNameRecursively(celestialRoot.transform, mapBody);
            var pqs = celes.GetComponent<PQS>();

            var scienceRegionsProvider = GameManager.Instance.Game.ScienceManager.ScienceRegionsDataProvider;
            
            _pqsScienceOverlay.SetCelestialBody(pqs);
            _LOGGER.LogDebug($"PQSScienceOverlay: PQS set to {mapBody}");
            _pqsScienceOverlay.SetScienceRegionsDataProvider(scienceRegionsProvider);
            _LOGGER.LogDebug($"PQSScienceOverlay: ScienceRegionsData Provider set");

            RegionBody = mapBody;
        }

        public void DownloadScienceRegionsTexture()
        {
            _pqsScienceOverlay.Update();
            _LOGGER.LogDebug($"PQSScienceOverlay: updated");
            
            if (_pqsScienceOverlay._overlayTexture == null)
            {
                _LOGGER.LogDebug($"PQSScienceOverlay: overlay texture is null");
                return;
            }
            
            byte[] bytes = _pqsScienceOverlay._overlayTexture.EncodeToPNG();
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "ExportedScienceRegionsMap.png");
            File.WriteAllBytes(path, bytes);
            
            _LOGGER.LogDebug($"PQSScienceOverlay: overlay texture exported");
        }

        public List<Waypoint> Waypoints = new();

        public void CreateWaypoint(string name, string bodyName, double latitude, double longitude, double? altitudeFromRadius = 0)
        {
            var spaceSimulation = GameManager.Instance.Game.SpaceSimulation;
            var celestialBodies = GameManager.Instance.Game.UniverseModel.GetAllCelestialBodies();
            var body = celestialBodies.Find(c => c.Name == bodyName);
            if (body == null)
                return;
            
            if (altitudeFromRadius == 0)
            {
                altitudeFromRadius = body.SurfaceProvider.GetTerrainAltitudeFromCenter(latitude, longitude) - body.radius; 
            }
            
            // var waypointComponentDefinition = new WaypointComponentDefinition() { Name = name };
            // var waypoint = spaceSimulation.CreateWaypointSimObject(
            //     waypointComponentDefinition, body, latitude, longitude, altitudeFromRadius);
            //
            // Waypoints.Add(waypoint);
            
            Waypoints.Add(new Waypoint(latitude, longitude, altitudeFromRadius, bodyName, name, WaypointState.Visible));
        }

        public void MoveWaypoint(int waypointIndex, double latitude, double longitude, double? altitudeFromRadius = 0)
        {
            var waypoint = Waypoints[waypointIndex];
            waypoint.Move(latitude, longitude, altitudeFromRadius);
        }

        public void DeleteWaypoint(int waypointIndex)
        {
            var waypoint = Waypoints[waypointIndex];
            waypoint.Destroy();
        }
        
        public void ShowHideWaypoint(int waypointIndex, bool state)
        {
            var waypoint = Waypoints[waypointIndex];

            if (state)
            {
             waypoint.Show();   
            }
            else
            {
                waypoint.Hide();
            }
        }

        public void TriggerExperiment(string body = "Kerbin")
        {
            var experimentDefinition =
                GameManager.Instance.Game.ScienceManager.ScienceExperimentsDataStore.GetExperimentDefinition("orbital_survey_visual_mapping_high_25");
            
            var celestialScalar = GameManager.Instance.Game.ScienceManager.ScienceRegionsDataProvider.
                _cbToScienceRegions[body].SituationData.CelestialBodyScalar;
            var highOrbitScalar = GameManager.Instance.Game.ScienceManager.ScienceRegionsDataProvider.
                _cbToScienceRegions[body].SituationData.HighOrbitScalar;


            foreach (var vessel in VesselManager.Instance.OrbitalSurveyVessels.FindAll(v => v.Body == body))
            {
                var scienceModule = vessel.ModuleStats[0].DataModule.PartComponentModule.ModuleScienceExperiment;

                // scienceModule._currentLocation.RequiresRegion = false;
                // scienceModule._currentLocation.SetScienceRegion(null);
                // scienceModule._currentLocation.SetScienceSituation(ScienceSitutation.HighOrbit);
                
                ResearchReport researchReport = new ResearchReport(
                    experimentID: experimentDefinition.ExperimentID,
                    displayName: experimentDefinition.DataReportDisplayName,
                    scienceModule._currentLocation, // Fix me - set the location to the passed body
                    ScienceReportType.DataType,
                    initialScienceValue: experimentDefinition.DataValue * celestialScalar * highOrbitScalar,
                    flavorText: experimentDefinition.DataFlavorDescriptions[0].LocalizationTag
                );
                
                researchReport.Location.RequiresRegion = false;
                //researchReport.Location.SetScienceRegion($"{body}_HighOrbit");
                researchReport.Location.SetBodyName(body);
                researchReport.Location.SetScienceRegion(null);
                researchReport.Location.SetScienceSituation(ScienceSitutation.HighOrbit);
                
                //researchReport.Location._scienceRegion = null;
                //researchReport.Location._scienceSitutation = ScienceSitutation.HighOrbit;
                //researchReport.Location._researchLocationId = $"{body}_HighOrbit";

                researchReport.ResearchLocationID = researchReport.Location.ResearchLocationId;
            
                scienceModule._storageComponent.StoreResearchReport(researchReport);
            }            
            
            ResearchReportAcquiredMessage message;
            if (GameManager.Instance.Game.Messages.TryCreateMessage(out message))
            {
                GameManager.Instance.Game.Messages.Publish(message);
            }

            NotificationUtility.Instance.NotifyExperimentComplete(body, ExperimentLevel.Quarter);
        }

        public void ActivateMission(string missionId = "orbital_survey_01")
        {
            var activateMissionAction = new ActivateMissionAction();
            activateMissionAction.TargetMissionID = missionId;
            activateMissionAction.Activate();
        }
        
        public void CreateMissionGranter()
        {
            var granters = GameManager.Instance.Game.KSP2MissionManager.MissionGranterManager.MissionGranters;

            var granter = new Assets.Scripts.Missions.Definitions.MissionGranter();

            granter.NameKey = "Falki";
            granter.LogoKey = "Assets/Images/Icons/icon.png"; //"flag_MFG_1Proton.png";
            granter.LocalizationNameKey = "OrbitalSurvey/Granter/Name";
            granter.LocalizationDescriptionKey = "OrbitalSurvey/Granter/Description";
            granters.Add(granter);
        }

        public void OnMissionTriumphDismissed(MessageCenterMessage obj)
        {
            var message = obj as OnMissionTriumphDismissed;
        }

        public void LoadAsset(string path)
        {
            Texture2D x = null;
            
            GameManager.Instance.Assets.Load<Texture2D>(
                path,
                tex =>
                {
                    DebugUI.Instance.Asset = tex;
                }
            );

            int i = 0;
        }

        public void GetDiscoverableRegions(string bodyName = "Kerbin")
        {
            var regionProvider = GameManager.Instance.Game.ScienceManager.ScienceRegionsDataProvider;
            var cvToScienceRegions = regionProvider._cbToScienceRegions;
            var regions = cvToScienceRegions[bodyName].Regions;

            var body = Utility.GetAllCelestialBodies().Find(b => b.Name == bodyName);
            var discoverables = regionProvider._cbToScienceRegionDiscoverables[bodyName].Discoverables;
            
            _LOGGER.LogDebug($"{bodyName} discoverables:");
            foreach (var discoverable in discoverables)
            {
                var position = new Position(body.SimulationObject.transform.bodyFrame, discoverable.Position);
                body.GetLatLonAltFromRadius(position, out double lat, out double lon, out double altFromRadius);
                
                _LOGGER.LogDebug($"{discoverable.ScienceRegionId} LAT {lat}, LON {lon}, ALT {altFromRadius}");
            }
        }

        public List<MissionData> GetAllActiveMissions()
        {
            return GameManager.Instance.Game.KSP2MissionManager?.ActiveMissions[0]?.MissionDatas;
        }
        
        public void CompleteCurrentMissionStage(string missionId)
        {
            var manager = GameManager.Instance.Game.KSP2MissionManager;
            var missions = manager.ActiveMissions[0].MissionDatas;
            var mission = missions.Find(m => m.ID == missionId); 
            //mission.OnStageComplete();
            
            mission.FireOnStageCompletedMessage();
            mission.OnStageComplete();
        }
        
        public void CompleteMission(string missionId)
        {
            var manager = GameManager.Instance.Game.KSP2MissionManager;
            var missions = manager.ActiveMissions[0].MissionDatas;
            var mission = missions.Find(m => m.ID == missionId);
            
            manager.SetMissionState(missionId, MissionState.Complete, null);
        }

        public void AddNewMission()
        {
            //SaveLoadMissionUtils.AddOrOverwriteMissionData(base.Game, this._missionDefinitions, missionData);
        }
    }
}
