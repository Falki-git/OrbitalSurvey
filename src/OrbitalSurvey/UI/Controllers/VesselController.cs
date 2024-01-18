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

    public static VesselController Instance;
    
    public delegate void ActiveVesselMapGuiPositionChanged((float percentX, float percentY) mapGuiPositionChanged);
    public event ActiveVesselMapGuiPositionChanged OnActiveVesselMapGuiPositionChanged;
    
    public UIDocument MainGui;
    private MainGuiController _mainGuiController;
    private VisualElement _root;
    private VisualElement _canvas;

    private float _canvasWidth;
    private float _canvasHeight;
    private bool _canvasInitialized;

    private List<(VesselManager.VesselStats vessel, VesselMarkerControl control)> _trackedVessels = new();
    
    private Action<float, float> _windowResizedHandler;
    private Action<float> _zoomFactorChangeHandler;
    private Action<Vector2> _panExecutedHandler;
    
    public void OnEnable()
    {
        Instance = this;
        MainGui = GetComponent<UIDocument>();
        _mainGuiController = GetComponent<MainGuiController>();
        _root = MainGui.rootVisualElement;

        _canvas = _root.Q<VisualElement>("marker-canvas");
        StartCoroutine(GetCanvasSize());
        StartCoroutine(RegisterForWindowResize());
        StartCoroutine(RegisterForZoomFactorChanged());
        StartCoroutine(RegisterForPanExecuted());
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
    private IEnumerator Rebuild() => Rebuild(SceneController.Instance.SelectedBody);
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
            vessel.OnMapGuiPositionChanged += (value) => OnMapGuiPositionChanged(control, value, vessel.IsActiveVessel);
            vessel.OnGeographicCoordinatesChanged += (value) => OnGeographicCoordinatesChanged(control, value);
            vessel.OnVisualModuleChanged += (value) => OnModuleChanged(control, value, MapType.Visual);
            vessel.OnBiomeModuleChanged += (value) => OnModuleChanged(control, value, MapType.Biome);
            
            OnMapGuiPositionChanged(control, vessel.MapLocationPercent, vessel.IsActiveVessel);
            InitializeModuleStyles(control, vessel);
            _canvas.Add(control);
            _trackedVessels.Add((vessel, control));
        }

        VesselManager.Instance.OnVesselChangedBody +=
            (value) => StartCoroutine(Rebuild());
        VesselManager.Instance.OnVesselRegistered +=
            (value) => StartCoroutine(Rebuild());
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
    
    private void OnMapGuiPositionChanged(VesselMarkerControl control, (float percentX, float percentY) mapGuiPositionChanged, bool isActiveVessel)
    {
        var scaledCoordinates = GetScaledCoordinates(mapGuiPositionChanged.percentX, 1 - mapGuiPositionChanged.percentY);
        control.style.left = scaledCoordinates.x;
        control.style.top = scaledCoordinates.y;

        if (isActiveVessel)
        {
            OnActiveVesselMapGuiPositionChanged?.Invoke(mapGuiPositionChanged);
        }
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
    
    /// <summary>
    /// Gets the new width and height of the canvas after window is resized by the player.
    /// Then repositions all vessel markers. 
    /// </summary>
    private IEnumerator RegisterForWindowResize()
    {
        if (ResizeController.Instance == null)
        {
            yield return null;
        }
        
        _windowResizedHandler = (newWidth, newHeight) =>
        {
            _canvasWidth = _canvas.layout.width;
            _canvasHeight = _canvas.layout.height;
            RepositionAllVesselControls();
        };

        ResizeController.Instance.OnWindowResized += _windowResizedHandler;
    }
    
    /// <summary>
    /// Repositions all vessel markers when zoom factor is changed
    /// </summary>
    private IEnumerator RegisterForZoomFactorChanged()
    {
        if (ZoomAndPanController.Instance == null)
        {
            yield return null;
        }
        
        _zoomFactorChangeHandler = (zoomFactor) => RepositionAllVesselControls();

        ZoomAndPanController.Instance.OnZoomFactorChanged += _zoomFactorChangeHandler;
    }
    
    /// <summary>
    /// Repositions all vessel markers when a pan is executed
    /// </summary>
    private IEnumerator RegisterForPanExecuted()
    {
        if (ZoomAndPanController.Instance == null)
        {
            yield return null;
        }

        _panExecutedHandler = (panOffset) => RepositionAllVesselControls();
        
        ZoomAndPanController.Instance.OnPanExecuted += _panExecutedHandler;
    }
    
    private Vector2 GetScaledCoordinates(float percentX, float percentY)
    {
        var scaledDistanceToCenter = ScaledDistanceToCenter(percentX, percentY);

        var scaledTextureCoordinates = new Vector2
        {
            x = scaledDistanceToCenter.x + _canvasWidth / 2,
            y = scaledDistanceToCenter.y + _canvasHeight / 2
        };

        return scaledTextureCoordinates;
    }

    private Vector2 ScaledDistanceToCenter(float percentX, float percentY)
    {
        var distanceToCenter = DistanceToCenter(percentX, percentY);
        distanceToCenter.x *= ZoomAndPanController.Instance.ZoomFactor;
        distanceToCenter.y *= ZoomAndPanController.Instance.ZoomFactor;
        return distanceToCenter;
    }

    private Vector2 DistanceToCenter(float percentX, float percentY)
    {
        var distanceToCenter = new Vector2(
            percentX * _canvasWidth - _canvasWidth / 2,
            percentY * _canvasHeight - _canvasHeight / 2);
        
        // apply panning offset
        distanceToCenter.x += ZoomAndPanController.Instance.PanOffset.x;
        distanceToCenter.y += ZoomAndPanController.Instance.PanOffset.y;

        return distanceToCenter;
    }
    
    /// <summary>
    /// Refreshes positions of all vessel markers.
    /// Called after a UI event triggered that requires repositioning (zoom, pan, resize)
    /// </summary>
    private void RepositionAllVesselControls()
    {
        foreach (var vesselTuple in _trackedVessels)
        {
            var vessel = vesselTuple.vessel;
            var control = vesselTuple.control;
            var scaledCoordinates = GetScaledCoordinates(vessel.MapLocationPercent.PercentX, 1 - vessel.MapLocationPercent.PercentY);
            control.style.left = scaledCoordinates.x;
            control.style.top = scaledCoordinates.y;
        }
    }

    private void OnDestroy()
    {
        VesselManager.Instance.ClearAllSubscriptions();

        if (ResizeController.Instance != null)
        {
            ResizeController.Instance.OnWindowResized -= _windowResizedHandler;        
        }

        if (ZoomAndPanController.Instance != null)
        {
            ZoomAndPanController.Instance.OnZoomFactorChanged -= _zoomFactorChangeHandler;
            ZoomAndPanController.Instance.OnPanExecuted -= _panExecutedHandler;
        }
    }
}