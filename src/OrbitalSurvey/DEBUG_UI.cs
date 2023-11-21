using SpaceWarp.API.UI;
using UnityEngine;
using BepInEx.Logging;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;

namespace OrbitalSurvey
{
    internal class DEBUG_UI
    {        
        public bool IsDebugWindowOpen;

        private Rect _debugWindowRect = new Rect(1900, 500, 350, 350);
        private GUIStyle _labelStyle;
        private GUIStyle _labelStyleShort;
        private GUIStyle _normalButton;
        private GUIStyle _normalSectionButton;
        private GUIStyle _toggledSectionButton;
        private readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.DEBUG_UI");
        private string _myCustomTextureFilename = "allblack.png";
        private string _textureName = string.Empty;
        private string _colorName = "red";
        private string _body = "Kerbin";

        private bool _showSavePersistenceSection;
        private bool _showOverlaySection;
        private bool _showBuildBiomeSection;
        private bool _showPaintTextureSection;
        private bool _showDisplayMapVisualsSection;
        
        private bool _showBiomeMask;
        private Texture2D _biomeTexture;
        private string _biomeR0;
        private string _biomeG0;
        private string _biomeB0;
        private string _biomeA0;
        private string _biomeR1;
        private string _biomeG1;
        private string _biomeB1;
        private string _biomeA1;
        private string _biomeR2;
        private string _biomeG2;
        private string _biomeB2;
        private string _biomeA2;
        private string _biomeR3;
        private string _biomeG3;
        private string _biomeB3;
        private string _biomeA3;
        
        private string _mapBody = "Kerbin";
        private bool _showMapDisplay;
        private Texture2D _MapTexture;
        private int _mapTypeIndex;
        private int _mapDataIndex;

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
            _labelStyleShort = new GUIStyle(Skins.ConsoleSkin.label) { fixedWidth = 10 };
            _normalButton = new GUIStyle(Skins.ConsoleSkin.button);
            _normalSectionButton = new GUIStyle(Skins.ConsoleSkin.button);
            _normalSectionButton.normal.textColor = new Color(120f/255f, 100f/255f, 255f/255f, 1f);
            _toggledSectionButton = new GUIStyle(Skins.ConsoleSkin.button);
            _toggledSectionButton.normal.textColor = Color.gray;
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
            
            GUILayout.Label("--");
            
            //////////////////////////////////////////////////////////////////////////////////////////
            
            if (GUILayout.Button(_showSavePersistenceSection ? "Hide Save Persistence Section" : "Show Save Persistence Section", _showSavePersistenceSection ? _toggledSectionButton : _normalSectionButton))
                _showSavePersistenceSection = !_showSavePersistenceSection;
            
            if(_showSavePersistenceSection)
            {
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
            }
            
            //////////////////////////////////////////////////////////////////////////////////////////
            
            if (GUILayout.Button(_showOverlaySection ? "Hide Overlay Section" : "Show Overlay Section", _showOverlaySection ? _toggledSectionButton : _normalSectionButton))
                _showOverlaySection = !_showOverlaySection;

            if (_showOverlaySection)
            {
                GUILayout.Label("--");
                
                if (GUILayout.Button("LoadMyCustomAssetTexture"))
                {
                    DEBUG_Manager.Instance.LoadMyCustomAssetTexture();
                }

                if (GUILayout.Button("LoadTextureFromDisk"))
                {
                    DEBUG_Manager.Instance.LoadTextureFromDisk(_myCustomTextureFilename);
                }

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
                
                if (GUILayout.Button("AddCurrentMapOverlay"))
                {
                    DEBUG_Manager.Instance.AddCurrentMapOverlay(_mapBody, _mapTypeIndex == 0 ? MapType.Visual : MapType.Biome, _textureName);
                }
                
                GUILayout.Label("--");
            }

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
            
            //////////////////////////////////////////////////////////////////////////////////////////
            
            if (GUILayout.Button(_showBuildBiomeSection ? "Hide Build Biome Section" : "Show Build Biome Section", _showBuildBiomeSection ? _toggledSectionButton : _normalSectionButton))
                _showBuildBiomeSection = !_showBuildBiomeSection;

            if (_showBuildBiomeSection)
            {
                GUILayout.Label("--");
                
                if (GUILayout.Button("BuildBiomeMask"))
                {
                    if (!_showBiomeMask)
                    {
                        Color? biome0 = null;
                        Color? biome1 = null;
                        Color? biome2 = null;
                        Color? biome3 = null;
                        if (!string.IsNullOrEmpty(_biomeR0))
                        {
                            biome0 = new Color(float.Parse(_biomeR0), float.Parse(_biomeG0), float.Parse(_biomeB0), float.Parse(_biomeA0));
                            biome1 = new Color(float.Parse(_biomeR1), float.Parse(_biomeG1), float.Parse(_biomeB1), float.Parse(_biomeA1));
                            biome2 = new Color(float.Parse(_biomeR2), float.Parse(_biomeG2), float.Parse(_biomeB2), float.Parse(_biomeA2));
                            biome3 = new Color(float.Parse(_biomeR3), float.Parse(_biomeG3), float.Parse(_biomeB3), float.Parse(_biomeA3));
                        }
                        
                        DEBUG_Manager.Instance.BuildBiomeMask(_body, biome0, biome1, biome2, biome3);
                        _biomeTexture = DEBUG_Manager.Instance.BiomeMask;
                    }
                    else
                    {
                        _biomeTexture = null;
                    }

                    _showBiomeMask = !_showBiomeMask;
                }
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("R", _labelStyleShort);
                    _biomeR0 = GUILayout.TextField(_biomeR0);
                    
                    GUILayout.Label("G", _labelStyleShort);
                    _biomeG0 = GUILayout.TextField(_biomeG0);
                    
                    GUILayout.Label("B", _labelStyleShort);
                    _biomeB0 = GUILayout.TextField(_biomeB0);
                    
                    GUILayout.Label("A", _labelStyleShort);
                    _biomeA0 = GUILayout.TextField(_biomeA0);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("R", _labelStyleShort);
                    _biomeR1 = GUILayout.TextField(_biomeR1);
                    
                    GUILayout.Label("G", _labelStyleShort);
                    _biomeG1 = GUILayout.TextField(_biomeG1);
                    
                    GUILayout.Label("B", _labelStyleShort);
                    _biomeB1 = GUILayout.TextField(_biomeB1);
                    
                    GUILayout.Label("A", _labelStyleShort);
                    _biomeA1 = GUILayout.TextField(_biomeA1);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("R", _labelStyleShort);
                    _biomeR2 = GUILayout.TextField(_biomeR2);
                    
                    GUILayout.Label("G", _labelStyleShort);
                    _biomeG2 = GUILayout.TextField(_biomeG2);
                    
                    GUILayout.Label("B", _labelStyleShort);
                    _biomeB2 = GUILayout.TextField(_biomeB2);
                    
                    GUILayout.Label("A", _labelStyleShort);
                    _biomeA2 = GUILayout.TextField(_biomeA2);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("R", _labelStyleShort);
                    _biomeR3 = GUILayout.TextField(_biomeR3);
                    
                    GUILayout.Label("G", _labelStyleShort);
                    _biomeG3 = GUILayout.TextField(_biomeG3);
                    
                    GUILayout.Label("B", _labelStyleShort);
                    _biomeB3 = GUILayout.TextField(_biomeB3);
                    
                    GUILayout.Label("A", _labelStyleShort);
                    _biomeA3 = GUILayout.TextField(_biomeA3);
                }
                GUILayout.EndHorizontal();

                if (_showBiomeMask)
                {
                    Vector2 scaledSize = new Vector2(300, 300);
                    float scaleWidth = scaledSize.x / _biomeTexture.width;
                    float scaleHeight = scaledSize.y / _biomeTexture.height;

                    int scaledWidth = (int)(_biomeTexture.width * scaleWidth);
                    int scaledHeight = (int)(_biomeTexture.height * scaleHeight);

                    GUILayout.Label(_biomeTexture, GUILayout.Width(scaledWidth), GUILayout.Height(scaledHeight));
                }
                
                GUILayout.Label("--");
            }
            
            //////////////////////////////////////////////////////////////////////////////////////////
            
            if (GUILayout.Button(_showPaintTextureSection ? "Hide Paint Texture Section" : "Show Paint Texture Section", _showPaintTextureSection ? _toggledSectionButton : _normalSectionButton))
                _showPaintTextureSection = !_showPaintTextureSection;

            if (_showPaintTextureSection)
            {
                GUILayout.Label("--");
                
                if (GUILayout.Button("PaintTextureAtCurrentPosition"))
                {
                    DEBUG_Manager.Instance.PaintTextureAtCurrentPosition();
                }

                GUILayout.Label($"Lat: {DEBUG_Manager.Instance.CurrentLatitude?.ToString()}");
                GUILayout.Label($"Lon: {DEBUG_Manager.Instance.CurrentLongitude?.ToString()}");
                
                GUILayout.Label("--");
            }
            
            //////////////////////////////////////////////////////////////////////////////////////////
            
            if (GUILayout.Button(_showDisplayMapVisualsSection ? "Hide Display Maps Section" : "Show Display Maps Section", _showDisplayMapVisualsSection ? _toggledSectionButton : _normalSectionButton))
                _showDisplayMapVisualsSection = !_showDisplayMapVisualsSection;

            if (_showDisplayMapVisualsSection)
            {
                GUILayout.Label("--");
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Body:", _labelStyle);
                    _mapBody = GUILayout.TextField(_mapBody);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                {
                    switch (_mapTypeIndex)
                    {
                        case 0:
                            GUILayout.Label("Map Type: [Visual]", _labelStyle);
                            break;
                        case 1:
                            GUILayout.Label("Map Type: [Biome]", _labelStyle);
                            break;
                    }
                    
                    if (GUILayout.Button("<"))
                        if (_mapTypeIndex > 0)
                            _mapTypeIndex--;
                    
                    if (GUILayout.Button(">"))
                        if (_mapTypeIndex < 1)
                            _mapTypeIndex++;
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                {
                    switch (_mapDataIndex)
                    {
                        case 0:
                            GUILayout.Label("Map Tex: [Scanned]", _labelStyle);
                            break;
                        case 1:
                            GUILayout.Label("Map Tex: [Hidden]", _labelStyle);
                            break;
                        case 2:
                            GUILayout.Label("Map Tex: [Current]", _labelStyle);
                            break;
                    }
                    
                    if (GUILayout.Button("<"))
                        if (_mapDataIndex > 0)
                            _mapDataIndex--;
                    
                    if (GUILayout.Button(">"))
                        if (_mapDataIndex < 2)
                            _mapDataIndex++;
                }
                GUILayout.EndHorizontal();
                
                if (GUILayout.Button(_showMapDisplay ? "Hide map" : "Show map"))
                {
                    if (!_showMapDisplay)
                    {
                        if (_mapTypeIndex == 0)
                        {
                            switch (_mapDataIndex)
                            {
                                case 0:
                                    _MapTexture = Core.Instance.CelestialDataDictionary[_mapBody].Maps[MapType.Visual].ScannedMap;
                                    break;
                                case 1:
                                    _MapTexture = Core.Instance.CelestialDataDictionary[_mapBody].Maps[MapType.Visual].HiddenMap;
                                    break;
                                case 2:
                                    _MapTexture = Core.Instance.CelestialDataDictionary[_mapBody].Maps[MapType.Visual].CurrentMap;
                                    break;
                            }
                        }
                        else if (_mapTypeIndex == 1)
                        {
                            switch (_mapDataIndex)
                            {
                                case 0:
                                    _MapTexture = Core.Instance.CelestialDataDictionary[_mapBody].Maps[MapType.Biome].ScannedMap;
                                    break;
                                case 1:
                                    _MapTexture = Core.Instance.CelestialDataDictionary[_mapBody].Maps[MapType.Biome].HiddenMap;
                                    break;
                                case 2:
                                    _MapTexture = Core.Instance.CelestialDataDictionary[_mapBody].Maps[MapType.Biome].CurrentMap;
                                    break;
                            }
                        }
                        
                    }
                    else
                    {
                        _MapTexture = null;
                    }

                    _showMapDisplay = !_showMapDisplay;
                }
                
                if (_showMapDisplay)
                {
                    Vector2 scaledSize = new Vector2(320, 320);
                    float scaleWidth = scaledSize.x / _MapTexture.width;
                    float scaleHeight = scaledSize.y / _MapTexture.height;

                    int scaledWidth = (int)(_MapTexture.width * scaleWidth);
                    int scaledHeight = (int)(_MapTexture.height * scaleHeight);

                    GUILayout.Label(_MapTexture, GUILayout.Width(scaledWidth), GUILayout.Height(scaledHeight));
                }

                if (GUILayout.Button("Clear map"))
                {
                    DEBUG_Manager.Instance.ClearMap(_mapBody, _mapTypeIndex == 0 ? MapType.Visual : MapType.Biome);
                }
                
                GUILayout.Label("--");
            }
            

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
}
