using System.Collections;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;
using OrbitalSurvey.UI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace OrbitalSurvey.UI;

public class VesselController : MonoBehaviour
{
    public VesselController()
    { }
    
    public UIDocument MainGui;
    private MainGuiController _mainGuiController;
    private VisualElement _root;
    private VisualElement _canvas;

    private float _canvasWidth;
    private float _canvasHeight;
    private bool _canvasInitialized;

    private List<(VesselManager.VesselStats vessel, VesselMarkerControl control)> _trackedVessels = new();
    
    public void OnEnable()
    {
        MainGui = GetComponent<UIDocument>();
        _mainGuiController = GetComponent<MainGuiController>();
        _root = MainGui.rootVisualElement;

        _canvas = _root.Q<VisualElement>("canvas");
        StartCoroutine(GetCanvasSize());
    }

    private IEnumerator GetCanvasSize()
    {
        // wait for 1 frame for the canvas to get its size
        yield return null;
        
        _canvasWidth = _canvas.layout.width;
        _canvasHeight = _canvas.layout.height;
        _canvasInitialized = true;
    }

    public void RebuildVesselMarkers(string body) => StartCoroutine(Rebuild(body));
    private IEnumerator Rebuild(string body)
    {
        if (!_canvasInitialized)
        {
            yield return null;
        }
        
        VesselManager.Instance.ClearAllSubscriptions();
        _canvas.Clear();
        
        var vessels = VesselManager.Instance.OrbitalSurveyVessels.FindAll(v => v.Body == body);

        foreach (var vessel in vessels)
        {
            var control = new VesselMarkerControl(SceneController.Instance.IsVesselNamesVisible,SceneController.Instance.IsGeoCoordinatesVisible)
            {
                NameValue = vessel.Name,
                LatitudeValue = vessel.GeographicCoordinates.Latitude,
                LongitudeValue = vessel.GeographicCoordinates.Longitude
            };

            vessel.OnNameChanged += (value) => OnNameChanged(control, value);
            vessel.OnMapGuiPositionChanged += (value) => OnMapGuiPositionChanged(control, value);
            vessel.OnGeographicCoordinatesChanged += (value) => OnGeographicCoordinatesChanged(control, value);
            vessel.OnVisualModuleChanged += (value) => OnModuleChanged(control, value, MapType.Visual);
            vessel.OnBiomeModuleChanged += (value) => OnModuleChanged(control, value, MapType.Biome);
            
            OnMapGuiPositionChanged(control, vessel.MapLocationPercent);
            InitializeModuleStyles(control, vessel);
            _canvas.Add(control);
            _trackedVessels.Add((vessel, control));
        }

        VesselManager.Instance.OnVesselChangedBody +=
            (value) => StartCoroutine(Rebuild(SceneController.Instance.SelectedBody));
        VesselManager.Instance.OnVesselRegistered +=
            (value) => StartCoroutine(Rebuild(SceneController.Instance.SelectedBody));
    }

    private void InitializeModuleStyles(VesselMarkerControl control, VesselManager.VesselStats vessel)
    {
        var mapType = SceneController.Instance.SelectedMapType;
        if (mapType == null)
            return;
        
        var module = vessel.ModuleStats.Find(m => m.Mode == mapType);
        if (module == null)
        {
            control.SetAsInactive();
            return;
        }
        
        OnModuleChanged(control, module, mapType.Value);
    }

    private void OnNameChanged(VesselMarkerControl control, string name)
    {
        control.NameValue = name;
    }
    
    private void OnMapGuiPositionChanged(VesselMarkerControl control, (float percentX, float percentY) mapGuiPositionChanged)
    {
        control.style.left =_canvasWidth * mapGuiPositionChanged.percentX;
        control.style.top = _canvasHeight * (1 - mapGuiPositionChanged.percentY);
    }
    
    private void OnGeographicCoordinatesChanged(VesselMarkerControl control, (double latitude, double longitude) coords)
    {
        control.LatitudeValue = coords.latitude;
        control.LongitudeValue = coords.longitude;
    }
    
    private void OnModuleChanged(VesselMarkerControl control, VesselManager.ModuleStats module, MapType mode)
    {
        if(mode != SceneController.Instance.SelectedMapType)
            return;

        if (!module.Enabled)
        {
            control.SetAsInactive();
            return;
        }

        if (module.Status == Status.Complete)
        {
            control.SetAsNormal();
            return;
        }

        if (module.State == State.BelowMin || module.State == State.AboveMax ||
            module.Status == Status.NoPower || module.Status == Status.NotDeployed)
        {
            control.SetAsError();
            return;
        }

        if (module.State == State.BelowIdeal || module.State == State.AboveIdeal)
        {
            control.SetAsWarning();
            return;
        }

        if (module.State == State.Ideal)
        {
            control.SetAsGood();
            return;
        }
    }

    public void ToggleVesselNames()
    {
        SceneController.Instance.IsVesselNamesVisible = !SceneController.Instance.IsVesselNamesVisible;

        foreach (var vessel in _trackedVessels)
        {
            vessel.control.SetVesselNameVisibility(SceneController.Instance.IsVesselNamesVisible);
        }

        // send notification
        var notificationText = SceneController.Instance.IsVesselNamesVisible ?
            LocalizationStrings.NOTIFICATIONS[Notification.VesselNamesOn] :
            LocalizationStrings.NOTIFICATIONS[Notification.VesselNamesOff];
        _mainGuiController.ShowNotification(notificationText);
    }

    public void ToggleGeoCoordinates()
    {
        SceneController.Instance.IsGeoCoordinatesVisible = !SceneController.Instance.IsGeoCoordinatesVisible;

        foreach (var vessel in _trackedVessels)
        {
            vessel.control.SetGeoCoordinatesVisibility(SceneController.Instance.IsGeoCoordinatesVisible);
        }
        
        // send notification
        var notificationText = SceneController.Instance.IsGeoCoordinatesVisible ?
            LocalizationStrings.NOTIFICATIONS[Notification.GeoCoordsOn] :
            LocalizationStrings.NOTIFICATIONS[Notification.GeoCoordsOff];
        _mainGuiController.ShowNotification(notificationText);
    }

    private void OnDestroy()
    {
        VesselManager.Instance.ClearAllSubscriptions();
    }
}