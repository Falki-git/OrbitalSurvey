using SpaceWarp.API.UI;
using UnityEngine;
using BepInEx.Logging;
using KSP.Audio;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;

namespace OrbitalSurvey
{
    internal class DEBUG_UI
    {        
        public bool IsDebugWindowOpen;
        public const bool WILL_DEBUG_WINDOW_OPEN_ON_GAME_LOAD = false;

        private Rect _debugWindowRect = new Rect(1900, 500, 350, 350);
        private GUIStyle _labelStyle;
        private readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.DEBUG_UI");
        private string _myCustomTextureFilename = "kerbinCustom3_black.png";
        private string _textureName = string.Empty;
        private string _colorName = "red";
        private string _body = "Kerbin";
        private bool _showBiomeMask;
        private Texture2D _biomeTexture;
        
        private string _visualMapBody = "Kerbin";
        private bool _showVisualMap;
        private Texture2D _visualMapTexture;

        public string DataToSave = "default";
        public string LoadedData;


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

        public void InitializeStyles()
        {
            _labelStyle = new GUIStyle(Skins.ConsoleSkin.label) { fixedWidth = 150 };
        }

        public void OnGUI()
        {
            if (_labelStyle == null)
                return;

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
                GUILayout.Label("Body:", _labelStyle);
                _body = GUILayout.TextField(_body);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Import texture:", _labelStyle);
                _myCustomTextureFilename = GUILayout.TextField(_myCustomTextureFilename);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Apply to tex name:", _labelStyle);
                _textureName = GUILayout.TextField(_textureName);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Apply color:", _labelStyle);
                _colorName = GUILayout.TextField(_colorName);
            }
            GUILayout.EndHorizontal();

            /*
            GUILayout.Label("--");

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Save - Width:", _labelStyle);
                OrbitalSurveyPlugin.Instance.MySaveData.Width = int.Parse(GUILayout.TextField(OrbitalSurveyPlugin.Instance.MySaveData.Width.ToString()));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Save - Height:", _labelStyle);
                OrbitalSurveyPlugin.Instance.MySaveData.Height = int.Parse(GUILayout.TextField(OrbitalSurveyPlugin.Instance.MySaveData.Height.ToString()));
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Create texture in save file"))
                {
                    //OrbitalSurveyPlugin.Instance.MySaveData.TestBoolArray = new bool[OrbitalSurveyPlugin.Instance.MySaveData.Width, OrbitalSurveyPlugin.Instance.MySaveData.Height];
                    OrbitalSurveyPlugin.Instance.MySaveData.TestColorArray = new Color32[300, 300];
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Color test array"))
                {
                    for (int i = 0; i < 100; i++)
                        for (int j = 0; j < 300; j++)
                            OrbitalSurveyPlugin.Instance.MySaveData.TestColorArray[i, j] = Color.red;

                    for (int i = 101; i < 200; i++)
                        for (int j = 0; j < 300; j++)
                            OrbitalSurveyPlugin.Instance.MySaveData.TestColorArray[i, j] = Color.yellow;

                    for (int i = 201; i < 300; i++)
                        for (int j = 0; j < 300; j++)
                            OrbitalSurveyPlugin.Instance.MySaveData.TestColorArray[i, j] = Color.magenta;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Unregister for saving"))
                {
                    SpaceWarp.API.SaveGameManager.ModSaves.UnRegisterSaveLoadGameData("falki.orbital_survey");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Reregister"))
                {
                    SpaceWarp.API.SaveGameManager.ModSaves.ReregisterSaveLoadGameData(
                        "falki.orbital_survey",
                        (savedData) =>
                        {
                            // This function will be called when a SAVE event is triggered.
                            // If you don't need to do anything on save events, pass null instead of this function.

                            bool b = savedData.TestBool;
                            int i = savedData.TestInt;
                            string s = savedData.TestString;

                        },
                        (loadedData) =>
                        {
                            // This function will be called when a LOAD event is triggered and BEFORE data is loaded to your saveData object.
                            // If you don't need to do anything on load events, pass null instead of this function.

                            bool b = loadedData.TestBool;
                            int i = loadedData.TestInt;
                            string s = loadedData.TestString;
                        },
                        OrbitalSurveyPlugin.Instance.MySaveData
                    );
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("--");
            */

            if (GUILayout.Button("LoadMyCustomAssetTexture"))
            {
                DEBUG_Manager.Instance.LoadMyCustomAssetTexture();
            }

            if (GUILayout.Button("LoadTextureFromDisk"))
            {
                DEBUG_Manager.Instance.LoadTextureFromDisk(_myCustomTextureFilename);
            }

            GUILayout.Label("--");

            if (GUILayout.Button("ApplyMyCustomTextureToOverlay"))
            {
                DEBUG_Manager.Instance.ApplyMyCustomTextureToOverlay(_textureName, _body);
            }

            if (GUILayout.Button("RemoveCustomOverlay"))
            {
                DEBUG_Manager.Instance.RemoveCustomOverlay(_body);
            }

            if (GUILayout.Button("BlackOceanSphereMaterial"))
            {
                DEBUG_Manager.Instance.BlackOceanSphereMaterial(_myCustomTextureFilename, _body, _textureName);
            }

            if (GUILayout.Button("AddCustPaintedTexOverlay"))
            {
                DEBUG_Manager.Instance.AddCustPaintedTexOverlay(_colorName, _body);
            }            

            GUILayout.Label("--");

            // Doesn't work
            /*
            {
                if (GUILayout.Button("ApplyScaledSpaceMainTextureToOverlay"))
                {
                    DEBUG_Manager.Instance.ApplyScaledSpaceMainTextureToOverlay(_textureName, _body);
                }

                if (GUILayout.Button("ApplyGlobalHeightMap"))
                {
                    DEBUG_Manager.Instance.ApplyGlobalHeightMap(_body);
                }

                if (GUILayout.Button("ApplyBiomeMask"))
                {
                    DEBUG_Manager.Instance.ApplyBiomeMask(_body);
                }

                if (GUILayout.Button("LoadMunMaterial"))
                {
                    DEBUG_Manager.Instance.LoadMunMaterial();
                }

                if (GUILayout.Button("ApplyTexToCustMaterialToOverlay"))
                {
                    DEBUG_Manager.Instance.ApplyTexToCustMaterialToOverlay(_textureName, _body);
                }
            }

            GUILayout.Label("--");
            */

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
                Vector2 scaledSize = new Vector2(300, 300);
                float scaleWidth = scaledSize.x / _biomeTexture.width;
                float scaleHeight = scaledSize.y / _biomeTexture.height;

                int scaledWidth = (int)(_biomeTexture.width * scaleWidth);
                int scaledHeight = (int)(_biomeTexture.height * scaleHeight);

                GUILayout.Label(_biomeTexture, GUILayout.Width(scaledWidth), GUILayout.Height(scaledHeight));
            }

            if (GUILayout.Button("PaintTextureAtCurrentPosition"))
            {
                DEBUG_Manager.Instance.PaintTextureAtCurrentPosition();
            }

            GUILayout.Label($"Lat: {DEBUG_Manager.Instance.CurrentLatitude?.ToString()}");
            GUILayout.Label($"Lon: {DEBUG_Manager.Instance.CurrentLongitude?.ToString()}");
            
            GUILayout.Label("--");
            
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Visual map for:",  _labelStyle);
                _visualMapBody = GUILayout.TextField(_visualMapBody);
            }
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button(_showVisualMap ? "Hide visual map" : "Show visual map"))
            {
                if (!_showVisualMap)
                {
                    _visualMapTexture = Core.Instance.CelestialDataDictionary[_visualMapBody].Maps[MapType.Visual].ScannedMap;
                }
                else
                {
                    _visualMapTexture = null;
                }

                _showVisualMap = !_showVisualMap;
            }
            
            if (_showVisualMap)
            {
                Vector2 scaledSize = new Vector2(320, 320);
                float scaleWidth = scaledSize.x / _visualMapTexture.width;
                float scaleHeight = scaledSize.y / _visualMapTexture.height;

                int scaledWidth = (int)(_visualMapTexture.width * scaleWidth);
                int scaledHeight = (int)(_visualMapTexture.height * scaleHeight);

                GUILayout.Label(_visualMapTexture, GUILayout.Width(scaledWidth), GUILayout.Height(scaledHeight));
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
}
