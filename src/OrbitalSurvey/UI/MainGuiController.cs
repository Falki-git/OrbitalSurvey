using KSP.UI.Binding;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;
using SpaceWarp.API.Assets;
using UnityEngine;
using UnityEngine.UIElements;

namespace OrbitalSurvey.UI;

public class MainGuiController : MonoBehaviour
{
    public UIDocument MainGui;
    public VisualElement Root;
    public DropdownField BodyDropdown;
    public DropdownField MapTypeDropdown;
    public Label PercentComplete;
    public Button CloseButton;
    public VisualElement MapContainer;
    
    private const string _BODY_INITIAL_VALUE = "<body>";
    private const string _MAPTYPE_INITIAL_VALUE = "<map>";

    private MapData _selectedMap;
    
    public MainGuiController()
    { }

    public void OnEnable()
    {
        MainGui = GetComponent<UIDocument>();
        Root = MainGui.rootVisualElement;
        BodyDropdown = Root.Q<DropdownField>("body__dropdown");
        MapTypeDropdown = Root.Q<DropdownField>("map-type__dropdown");
        PercentComplete = Root.Q<Label>("percent-complete");
        MapContainer = Root.Q<VisualElement>("map__container");
        
        CloseButton = Root.Q<Button>("close-button");
        CloseButton.RegisterCallback<ClickEvent>(OnCloseButton);
        
        BuildBodyDropdown();
        BuildMapTypeDropdown();

        var staticBackground = AssetManager.GetAsset<Texture2D>(
            $"{OrbitalSurveyPlugin.ModGuid}/orbitalsurvey_ui/ui/orbital_survey/static_background.jpeg");
        MapContainer.style.backgroundImage = staticBackground;
        
        // check if a map was previously selected and restore it
        if (!String.IsNullOrEmpty(SceneController.Instance.SelectedBody) &&
            SceneController.Instance.SelectedMapType != null)
        {
            BodyDropdown.value = SceneController.Instance.SelectedBody;
            MapTypeDropdown.value = SceneController.Instance.SelectedMapType.ToString();
            OnSelectionChanged(null);
        }
        
        // check if previous window position exists and restore it
        if (SceneController.Instance.WindowPosition != null)
            Root[0].transform.position = SceneController.Instance.WindowPosition.Value;
        
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
        if (BodyDropdown.value == _BODY_INITIAL_VALUE || MapTypeDropdown.value == _MAPTYPE_INITIAL_VALUE)
            return;
        
        if (_selectedMap != null) 
            _selectedMap.OnDiscoveredPixelCountChanged -= UpdatePercentageComplete;

        var body = BodyDropdown.value;
        var mapType = Enum.Parse<MapType>(MapTypeDropdown.value);
        _selectedMap = Core.Instance.CelestialDataDictionary[body].Maps[mapType];
        
        MapContainer.style.backgroundImage = _selectedMap.CurrentMap;
        
        _selectedMap.OnDiscoveredPixelCountChanged += UpdatePercentageComplete;
        UpdatePercentageComplete(_selectedMap.PercentDiscovered);

        SceneController.Instance.SelectedBody = body;
        SceneController.Instance.SelectedMapType = mapType;
    }

    private void UpdatePercentageComplete(float percent)
    {
        if (percent == 1f)
            PercentComplete.text = StatusStrings.COMPLETE;
        else 
            PercentComplete.text = percent.ToString("P0");
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