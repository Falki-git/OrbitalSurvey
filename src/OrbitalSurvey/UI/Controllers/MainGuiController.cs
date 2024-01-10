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
    public VisualElement Root;
    public DropdownField BodyDropdown;
    public DropdownField MapTypeDropdown;
    public Label PercentComplete;
    public Button CloseButton;
    public VisualElement MapContainer;
    public Label NotificationLabel;
    public VisualElement UpperSidebar;
    public VisualElement LowerSidebar;
    public SideToggleControl OverlayToggle;
    public SideToggleControl VesselToggle;
    public SideToggleControl GeoCoordinatesToggle;
    public VisualElement LegendContainer;

    public VesselController VesselController;
    
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
        Root = MainGui.rootVisualElement;
        
        // header controls
        BodyDropdown = Root.Q<DropdownField>("body__dropdown");
        MapTypeDropdown = Root.Q<DropdownField>("map-type__dropdown");
        PercentComplete = Root.Q<Label>("percent-complete");
        CloseButton = Root.Q<Button>("close-button");
        CloseButton.RegisterCallback<ClickEvent>(OnCloseButton);
        
        // body control
        MapContainer = Root.Q<VisualElement>("map__container");
        NotificationLabel = Root.Q<Label>("notification");
        
        // side-bar controls
        UpperSidebar = Root.Q<VisualElement>("side-bar__upper");
        LowerSidebar = Root.Q<VisualElement>("side-bar__lower");
        OverlayToggle = new SideToggleControl() { TextValue = "OVL" };
        OverlayToggle.RegisterCallback<ClickEvent>(OnOverlayToggleClicked);
        VesselToggle = new SideToggleControl() { TextValue = "VES" };
        VesselToggle.SetEnabled(true);
        VesselToggle.SwitchToggleState(SceneController.Instance.IsVesselNamesVisible, false);
        VesselToggle.RegisterCallback<ClickEvent>(OnVesselToggleClicked);
        GeoCoordinatesToggle = new SideToggleControl() { TextValue = "GEO" };
        GeoCoordinatesToggle.SetEnabled(true);
        GeoCoordinatesToggle.SwitchToggleState(SceneController.Instance.IsGeoCoordinatesVisible, false);
        GeoCoordinatesToggle.RegisterCallback<ClickEvent>(OnGeoCoordinatesToggleClicked);
        UpperSidebar.Add(OverlayToggle);
        LowerSidebar.Add(VesselToggle);
        LowerSidebar.Add(GeoCoordinatesToggle);
        
        // footer controls
        LegendContainer = Root.Q<VisualElement>("legend__container");
        
        BuildBodyDropdown();
        Core.Instance.OnMapHasDataValueChanged += PopulateBodyChoices;
        BuildMapTypeDropdown();

        var staticBackground = AssetManager.GetAsset<Texture2D>(
            $"{AssetUtility.OtherAssetsAddresses["StaticBackground"]}");
        MapContainer.style.backgroundImage = staticBackground;
        
        // create vessel controller (for markers and additional info)
        VesselController = gameObject.AddComponent<VesselController>();
        
        // check if a map was previously selected and restore it (window was previously closed and now opened again)
        if (!string.IsNullOrEmpty(SceneController.Instance.SelectedBody))
        {
            BodyDropdown.value = SceneController.Instance.SelectedBody;
        }
        
        if (SceneController.Instance.SelectedMapType != null)
        {
            MapTypeDropdown.value = GetLocalizedValueForMapType(SceneController.Instance.SelectedMapType.Value);
            OnSelectionChanged(null, false);
        }
        
        // if overlay is already active, set the overlay toggle as toggled
        if (OverlayManager.Instance.OverlayActive)
        {
            OverlayToggle.SwitchToggleState(true, false);
        }
        
        // check if previous window position exists and restore it (window was previously moved then closed)
        if (SceneController.Instance.WindowPosition != null)
            Root[0].transform.position = SceneController.Instance.WindowPosition.Value;
        else
            Root[0].transform.position = new Vector3(100, 200);
        
        // save the window position (only for current session) when it moves
        Root[0].RegisterCallback<PointerUpEvent>(OnPositionChanged);
    }

    private void OnOverlayToggleClicked(ClickEvent evt)
    {
        // when this method is called, the toggle is already set to the new value, so we'll use it
        ToggleOverlay(OverlayToggle.IsToggled);
        
        var notificationText = OverlayToggle.IsToggled ?
            LocalizationStrings.NOTIFICATIONS[Notification.OverlayOn] :
            LocalizationStrings.NOTIFICATIONS[Notification.OverlayOff];
        ShowNotification(notificationText);
    }
    
    private void OnVesselToggleClicked(ClickEvent evt)
    {
        VesselController.ToggleVesselNames();
    }

    private void OnGeoCoordinatesToggleClicked(ClickEvent evt)
    {
        VesselController.ToggleGeoCoordinates();
    }

    private string GetLocalizedValueForMapType(MapType mapType)
    {
        var localizationString =
            LocalizationStrings.MODE_TYPE_TO_MAP_TYPE.First(kvp => kvp.Value == mapType).Key;

        return _mapTypeLocalizationStrings.Find(listItem => listItem.Item1 == localizationString).Item2;
    }

    private void BuildBodyDropdown()
    {
        BodyDropdown.value = _BODY_INITIAL_VALUE;
        PopulateBodyChoices(Core.Instance.GetBodiesContainingData());
        BodyDropdown.RegisterValueChangedCallback(evt => OnSelectionChanged(evt));
    }
    
    private void BuildMapTypeDropdown()
    {
        MapTypeDropdown.value = _MAPTYPE_INITIAL_VALUE;
        MapTypeDropdown.choices = GetMapTypeChoices();
        MapTypeDropdown.RegisterValueChangedCallback(evt => OnSelectionChanged(evt));
    }
    
    private void PopulateBodyChoices(IEnumerable<string> bodiesWithData)
    {
        BodyDropdown.choices = bodiesWithData.ToList();
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
        
        if (MapTypeDropdown.value == _MAPTYPE_INITIAL_VALUE)
            return;

        string mapTypeLocalizationString =
            _mapTypeLocalizationStrings.Find(listItem => listItem.Item2 == MapTypeDropdown.value).Item1;
        
        var mapType = LocalizationStrings.MODE_TYPE_TO_MAP_TYPE[mapTypeLocalizationString];
        SceneController.Instance.SelectedMapType = mapType;

        // Enabled the Overlay toggle if it's disabled and if we're in Flight/Map view
        if (!OverlayToggle.IsEnabled)
        {
            OverlayToggle.SetEnabled(_isOverlayEligible);
        }
        
        // if Planetary Overlay is already toggled, change the map type
        if (OverlayToggle.IsToggled)
        {
            OverlayManager.Instance.DrawOverlay(mapType);
        }
        
        if (BodyDropdown.value == _BODY_INITIAL_VALUE)
            return;
        
        var body = BodyDropdown.value;
        
        BuildLegend(body);
        
        if (_selectedMap != null) 
            _selectedMap.OnDiscoveredPixelCountChanged -= UpdatePercentageComplete;

        _selectedMap = Core.Instance.CelestialDataDictionary[body].Maps[mapType];
        
        if (_selectedMap.PercentDiscovered == 0f)
        {
            MapContainer.Clear();
            MapContainer.Add(new Label("Awaiting scanning data..."));
            MapContainer.style.backgroundImage = null;
            _selectedMap.OnDiscoveredPixelCountChanged += SetMap;
        }
        else
        {
            SetMap(0);    
        }

        _selectedMap.OnDiscoveredPixelCountChanged += UpdatePercentageComplete;
        UpdatePercentageComplete(_selectedMap.PercentDiscovered);

        VesselController.RebuildVesselMarkers(body);

        SceneController.Instance.SelectedBody = body;
    }

    private void UpdatePercentageComplete(float percent)
    {
        // Is map fully scanned
        if (percent == 1f)
        {
            // Map is fully scanned
            PercentComplete.text = LocalizationStrings.COMPLETE;

            // Show Region legend if MapType is Biome
            if (SceneController.Instance.SelectedMapType == MapType.Biome &&
                Settings.ShowRegionLegend.Value)
            {
                LegendContainer.visible = true;
            }
        }
        else
        {
            PercentComplete.text = $"{Math.Floor(percent * 100).ToString()} %";
        }
    }
    
    private void SetMap(float _)
    {
        MapContainer.Clear();
        MapContainer.style.backgroundImage = _selectedMap.CurrentMap;
        _selectedMap.OnDiscoveredPixelCountChanged -= SetMap;
    }
    
    /// <summary>
    /// Builds the Regions legend container
    /// </summary>
    private void BuildLegend(string body)
    {
        LegendContainer.Clear();
        LegendContainer.visible = false;

        if (string.IsNullOrEmpty(body))
            return;
        
        var legendRegions = RegionsManager.Instance.Data[body].Values.ToList();

        foreach (var region in legendRegions)
        {
            LegendContainer.Add(new LegendKeyControl(region.Color, region.RegionId.AddSpaceBeforeUppercase()));
        }
    }
    
    private void ToggleOverlay(bool newState)
    {
        if (MapTypeDropdown.value == _MAPTYPE_INITIAL_VALUE)
            return;
        
        bool isSuccessful;
        if (newState)
        {
            string mapTypeLocalizationString =
                _mapTypeLocalizationStrings.Find(listItem => listItem.Item2 == MapTypeDropdown.value).Item1;
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
        SceneController.Instance.WindowPosition = Root[0].transform.position;
    }
    
    private void OnCloseButton(ClickEvent evt)
    {
        SceneController.Instance.ToggleUI(false);
    }

    public void ShowNotification(string message)
    {
        NotificationLabel.text = message;
        NotificationLabel.AddToClassList("notification--show");
        
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
        NotificationLabel.RemoveFromClassList("notification--show");
    }
}