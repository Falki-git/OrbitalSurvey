using System.Collections;
using BepInEx.Logging;
using I2.Loc;
using KSP.Game;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;
using OrbitalSurvey.UI.Controls;
using OrbitalSurvey.Utilities;
using SpaceWarp.API.Assets;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = BepInEx.Logging.Logger;

namespace OrbitalSurvey.UI;

public class MainGuiController : MonoBehaviour
{
    public MainGuiController()
    { }
    
    public UIDocument MainGui;
    
    private VisualElement _root;
    private DropdownField _bodyDropdown;
    private DropdownField _mapTypeDropdown;
    private Label _percentComplete;
    private Button _closeButton;
    private VisualElement _mapContainer;
    private Label _notificationLabel;
    private VisualElement _upperSidebar;
    private VisualElement _lowerSidebar;
    private SideToggleControl _overlayToggle;
    private SideToggleControl _vesselToggle;
    private SideToggleControl _geoCoordinatesToggle;
    private VisualElement _legendContainer;
    private VesselController _vesselController;
    private const string _BODY_INITIAL_VALUE = "<body>";
    private const string _MAPTYPE_INITIAL_VALUE = "<map>";
    private Coroutine _hideNotification;
    private MapData _selectedMap;
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.MainGuiController");
    
    /// <summary>
    /// Item1 = localization key (e.g. "PartModules/OrbitalSurvey/Mode/Visual"), Item2 = localization value (e.g. "Visual") 
    /// </summary>
    private List<(string, string)> _mapTypeLocalizationStrings;

    private bool _isOverlayEligible
    {
        get
        {
            if (Utility.GameState?.GameState == GameState.FlightView || Utility.GameState?.GameState == GameState.Map3DView)
                return true;

            return false;
        }
    }

    public void OnEnable()
    {
        MainGui = GetComponent<UIDocument>();
        _root = MainGui.rootVisualElement;
        
        // header controls
        _bodyDropdown = _root.Q<DropdownField>("body__dropdown");
        _mapTypeDropdown = _root.Q<DropdownField>("map-type__dropdown");
        _percentComplete = _root.Q<Label>("percent-complete");
        _closeButton = _root.Q<Button>("close-button");
        _closeButton.RegisterCallback<ClickEvent>(OnCloseButton);
        
        // body control
        _mapContainer = _root.Q<VisualElement>("map__container");
        _notificationLabel = _root.Q<Label>("notification");
        
        // side-bar controls
        _upperSidebar = _root.Q<VisualElement>("side-bar__upper");
        _lowerSidebar = _root.Q<VisualElement>("side-bar__lower");
        _overlayToggle = new SideToggleControl() { TextValue = "OVL" };
        _overlayToggle.RegisterCallback<ClickEvent>(OnOverlayToggleClicked);
        _vesselToggle = new SideToggleControl() { TextValue = "VES" };
        _vesselToggle.SetEnabled(true);
        _vesselToggle.SwitchToggleState(SceneController.Instance.IsVesselNamesVisible, false);
        _vesselToggle.RegisterCallback<ClickEvent>(OnVesselToggleClicked);
        _geoCoordinatesToggle = new SideToggleControl() { TextValue = "GEO" };
        _geoCoordinatesToggle.SetEnabled(true);
        _geoCoordinatesToggle.SwitchToggleState(SceneController.Instance.IsGeoCoordinatesVisible, false);
        _geoCoordinatesToggle.RegisterCallback<ClickEvent>(OnGeoCoordinatesToggleClicked);
        _upperSidebar.Add(_overlayToggle);
        _lowerSidebar.Add(_vesselToggle);
        _lowerSidebar.Add(_geoCoordinatesToggle);
        
        // footer controls
        _legendContainer = _root.Q<VisualElement>("legend__container");
        
        BuildBodyDropdown();
        Core.Instance.OnMapHasDataValueChanged += PopulateBodyChoices;
        BuildMapTypeDropdown();

        var staticBackground = AssetManager.GetAsset<Texture2D>(
            $"{AssetUtility.OtherAssetsAddresses["StaticBackground"]}");
        _mapContainer.style.backgroundImage = staticBackground;
        
        // create vessel controller (for markers and additional info)
        _vesselController = gameObject.AddComponent<VesselController>();
        
        // check if a map was previously selected and restore it (window was previously closed and now opened again)
        if (!string.IsNullOrEmpty(SceneController.Instance.SelectedBody))
        {
            _bodyDropdown.value = SceneController.Instance.SelectedBody;
        }
        
        if (SceneController.Instance.SelectedMapType != null)
        {
            _mapTypeDropdown.value = GetLocalizedValueForMapType(SceneController.Instance.SelectedMapType.Value);
            OnSelectionChanged(null, false);
        }
        
        // if overlay is already active, set the overlay toggle as toggled
        if (OverlayManager.Instance.OverlayActive)
        {
            _overlayToggle.SwitchToggleState(true, false);
        }
        
        // check if previous window position exists and restore it (window was previously moved then closed)
        if (SceneController.Instance.WindowPosition != null)
            _root[0].transform.position = SceneController.Instance.WindowPosition.Value;
        else
            _root[0].transform.position = new Vector3(100, 200);
        
        // save the window position (only for current session) when it moves
        _root[0].RegisterCallback<PointerUpEvent>(OnPositionChanged);
    }

    private void OnOverlayToggleClicked(ClickEvent evt)
    {
        // when this method is called, the toggle is already set to the new value, so we'll use it
        ToggleOverlay(_overlayToggle.IsToggled);
        
        var notificationText = _overlayToggle.IsToggled ?
            LocalizationStrings.NOTIFICATIONS[Notification.OverlayOn] :
            LocalizationStrings.NOTIFICATIONS[Notification.OverlayOff];
        ShowNotification(notificationText);
    }
    
    private void OnVesselToggleClicked(ClickEvent evt)
    {
        _vesselController.ToggleVesselNames();
    }

    private void OnGeoCoordinatesToggleClicked(ClickEvent evt)
    {
        _vesselController.ToggleGeoCoordinates();
    }

    private string GetLocalizedValueForMapType(MapType mapType)
    {
        var localizationString =
            LocalizationStrings.MODE_TYPE_TO_MAP_TYPE.First(kvp => kvp.Value == mapType).Key;

        return _mapTypeLocalizationStrings.Find(listItem => listItem.Item1 == localizationString).Item2;
    }

    private void BuildBodyDropdown()
    {
        _bodyDropdown.value = _BODY_INITIAL_VALUE;
        PopulateBodyChoices(Core.Instance.GetBodiesContainingData());
        _bodyDropdown.RegisterValueChangedCallback(evt => OnSelectionChanged(evt));
    }
    
    private void BuildMapTypeDropdown()
    {
        _mapTypeDropdown.value = _MAPTYPE_INITIAL_VALUE;
        _mapTypeDropdown.choices = GetMapTypeChoices();
        _mapTypeDropdown.RegisterValueChangedCallback(evt => OnSelectionChanged(evt));
    }
    
    private void PopulateBodyChoices(IEnumerable<string> bodiesWithData)
    {
        _bodyDropdown.choices = bodiesWithData.ToList();
    }

    private List<string> GetMapTypeChoices()
    {
        _mapTypeLocalizationStrings = new();

        foreach (var key in LocalizationStrings.MODE_TYPE_TO_MAP_TYPE.Keys)
        {
            _mapTypeLocalizationStrings.Add((key, new LocalizedString(key)));
        }

        return _mapTypeLocalizationStrings.Select(listItem => listItem.Item2).ToList();
    }

    private void OnSelectionChanged(ChangeEvent<string> evt, bool playSound = true)
    {
        if (playSound && Settings.PlayUiSounds.Value) { KSP.Audio.KSPAudioEventManager.OnKSCBuildingClick(new Vector2()); }
        
        if (_mapTypeDropdown.value == _MAPTYPE_INITIAL_VALUE)
            return;

        string mapTypeLocalizationString =
            _mapTypeLocalizationStrings.Find(listItem => listItem.Item2 == _mapTypeDropdown.value).Item1;
        
        var mapType = LocalizationStrings.MODE_TYPE_TO_MAP_TYPE[mapTypeLocalizationString];
        SceneController.Instance.SelectedMapType = mapType;

        // Enabled the Overlay toggle if it's disabled and if we're in Flight/Map view
        if (!_overlayToggle.IsEnabled)
        {
            _overlayToggle.SetEnabled(_isOverlayEligible);
        }
        
        // if Planetary Overlay is already toggled, change the map type
        if (_overlayToggle.IsToggled)
        {
            OverlayManager.Instance.DrawOverlay(mapType);
        }
        
        if (_bodyDropdown.value == _BODY_INITIAL_VALUE)
            return;
        
        var body = _bodyDropdown.value;
        
        BuildLegend(body);
        
        if (_selectedMap != null) 
            _selectedMap.OnDiscoveredPixelCountChanged -= UpdatePercentageComplete;

        _selectedMap = Core.Instance.CelestialDataDictionary[body].Maps[mapType];
        
        if (_selectedMap.PercentDiscovered == 0f)
        {
            _mapContainer.Clear();
            _mapContainer.Add(new Label("Awaiting scanning data..."));
            _mapContainer.style.backgroundImage = null;
            _selectedMap.OnDiscoveredPixelCountChanged += SetMap;
        }
        else
        {
            SetMap(0);    
        }

        _selectedMap.OnDiscoveredPixelCountChanged += UpdatePercentageComplete;
        UpdatePercentageComplete(_selectedMap.PercentDiscovered);

        _vesselController.RebuildVesselMarkers(body);

        SceneController.Instance.SelectedBody = body;
    }

    private void UpdatePercentageComplete(float percent)
    {
        // Is map fully scanned
        if (percent == 1f)
        {
            // Map is fully scanned
            _percentComplete.text = LocalizationStrings.COMPLETE;

            // Show Region legend if MapType is Biome
            if (SceneController.Instance.SelectedMapType == MapType.Biome &&
                Settings.ShowRegionLegend.Value)
            {
                _legendContainer.visible = true;
            }
        }
        else
        {
            _percentComplete.text = $"{Math.Floor(percent * 100).ToString()} %";
        }
    }
    
    private void SetMap(float _)
    {
        _mapContainer.Clear();
        _mapContainer.style.backgroundImage = _selectedMap.CurrentMap;
        _selectedMap.OnDiscoveredPixelCountChanged -= SetMap;
    }
    
    /// <summary>
    /// Builds the Regions legend container
    /// </summary>
    private void BuildLegend(string body)
    {
        _legendContainer.Clear();
        _legendContainer.visible = false;

        if (string.IsNullOrEmpty(body))
            return;
        
        var legendRegions = RegionsManager.Instance.Data[body].Values.ToList();

        foreach (var region in legendRegions)
        {
            _legendContainer.Add(new LegendKeyControl(region.Color, region.RegionId.AddSpaceBeforeUppercase()));
        }
    }
    
    private void ToggleOverlay(bool newState)
    {
        if (_mapTypeDropdown.value == _MAPTYPE_INITIAL_VALUE)
            return;
        
        bool isSuccessful;
        if (newState)
        {
            string mapTypeLocalizationString =
                _mapTypeLocalizationStrings.Find(listItem => listItem.Item2 == _mapTypeDropdown.value).Item1;
            var mapType = LocalizationStrings.MODE_TYPE_TO_MAP_TYPE[mapTypeLocalizationString];
            isSuccessful = OverlayManager.Instance.DrawOverlay(mapType);
        }
        else
        {
            isSuccessful = OverlayManager.Instance.RemoveOverlay();
        }
    }
    
    private void OnPositionChanged(PointerUpEvent evt)
    {
        SceneController.Instance.WindowPosition = _root[0].transform.position;
    }
    
    private void OnCloseButton(ClickEvent evt)
    {
        SceneController.Instance.ToggleUI(false);
    }

    public void ShowNotification(string message)
    {
        _notificationLabel.text = message;
        _notificationLabel.AddToClassList("notification--show");
        
        // stop the previous coroutine for hiding if it's still running (case: multiple fast clicks) 
        if (_hideNotification != null)
        {
            StopCoroutine(_hideNotification);
        }

        // start a coroutine for hiding the notification
        _hideNotification = StartCoroutine(HideNotification(3f));
    }

    private IEnumerator HideNotification(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        _notificationLabel.RemoveFromClassList("notification--show");
    }
}