using BepInEx.Logging;
using KSP.Game;
using KSP.Game.Missions.Definitions;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;
using SpaceWarp.API.Game.Waypoints;
using SpaceWarp.API.UI;
using UnityEngine;
using Utility = OrbitalSurvey.Utilities.Utility;
#pragma warning disable CS0414 // Field is assigned but its value is never used

#pragma warning disable CS0618 // Type or member is obsolete

namespace OrbitalSurvey.Debug
{
    internal class DebugUI
    {
        public bool IsDebugWindowOpen;

        private Rect _debugWindowRect = new Rect(1800/*115*/, 54 /*54*/, 350, 350);
        private GUIStyle _labelStyle;
        private GUIStyle _labelMissionTableStyle;
        private GUIStyle _labelStyleShort;
        private GUIStyle _narrowLabel;
        private GUIStyle _normalButton;
        private GUIStyle _normalSectionButton;
        private GUIStyle _toggledSectionButton;
        private GUIStyle _narrowButton;
        private GUIStyle _normalTextfield;
        private readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.DEBUG_UI");
        private string _myCustomTextureFilename = "allblack.png";
        private string _myCustomTextureName = string.Empty;
        private string _textureName = string.Empty;
        private string _colorName = "red";
        private string _body;
        private string _missionBody;
        public string UT;
        private List<string> _bodyNames = new();
        private List<string> _missionBodyNames = new();
        private int _bodyIndex;
        private int _missionBodyIndex;
        
        private bool _showAnalyticsScanningSection;
        //private bool _showSavePersistenceSection;
        private bool _showOverlaySection = false;
        private bool _showBuildBiomeSection;
        private bool _showPaintTextureSection;
        private bool _showDisplayMapVisualsSection = false;
        private bool _showMap3dSection = false;
        private bool _showResourceConsumption = false;
        private bool _showPamOverridesSection = false;
        private bool _showScienceRegionsSection = false;
        private bool _showWaypointSection = false;
        private bool _showMissionSection = true;
        
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
        private int _mapTypeIndex = 1;
        private int _mapDataIndex = 2;

        private bool _isLifeSupportRegistered = true;
        private bool _isOrbitalSurveyRegistered = true;

        //public string DataToSave = "default";
        //public string LoadedData;

        public bool BufferAnalyticsScan;
        
        public bool DebugFovEnabled;
        public bool DebugTriggerExperiment;
        public bool DebugTriggerScienceReport;
        
        // waypoints
        private string _waypointBody;
        private List<string> _waypointBodyNames = new();
        private int _waypointBodyIndex;
        private string _waypointName = "Waypoint Test";
        private string _waypointLatitude = "0";
        private string _waypointLongitude = "0";
        private string _waypointAltitudeFromRadius = "0";
        private int _waypointIndex;
        private string _waypointNameExisting = "n/a";
        
        // missions
        private string _assetName = "Assets/Images/Icons/icon.png";
        public Texture2D Asset;
        private bool _showActiveMissions = false;
        private bool _showMissionDetails = true;
        private string _missionId = "orbital_survey_02";
        private int _missionIndex;
        private string _stageToActivate = "0";

        private static DebugUI _instance;
        internal static DebugUI Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DebugUI();

                return _instance;
            }
        }

        public void InitializeStyles()
        {
            _labelStyle = new GUIStyle(Skins.ConsoleSkin.label) { fixedWidth = 150 };
            _labelStyleShort = new GUIStyle(Skins.ConsoleSkin.label) { fixedWidth = 10 };
            _labelMissionTableStyle = new GUIStyle(Skins.ConsoleSkin.label) { fixedWidth = 100 };
            _narrowLabel = new GUIStyle(Skins.ConsoleSkin.label) { fixedWidth = 20 };
            _normalButton = new GUIStyle(Skins.ConsoleSkin.button);
            _normalSectionButton = new GUIStyle(Skins.ConsoleSkin.button);
            _normalSectionButton.normal.textColor = new Color(120f/255f, 150f/255f, 255f/255f, 1f);
            _toggledSectionButton = new GUIStyle(Skins.ConsoleSkin.button);
            _toggledSectionButton.normal.textColor = Color.gray;
            _narrowButton = new GUIStyle(Skins.ConsoleSkin.button) { fixedWidth = 20 };
            _normalTextfield = new GUIStyle(Skins.ConsoleSkin.textField) { fixedWidth = 150 };
        }

        public void InitializeControls()
        {
            _bodyNames = Utility.GetAllCelestialBodyNames();
            _body = "Kerbin";
            _bodyIndex = _bodyNames.IndexOf(_body);
            
            _textureName = "_MainTex";
            
            _waypointBodyNames = Utility.GetAllCelestialBodyNames();
            _waypointBody = "Kerbin";
            _waypointBodyIndex = _waypointBodyNames.IndexOf(_waypointBody);
            
            _missionBodyNames = Utility.GetAllCelestialBodyNames();
            _missionBody = "Kerbin";
            _missionBodyIndex = _missionBodyNames.IndexOf(_missionBody);
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
                    "// Orbital Survey DEBUG",
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
                if (GUILayout.Button("<", _narrowButton) && _bodyIndex > 0)
                    _body = _bodyNames[--_bodyIndex];
                _body = GUILayout.TextField(_body);
                if (GUILayout.Button(">", _narrowButton) && _bodyIndex < _bodyNames.Count-1)
                    _body = _bodyNames[++_bodyIndex];
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Import texture:", _labelStyle);
                if (GUILayout.Button("\u23f6", _narrowButton))
                {
                    DebugManager.Instance.LoadTextureFromDisk(_myCustomTextureFilename);
                    _myCustomTextureName = _myCustomTextureFilename;
                }
                _myCustomTextureFilename = GUILayout.TextField(_myCustomTextureFilename);
            }
            GUILayout.EndHorizontal();

            if (_myCustomTextureName != string.Empty)
                GUILayout.Label($"Loaded tex: {_myCustomTextureName}");

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
            
            #region PAM Overrides
            
            if (GUILayout.Button(_showPamOverridesSection ? "Hide PAM Overrides Section" : "Show PAM Overrides Section", _showPamOverridesSection ? _toggledSectionButton : _normalSectionButton))
                _showPamOverridesSection = !_showPamOverridesSection;

            if (_showPamOverridesSection)
            {
                GUILayout.Label("--");
                
                if (GUILayout.Button(DebugFovEnabled ? "Disable Custom FOV" :  "Enable Custom FOV"))
                {
                    DebugFovEnabled = !DebugFovEnabled;
                }
                if (GUILayout.Button(DebugTriggerExperiment ? "Disable Trigger Experiment" :  "Enable Trigger Experiment"))
                {
                    DebugTriggerExperiment = !DebugTriggerExperiment;
                }
                if (GUILayout.Button(DebugTriggerScienceReport ? "Disable Trigger Science Report" :  "Enable Trigger Science Report"))
                {
                    DebugTriggerScienceReport = !DebugTriggerScienceReport;
                }
                
                GUILayout.Label("--");
            }
            
            #endregion
            
            //////////////////////////////////////////////////////////////////////////////////////////
            
            #region Analytics Scanning
            
            if (GUILayout.Button(_showAnalyticsScanningSection ? "Hide Analytics Scanning Section" : "Show Analytics Scanning Section", _showAnalyticsScanningSection ? _toggledSectionButton : _normalSectionButton))
                _showAnalyticsScanningSection = !_showAnalyticsScanningSection;

            if (_showAnalyticsScanningSection)
            {
                GUILayout.Label("--");
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Universal Time:", _labelStyle);
                    UT = GUILayout.TextField(UT);
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("BufferAnalyticsScan"))
                {
                    BufferAnalyticsScan = true;
                }
                
                GUILayout.Label("--");
            }
            
            #endregion
            
            //////////////////////////////////////////////////////////////////////////////////////////
            
            #region Save Persistence Section
            /*
            if (GUILayout.Button(_showSavePersistenceSection ? "Hide Save Persistence Section" : "Show Save Persistence Section", _showSavePersistenceSection ? _toggledSectionButton : _normalSectionButton))
                _showSavePersistenceSection = !_showSavePersistenceSection;
            if(_showSavePersistenceSection)
            {
                GUILayout.Label("--");
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Save - Width:", _labelStyle);
                    //OrbitalSurveyPlugin.Instance.MySaveData.Width = int.Parse(GUILayout.TextField(OrbitalSurveyPlugin.Instance.MySaveData.Width.ToString()));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Save - Height:", _labelStyle);
                    //OrbitalSurveyPlugin.Instance.MySaveData.Height = int.Parse(GUILayout.TextField(OrbitalSurveyPlugin.Instance.MySaveData.Height.ToString()));
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Create texture in save file"))
                    {
                        //OrbitalSurveyPlugin.Instance.MySaveData.TestBoolArray = new bool[OrbitalSurveyPlugin.Instance.MySaveData.Width, OrbitalSurveyPlugin.Instance.MySaveData.Height];
                        //OrbitalSurveyPlugin.Instance.MySaveData.TestColorArray = new Color32[300, 300];
                    }
                }
                GUILayout.EndHorizontal();

                // GUILayout.BeginHorizontal();
                // {
                //     if (GUILayout.Button("Color test array"))
                //     {
                //         for (int i = 0; i < 100; i++)
                //             for (int j = 0; j < 300; j++)
                //                 OrbitalSurveyPlugin.Instance.MySaveData.TestColorArray[i, j] = Color.red;
                //
                //         for (int i = 101; i < 200; i++)
                //             for (int j = 0; j < 300; j++)
                //                 OrbitalSurveyPlugin.Instance.MySaveData.TestColorArray[i, j] = Color.yellow;
                //
                //         for (int i = 201; i < 300; i++)
                //             for (int j = 0; j < 300; j++)
                //                 OrbitalSurveyPlugin.Instance.MySaveData.TestColorArray[i, j] = Color.magenta;
                //     }
                // }
                // GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Unregister for saving"))
                    {
                        SpaceWarp.API.SaveGameManager.ModSaves.UnRegisterSaveLoadGameData("falki.orbital_survey");
                    }
                }
                GUILayout.EndHorizontal();

                // GUILayout.BeginHorizontal();
                // {
                //     if (GUILayout.Button("Reregister"))
                //     {
                //         SpaceWarp.API.SaveGameManager.ModSaves.ReregisterSaveLoadGameData(
                //             "falki.orbital_survey",
                //             (savedData) =>
                //             {
                //                 // This function will be called when a SAVE event is triggered.
                //                 // If you don't need to do anything on save events, pass null instead of this function.
                //
                //                 bool b = savedData.TestBool;
                //                 int i = savedData.TestInt;
                //                 string s = savedData.TestString;
                //
                //             },
                //             (loadedData) =>
                //             {
                //                 // This function will be called when a LOAD event is triggered and BEFORE data is loaded to your saveData object.
                //                 // If you don't need to do anything on load events, pass null instead of this function.
                //
                //                 bool b = loadedData.TestBool;
                //                 int i = loadedData.TestInt;
                //                 string s = loadedData.TestString;
                //             },
                //             //OrbitalSurveyPlugin.Instance.MySaveData
                //         );
                //     }
                // }
                // GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Export Current Overlay Texture"))
                    {
                        DEBUG_Manager.Instance.ExportCurrentOverlayTexture(_body, _textureName);
                    }
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Label("--");
            }
            */
            #endregion
            
            //////////////////////////////////////////////////////////////////////////////////////////
            
            #region Overlay Section
            
            if (GUILayout.Button(_showOverlaySection ? "Hide Overlay Section" : "Show Overlay Section", _showOverlaySection ? _toggledSectionButton : _normalSectionButton))
                _showOverlaySection = !_showOverlaySection;

            if (_showOverlaySection)
            {
                GUILayout.Label("--");
                
                if (GUILayout.Button("LoadMyCustomAssetTexture"))
                {
                    DebugManager.Instance.LoadMyCustomAssetTexture();
                    _myCustomTextureName = "(custom asset)";
                }

                /*
                if (GUILayout.Button("LoadTextureFromDisk"))
                {
                    DebugManager.Instance.LoadTextureFromDisk(_myCustomTextureFilename);
                    _myCustomTextureName = _myCustomTextureFilename;
                }
                */

                if (GUILayout.Button("ApplyMyCustomTextureToOverlay"))
                {
                    DebugManager.Instance.ApplyMyCustomTextureToOverlay(_textureName, _body);
                }

                if (GUILayout.Button("RemoveCustomOverlay"))
                {
                    DebugManager.Instance.RemoveCustomOverlay(_body);
                }

                if (GUILayout.Button("AddCustPaintedTexOverlay"))
                {
                    DebugManager.Instance.AddCustPaintedTexOverlay(_colorName, _body);
                }
                
                if (GUILayout.Button("BlackOceanSphereMaterial"))
                {
                    DebugManager.Instance.BlackOceanSphereMaterial(_myCustomTextureFilename, _body, _textureName);
                }
                
                if (GUILayout.Button("RevertOceanSphereMaterial"))
                {
                    DebugManager.Instance.RevertOceanSphereMaterial(_body, _textureName);
                }
                
                if (GUILayout.Button("AddCurrentMapOverlay"))
                {
                    DebugManager.Instance.AddCurrentMapOverlay(_mapBody, _mapTypeIndex == 0 ? MapType.Visual : MapType.Biome, _textureName);
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
            
            #endregion
            
            //////////////////////////////////////////////////////////////////////////////////////////
            
            #region Build Biome Section
            
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
                        
                        DebugManager.Instance.BuildBiomeMask(_body, biome0, biome1, biome2, biome3);
                        _biomeTexture = DebugManager.Instance.BiomeMask;
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
            
            #endregion
            
            //////////////////////////////////////////////////////////////////////////////////////////
            
            #region Paint Texture Section
            
            if (GUILayout.Button(_showPaintTextureSection ? "Hide Paint Texture Section" : "Show Paint Texture Section", _showPaintTextureSection ? _toggledSectionButton : _normalSectionButton))
                _showPaintTextureSection = !_showPaintTextureSection;

            if (_showPaintTextureSection)
            {
                GUILayout.Label("--");
                
                if (GUILayout.Button("PaintTextureAtCurrentPosition"))
                {
                    DebugManager.Instance.PaintTextureAtCurrentPosition();
                }
                
                GUILayout.Label("--");
            }
            
            #endregion
            
            //////////////////////////////////////////////////////////////////////////////////////////
            
            #region Display Maps Section
            
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
                
                GUILayout.Label($"Lat: {DebugManager.Instance.CurrentLatitude?.ToString()}");
                GUILayout.Label($"Lon: {DebugManager.Instance.CurrentLongitude?.ToString()}");

                if (GUILayout.Button("Clear map"))
                {
                    DebugManager.Instance.ClearMap(_mapBody, _mapTypeIndex == 0 ? MapType.Visual : MapType.Biome);
                }
                
                GUILayout.Label("--");
            }
            
            #endregion
            
            #region Map3d section
            
            if (GUILayout.Button(_showMap3dSection ? "Hide Map3d Section" : "Show Map3d Section", _showMap3dSection ? _toggledSectionButton : _normalSectionButton))
                _showMap3dSection = !_showMap3dSection;

            if (_showMap3dSection)
            {
                GUILayout.Label("--");
                
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("ReplaceMap3dTexture"))
                    {
                        DebugManager.Instance.ReplaceMap3dTexture(_body, _textureName);
                    }
                    if (GUILayout.Button("Undo"))
                    {
                        DebugManager.Instance.UndoReplaceMap3dTexture(_body, _textureName);
                    }
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Label("--");
            }
            
            #endregion
            
            #region Vessel Stats
            
            if (GUILayout.Button(_showResourceConsumption ? "Hide Resource Section" : "Show Resource Section", _showResourceConsumption ? _toggledSectionButton : _normalSectionButton))
                _showResourceConsumption = !_showResourceConsumption;

            if (_showResourceConsumption)
            {
                GUILayout.Label("--");
                
                /*
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label($"LifeSupport: {_isLifeSupportRegistered}");
                    if (GUILayout.Button("Register"))
                    {
                        SpaceWarp.API.Parts.PartComponentModuleOverride
                            .RegisterModuleForBackgroundResourceProcessing<KerbalLifeSupportSystem.Modules.
                                PartComponentModule_LifeSupportConsumer>();
                        _isLifeSupportRegistered = true;
                    }

                    if (GUILayout.Button("Unregister"))
                    {
                        SpaceWarp.API.Parts.PartComponentModuleOverride
                            .UnRegisterModuleForBackgroundResourceProcessing<KerbalLifeSupportSystem.Modules.
                                PartComponentModule_LifeSupportConsumer>();
                        _isLifeSupportRegistered = false;
                    }
                }
                GUILayout.EndHorizontal();
                */
                GUILayout.BeginHorizontal();
                { 
                    GUILayout.Label($"OrbitalSurv.: {_isOrbitalSurveyRegistered}");
                    if (GUILayout.Button("Register"))
                    {
                        SpaceWarp.API.Parts.PartComponentModuleOverride
                            .RegisterModuleForBackgroundResourceProcessing<OrbitalSurvey.Modules.PartComponentModule_OrbitalSurvey>();
                        _isOrbitalSurveyRegistered = true;
                    }
                    if (GUILayout.Button("Unregister"))
                    {
                        /*
                        SpaceWarp.API.Parts.PartComponentModuleOverride
                            .UnRegisterModuleForBackgroundResourceProcessing<OrbitalSurvey.Modules.PartComponentModule_OrbitalSurvey>();
                            */
                        _isOrbitalSurveyRegistered = false;
                    }
                }
                GUILayout.EndHorizontal();
                
                
                var vessels = DebugManager.Instance.GetVesselElectricityPercentages();

                foreach (var vessel in vessels)
                {
                    GUILayout.Label($"{vessel.Item1}: {vessel.Item2:N2}");
                }
                
                GUILayout.Label("--");
            }
            
            #endregion
            
            #region ScienceRegions
            
            if (GUILayout.Button(_showScienceRegionsSection ? "Hide ScienceRegions Section" : "Show ScienceRegions Section", _showScienceRegionsSection ? _toggledSectionButton : _normalSectionButton))
                _showScienceRegionsSection = !_showScienceRegionsSection;

            if (_showScienceRegionsSection)
            {
                GUILayout.Label("--");
                
                if (GUILayout.Button("Reset PQSScienceOverlay"))
                {
                    DebugManager.Instance.ResetPqsScienceOverlay();
                }
                
                if (GUILayout.Button("Set Body"))
                {
                    DebugManager.Instance.GetScienceRegionsMap(_body);
                }
                
                if (GUILayout.Button("DownloadTexture"))
                {
                    DebugManager.Instance.DownloadScienceRegionsTexture();
                }
                
                if (GUILayout.Button("TriggerExperiment"))
                {
                    DebugManager.Instance.TriggerExperiment(_body);
                }
                
                GUILayout.Label("--");
            }
            
            #endregion

            #region WaypointSection
            
            if (GUILayout.Button(_showWaypointSection ? "Hide Waypoint Section" : "Show Waypoint Section", _showWaypointSection ? _toggledSectionButton : _normalSectionButton))
                _showWaypointSection = !_showWaypointSection;

            if (_showWaypointSection)
            {
                GUILayout.Label("--");
                
                // Create waypoint
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Body:", _labelStyle);
                    if (GUILayout.Button("<", _narrowButton) && _waypointBodyIndex > 0)
                        _waypointBody = _waypointBodyNames[--_waypointBodyIndex];
                    _waypointBody = GUILayout.TextField(_waypointBody);
                    if (GUILayout.Button(">", _narrowButton) && _waypointBodyIndex < _waypointBodyNames.Count-1)
                        _waypointBody = _waypointBodyNames[++_waypointBodyIndex];
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Name:", _labelStyle);
                    _waypointName = GUILayout.TextField(_waypointName);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Latitude:", _labelStyle);
                    _waypointLatitude = GUILayout.TextField(_waypointLatitude);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Longitude:", _labelStyle);
                    _waypointLongitude = GUILayout.TextField(_waypointLongitude);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("AltFromRadius:", _labelStyle);
                    _waypointAltitudeFromRadius = GUILayout.TextField(_waypointAltitudeFromRadius);
                }
                GUILayout.EndHorizontal();
                
                if (GUILayout.Button("CreateWaypoint"))
                {
                    var lat = double.Parse(_waypointLatitude);
                    var lon = double.Parse(_waypointLongitude);
                    var alt = double.Parse(_waypointAltitudeFromRadius);
                    
                    DebugManager.Instance.CreateWaypoint(
                        name: string.IsNullOrEmpty(_waypointName) ? null : _waypointName,
                        bodyName: string.IsNullOrEmpty(_waypointBody) ? null : _waypointBody,
                        latitude: lat,
                        longitude: lon,
                        altitudeFromRadius: alt == 0 ? null : alt
                        );

                    _waypointIndex = DebugManager.Instance.Waypoints.Count - 1;
                    _waypointNameExisting = DebugManager.Instance.Waypoints[_waypointIndex].Name;
                }
                
                // Modify Waypoint
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Waypoint:", _labelStyle);
                    if (GUILayout.Button("<", _narrowButton) && DebugManager.Instance.Waypoints.Count > 0 &&_waypointIndex > 0)
                        _waypointNameExisting = DebugManager.Instance.Waypoints[--_waypointIndex].Name;
                    GUILayout.Label(_waypointNameExisting, _labelStyle);
                    if (GUILayout.Button(">", _narrowButton) && _waypointIndex < DebugManager.Instance.Waypoints.Count-1)
                        _waypointNameExisting = DebugManager.Instance.Waypoints[++_waypointIndex].Name;
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("MoveWaypoint"))
                {
                    var lat = double.Parse(_waypointLatitude);
                    var lon = double.Parse(_waypointLongitude);
                    var alt = double.Parse(_waypointAltitudeFromRadius);
                    
                    DebugManager.Instance.MoveWaypoint(_waypointIndex, lat, lon, alt == 0 ? null: alt);
                }
                
                if (GUILayout.Button("DeleteWaypoint"))
                {
                    DebugManager.Instance.DeleteWaypoint(_waypointIndex--);
                }

                if (DebugManager.Instance.Waypoints.Count > _waypointIndex)
                {
                    var waypoint = DebugManager.Instance.Waypoints[_waypointIndex];

                    if (waypoint.State == WaypointState.Hidden)
                    {
                        if (GUILayout.Button("ShowWaypoint"))
                        {
                            DebugManager.Instance.ShowHideWaypoint(_waypointIndex, true);
                        }    
                    }
                    else if (waypoint.State == WaypointState.Visible)
                    {
                        if (GUILayout.Button("HideWaypoint"))
                        {
                            DebugManager.Instance.ShowHideWaypoint(_waypointIndex, false);
                        }    
                    }
                }
                
                GUILayout.Label("--");
            }
            
            #endregion
            
            #region MissionSection
            
            if (GUILayout.Button(_showMissionSection ? "Hide Mission Section" : "Show Mission Section", _showMissionSection ? _toggledSectionButton : _normalSectionButton))
                _showMissionSection = !_showMissionSection;

            if (_showMissionSection)
            {
                GUILayout.Label("--");
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Body:", _labelStyle);
                    if (GUILayout.Button("<", _narrowButton) && _missionBodyIndex > 0)
                        _missionBody = _missionBodyNames[--_missionBodyIndex];
                    _missionBody = GUILayout.TextField(_missionBody);
                    if (GUILayout.Button(">", _narrowButton) && _missionBodyIndex < _missionBodyNames.Count-1)
                        _missionBody = _missionBodyNames[++_missionBodyIndex];
                }
                GUILayout.EndHorizontal();
                
                if (GUILayout.Button("Get Discoverable Regions"))
                {
                    DebugManager.Instance.GetDiscoverableRegions(_missionBody);
                }
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("MissionId:", _labelStyle);
                    _missionId = GUILayout.TextField(_missionId);
                    if (GUILayout.Button("Activate Mission"))
                    {
                        DebugManager.Instance.ActivateMission(_missionId);
                    }
                }
                GUILayout.EndHorizontal();
                
                if (GUILayout.Button("Create Mission Granter"))
                {
                    DebugManager.Instance.CreateMissionGranter();
                }
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Asset:", _labelStyle);
                    _assetName = GUILayout.TextField(_assetName);
                    
                    if (GUILayout.Button("Load Asset"))
                    {
                        DebugManager.Instance.LoadAsset(_assetName);
                    }
                }
                GUILayout.EndHorizontal();
                
                if (Asset != null)
                {
                    GUILayout.Label(Asset, GUILayout.Width(Asset.width), GUILayout.Height(Asset.height));    
                }
                
                if (GUILayout.Button(_showActiveMissions ? "Hide Active Missions" : "Show Active Missions", _normalButton))
                    _showActiveMissions = !_showActiveMissions;

                if (_showActiveMissions)
                {
                    var missions = DebugManager.Instance.GetAllActiveMissions();

                    if (missions != null)
                    {
                        foreach (var mission in missions)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label($"{mission.name}", _labelStyle);
                                GUILayout.Label($"{mission.currentStageIndex}", _labelStyle);
                    
                                if (GUILayout.Button("CompleteStage"))
                                {
                                    DebugManager.Instance.CompleteCurrentMissionStage(mission.ID);
                                }
                            
                                if (GUILayout.Button("CompleteMission"))
                                {
                                    DebugManager.Instance.CompleteMission(mission.ID);
                                }
                            
                            }
                            GUILayout.EndHorizontal();
                        }    
                    }
                    
                }
                
                if (GUILayout.Button(_showMissionDetails ? "Hide Mission Details" : "Show Mission Details", _normalButton))
                    _showMissionDetails = !_showMissionDetails;

                if (_showMissionDetails)
                {
                    var missions = GameManager.Instance.Game?.KSP2MissionManager?.ActiveMissions[0]?.MissionDatas;
                    MissionData mission = null;
                    if (missions != null && missions.Count > _missionIndex)
                    {
                        mission = missions[_missionIndex];
                    }
                    
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("MissionId:", _labelStyle);
                        if (GUILayout.Button("<", _narrowButton) && _missionIndex > 0)
                            _missionIndex--;
                        
                        GUILayout.Label(_missionIndex.ToString(), _narrowLabel);

                        if (GUILayout.Button(">", _narrowButton) && _missionIndex < missions.Count - 1)
                            _missionIndex++;
                        
                        if (mission != null)
                        {
                            GUILayout.Label($"Mission.ID: {mission.ID}");
                        }
                    }
                    GUILayout.EndHorizontal();
                    
                    if (mission == null)
                    {
                        GUILayout.Label("Mission is null)", _labelStyle);
                    }
                    else
                    {
                        //GUILayout.Label($"Mission.ID: {mission.ID}");
                        GUILayout.Label($"Mission.name: {mission.name}");
                        GUILayout.Label($"Stages count: {mission.missionStages.Count}");
                        GUILayout.Label($"Current stage: {mission.currentStageIndex}");
                        
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Activate stage:", _labelStyle);
                            _stageToActivate = GUILayout.TextField(_stageToActivate, _normalTextfield);
                            if (GUILayout.Button("Activate", _normalButton))
                            {
                                mission.DeactivateStage(mission.currentStageIndex);
                                mission.ActivateStage(int.Parse(_stageToActivate));
                                DebugManager.Instance.CompleteCurrentMissionStage(mission.ID);
                            }
                        }
                        GUILayout.EndHorizontal();
                        
                        if (GUILayout.Button("CompleteStage"))
                        {
                            DebugManager.Instance.CompleteCurrentMissionStage(mission.ID);
                        }

                        if (mission.missionStages != null && mission.missionStages.Count > 0)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Index", _labelMissionTableStyle);
                                GUILayout.Label("StageID", _labelMissionTableStyle);
                                GUILayout.Label("active", _labelMissionTableStyle);
                                GUILayout.Label("completed", _labelMissionTableStyle);
                            }
                            GUILayout.EndHorizontal();
                            
                            int i = 0;
                            foreach (var stage in mission.missionStages)
                            {
                                
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label(i.ToString(), _labelMissionTableStyle);
                                    GUILayout.Label($"{stage.StageID}", _labelMissionTableStyle);
                                    GUILayout.Label($"{stage.active}", _labelMissionTableStyle);
                                    GUILayout.Label($"{stage.completed}", _labelMissionTableStyle);
                                    if (GUILayout.Button("Activate"))
                                    {
                                        //mission.DeactivateStage(mission.currentStageIndex);
                                        mission.ActivateStage(i); // we need to do this because the mission dialog cannot be dismissed unless we do it
                                        //mission.missionStages[i].Activate();
                                        //DebugManager.Instance.CompleteCurrentMissionStage(mission.ID);
                                        DebugManager.Instance.CompleteSpecificMissionStage(mission.ID, i); // this marks the stage as complete
                                        mission.missionStages[i].Activate(); // this will trigger the action dialog
                                    }
                                }
                                GUILayout.EndHorizontal();
                                i++;
                            }
                        }
                    }
                }
                
                GUILayout.Label("--");
            }
            
            #endregion
            
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
}
