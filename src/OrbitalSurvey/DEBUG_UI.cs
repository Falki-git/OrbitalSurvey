using SpaceWarp.API.UI;
using UnityEngine.UI.Extensions;
using UnityEngine;
using BepInEx.Logging;

namespace OrbitalSurvey
{
    internal class DEBUG_UI
    {        
        public bool IsDebugWindowOpen;
        public const bool WILL_DEBUG_WINDOW_OPEN_ON_GAME_LOAD = true;

        private Rect _debugWindowRect = new Rect(1900, 500, 350, 350);
        private readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.DEBUG_UI");
        private string _myCustomTextureFilename = "test.png";
        private string _textureName = string.Empty;
        private string _body = "Kerbin";
        private bool _showBiomeMask;
        private Texture2D _biomeTexture;

        private static DEBUG_UI _instance;
        internal static DEBUG_UI Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DEBUG_UI();

                return _instance;
            }
        }

        public void OnGUI()
        {
            // Set the UI
            GUI.skin = Skins.ConsoleSkin;

            if (IsDebugWindowOpen)
            {
                _debugWindowRect = GUILayout.Window(
                    GUIUtility.GetControlID(FocusType.Passive),
                    _debugWindowRect,
                    FillDebugUI,
                    "// Orbital Survey",
                    GUILayout.Width(0),
                    GUILayout.Height(0)
                    );
            }
        }

        private void FillDebugUI(int _)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Body:", new GUIStyle(Skins.ConsoleSkin.label) { fixedWidth = 150 });
                _body = GUILayout.TextField(_body);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Imp tex:", new GUIStyle(Skins.ConsoleSkin.label) { fixedWidth = 150 });
                _myCustomTextureFilename = GUILayout.TextField(_myCustomTextureFilename);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Apply tex name:", new GUIStyle(Skins.ConsoleSkin.label) { fixedWidth = 150 });
                _textureName = GUILayout.TextField(_textureName);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("--");

            if (GUILayout.Button("AddCustPaintedTexOverlay"))
            {
                DEBUG_Manager.Instance.AddCustPaintedTexOverlay(_body);
            }
            if (GUILayout.Button("RemoveCustomOverlay"))
            {
                DEBUG_Manager.Instance.RemoveCustomOverlay(_body);
            }
            if (GUILayout.Button("DrawCustomOverlays"))
            {
                DEBUG_Manager.Instance.DrawCustomOverlays(_body);
            }
            if (GUILayout.Button("LoadMyCustomAssetTexture"))
            {
                DEBUG_Manager.Instance.LoadMyCustomAssetTexture();
            }

            GUILayout.Label("--");

            if (GUILayout.Button("ApplyMyCustomTextureToOverlay"))
            {
                DEBUG_Manager.Instance.ApplyMyCustomTextureToOverlay(_textureName, _body);
            }

            if (GUILayout.Button("LoadTextureFromDisk"))
            {
                DEBUG_Manager.Instance.LoadTextureFromDisk(_myCustomTextureFilename);
            }

            if (GUILayout.Button("ApplyScaledSpaceMainTextureToOverlay"))
            {
                DEBUG_Manager.Instance.ApplyScaledSpaceMainTextureToOverlay(_textureName, _body);
            }

            GUILayout.Label("--");

            if (GUILayout.Button("ApplyGlobalHeightMap"))
            {
                DEBUG_Manager.Instance.ApplyGlobalHeightMap(_body);
            }

            if (GUILayout.Button("ApplyBiomeMask"))
            {
                DEBUG_Manager.Instance.ApplyBiomeMask(_body);
            }

            GUILayout.Label("--");

            if (GUILayout.Button("LoadMunMaterial"))
            {
                DEBUG_Manager.Instance.LoadMunMaterial();
            }

            if (GUILayout.Button("ApplyTexToCustMaterialToOverlay"))
            {
                DEBUG_Manager.Instance.ApplyTexToCustMaterialToOverlay(_textureName, _body);
            }

            GUILayout.Label("--");

            if (GUILayout.Button("BlackOceanSphereMaterial"))
            {
                DEBUG_Manager.Instance.BlackOceanSphereMaterial(_textureName, _body);
            }

            GUILayout.Label("--");

            if (GUILayout.Button("BuildBiomeMask"))
            {
                if (!_showBiomeMask)
                {
                    DEBUG_Manager.Instance.BuildBiomeMask(_body);
                    _biomeTexture = DEBUG_Manager.Instance.BiomeMask;
                }
                else
                {
                    _biomeTexture = null;
                }

                _showBiomeMask = !_showBiomeMask;
            }

            if (_showBiomeMask)
            {
                //GUILayout.Label(_biomeTexture);

                Vector2 scaledSize = new Vector2(300, 300);
                float scaleWidth = scaledSize.x / _biomeTexture.width;
                float scaleHeight = scaledSize.y / _biomeTexture.height;

                int scaledWidth = (int)(_biomeTexture.width * scaleWidth);
                int scaledHeight = (int)(_biomeTexture.height * scaleHeight);

                GUILayout.Label(_biomeTexture, GUILayout.Width(scaledWidth), GUILayout.Height(scaledHeight));
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 500));
        }
    }
}
