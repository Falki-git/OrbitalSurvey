using BepInEx.Logging;
using KSP.Game;
using KSP.Rendering.Planets;
using KSP.Sim.impl;
using SpaceWarp.API.Assets;
using System.Reflection;
using UnityEngine;

namespace OrbitalSurvey
{
    internal class DEBUG_Manager
    {
        public MyIPQS MyOverlay;
        public Texture2D MyCustomTexture;
        public Material MyCustomMaterial;
        public Texture2D BiomeMask;

        private static readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.DEBUG_Manager");

        private static DEBUG_Manager _instance;
        internal static DEBUG_Manager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DEBUG_Manager();

                return _instance;
            }
        }        

        public void AddCustPaintedTexOverlay(string colorName, string body = "Kerbin")
        {
            RemoveCustomOverlay(body);

            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = Utility.FindObjectByNameRecursively(celestialRoot.transform, body);
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
            var celes = Utility.FindObjectByNameRecursively(celestialRoot.transform, body);

            PQSRenderer pqsRenderer = celes.GetComponent<PQSRenderer>();
            if (pqsRenderer._overlays?.Count > 0)
                pqsRenderer.RemoveOverlay(MyOverlay);
        }

        public void DrawCustomOverlays(string body = "Kerbin")
        {
            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = Utility.FindObjectByNameRecursively(celestialRoot.transform, body);
            var pqsRenderer = celes.GetComponent<PQSRenderer>();

            pqsRenderer.DrawPQSOverlays(pqsRenderer.SourceCamera);
            _logger.LogDebug("DrawCustomOverlays executed");
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
            var celes = Utility.FindObjectByNameRecursively(celestialRoot.transform, body);
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
            var pqsKerbin = Utility.FindObjectByNameRecursively(celestialRoot.transform, body);

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
            var celes = Utility.FindObjectByNameRecursively(celestialRoot.transform, body);
            var pqs = celes.GetComponent<PQS>();
            var heightMapInfo = pqs.data.heightMapInfo;

            heightMapInfo.globalHeightMap = MyCustomTexture;
        }

        public void ApplyBiomeMask(string body = "Kerbin")
        {
            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = Utility.FindObjectByNameRecursively(celestialRoot.transform, body);
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
            var celes = Utility.FindObjectByNameRecursively(celestialRoot.transform, "Mun");
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
            var celes = Utility.FindObjectByNameRecursively(celestialRoot.transform, body);

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
            var celes = Utility.FindObjectByNameRecursively(celestialRoot.transform, body);
            var pqsRenderer = celes.GetComponent<PQSRenderer>();
            var oceanMaterial = pqsRenderer._oceanSpereMaterial;

            var tex = Utility.ImportTexture(nameOfTextureToLoad);

            pqsRenderer._oceanSpereMaterial.SetTexture(nameOfMaterialTextureToOverride, tex);
            pqsRenderer._oceanMaterial.SetTexture(nameOfMaterialTextureToOverride, tex);
        }

        public void BuildBiomeMask(string body = "Kerbin")
        {
            BiomeMask = new Texture2D(4096, 4096, TextureFormat.ARGB32, true);

            var celestialRoot = GameObject.Find("#PhysicsSpace/#Celestial");
            var celes = Utility.FindObjectByNameRecursively(celestialRoot.transform, body);
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
                        case 0: pixelColor = Color.green; break; // red channel - grasslands
                        case 1: pixelColor = new Color(194f / 255f, 178 / 255f, 128 / 255f, 255); break; // green channel - sand OR ocean
                        case 2: pixelColor = Color.white; break; // blue channel - snow
                        case 3: pixelColor = new Color(133f / 255f, 94f / 255f, 66f / 255f); break; // alpha channel - mountains
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

        
    }
}
