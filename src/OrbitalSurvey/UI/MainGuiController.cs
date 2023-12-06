using KSP.Game;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;
using SpaceWarp.API.Assets;
using UnityEngine;
using UnityEngine.UIElements;

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
    public Toggle PlanetaryOverlay;
    
    private const string _BODY_INITIAL_VALUE = "<body>";
    private const string _MAPTYPE_INITIAL_VALUE = "<map>";

    private MapData _selectedMap;

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
        
        // footer controls
        PlanetaryOverlay = Root.Q<Toggle>("planetary-overlay");
        PlanetaryOverlay.RegisterValueChangedCallback(ToggleOverlay);
        
        BuildBodyDropdown();
        BuildMapTypeDropdown();

        var staticBackground = AssetManager.GetAsset<Texture2D>(
            $"{AssetUtility.OtherAssetsAddresses["StaticBackground"]}");
        MapContainer.style.backgroundImage = staticBackground;
        
        // disable Planetary Overlay toggle since it's first needed for the body and maptype to be selected
        PlanetaryOverlay.SetEnabled(false);
        
        // check if a map was previously selected and restore it (window was previously closed and now opened again)
        if (!String.IsNullOrEmpty(SceneController.Instance.SelectedBody) &&
            SceneController.Instance.SelectedMapType != null)
        {
            BodyDropdown.value = SceneController.Instance.SelectedBody;
            MapTypeDropdown.value = SceneController.Instance.SelectedMapType.ToString();
            OnSelectionChanged(null);
        }
        
        // check if previous window position exists and restore it (window was previously moved then closed)
        if (SceneController.Instance.WindowPosition != null)
            Root[0].transform.position = SceneController.Instance.WindowPosition.Value;
        else
            Root[0].transform.position = new Vector3(100, 200);
        
        // save the window position (only for current session) when it moves
        Root[0].RegisterCallback<PointerUpEvent>(OnPositionChanged);
    }

    private void BuildBodyDropdown()
    {
        BodyDropdown.value = _BODY_INITIAL_VALUE;
        BodyDropdown.choices = Core.Instance.CelestialDataDictionary.Keys.ToList();
        BodyDropdown.RegisterValueChangedCallback(OnSelectionChanged);
    }

    private void BuildMapTypeDropdown()
    {
        MapTypeDropdown.value = _MAPTYPE_INITIAL_VALUE;
        MapTypeDropdown.choices = Enum.GetNames(typeof(MapType)).ToList();
        MapTypeDropdown.RegisterValueChangedCallback(OnSelectionChanged);
    }
    
    private void OnSelectionChanged(ChangeEvent<string> _)
    {
        if (MapTypeDropdown.value == _MAPTYPE_INITIAL_VALUE)
            return;
        
        var mapType = Enum.Parse<MapType>(MapTypeDropdown.value);

        // Planetary Overlay
        PlanetaryOverlay.SetEnabled(_isOverlayEligible);
        // if Planetary Overlay is already toggled, change the map type
        if (PlanetaryOverlay.value)
            OverlayManager.Instance.DrawOverlay(mapType);
        
        if (BodyDropdown.value == _BODY_INITIAL_VALUE)
            return;
        
        var body = BodyDropdown.value;
        
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

        SceneController.Instance.SelectedBody = body;
        SceneController.Instance.SelectedMapType = mapType;
    }

    private void UpdatePercentageComplete(float percent)
    {
        if (percent == 1f)
            PercentComplete.text = LocalizationStrings.COMPLETE;
        else 
            PercentComplete.text = percent.ToString("P0");
    }
    
    private void SetMap(float _)
    {
        MapContainer.Clear();
        MapContainer.style.backgroundImage = _selectedMap.CurrentMap;
        _selectedMap.OnDiscoveredPixelCountChanged -= SetMap;
    }
    
    private void ToggleOverlay(ChangeEvent<bool> evt)
    {
        if (MapTypeDropdown.value == _MAPTYPE_INITIAL_VALUE)
            return;
        
        bool isSuccessful;
        if (evt.newValue)
        {
            isSuccessful = OverlayManager.Instance.DrawOverlay(Enum.Parse<MapType>(MapTypeDropdown.value));
        }
        else
        {
            isSuccessful = OverlayManager.Instance.RemoveOverlay();
        }

        if (!isSuccessful)
        {
            PlanetaryOverlay.SetValueWithoutNotify(evt.previousValue);
        }
    }
    
    private void OnPositionChanged(PointerUpEvent evt)
    {
        SceneController.Instance.WindowPosition = Root[0].transform.position;
    }
    
    private void OnCloseButton(ClickEvent evt)
    {
        SceneController.Instance.ShowMainGui = false;
        // GameObject.Find(OrbitalSurveyPlugin.ToolbarFlightButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(false);
        // GameObject.Find(OrbitalSurveyPlugin.ToolbarOABButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(false);
        // GameObject.Find(OrbitalSurveyPlugin.ToolbarKSCButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(false);
    }
}