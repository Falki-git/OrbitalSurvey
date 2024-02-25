using System.Collections;
using BepInEx.Logging;
using OrbitalSurvey.Models;
using OrbitalSurvey.UI.Controls;
using OrbitalSurvey.Utilities;
using SpaceWarp.API.Game.Waypoints;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = BepInEx.Logging.Logger;

namespace OrbitalSurvey.UI;

public class WaypointController: MonoBehaviour
{
    public static WaypointController Instance;
    
    private MainGuiController _mainGuiController;
    private VisualElement _root;
    private VisualElement _mapContainer;
    private VisualElement _waypointCanvas;
    private VisualElement _contextCanvas;
    private VisualElement _contextMenu;
    private Label _contextTitle;
    private TextField _waypointName;
    private Button _colorYellow;
    private Button _colorRed;
    private Button _colorGreen;
    private Button _colorBlue;
    private Button _colorGray;
    private Button _addWaypoint;
    private Button _updateWaypoint;
    private Button _removeWaypoint;

    private Vector2 _waypointToAddPercentPosition;
    private WaypointColor _waypointToAddColor = WaypointColor.Yellow;
    
    private float _canvasWidth;
    private float _canvasHeight;
    private bool _waypointCanvasInitialized;
    
    private WaypointModel _selectedWaypointModel;
    
    private Action<float, float> _windowResizedHandler;
    private Action<float> _zoomFactorChangeHandler;
    private Action<Vector2> _panExecutedHandler;
    
    private const string UssColorButtonSelected = "color-selected";
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.WaypointController");

    public void Start()
    {
        Instance = this;
        _mainGuiController = GetComponent<MainGuiController>();
        _root = GetComponent<UIDocument>().rootVisualElement[0];
        
        _mapContainer = _root.Q<VisualElement>("map");

        // stop propagation on PointerDownEvent is needed for the subsequent PointerUpEvent to trigger.
        // evt.button == 1 is needed to allow dragging of the window for left clicks
        _mapContainer.RegisterCallback<PointerDownEvent>(evt => { if (evt.button == 1) evt.StopPropagation(); });
        _mapContainer.RegisterCallback<PointerUpEvent>(OnMapContainerRightClicked);
        
        _waypointCanvas = _root.Q<VisualElement>("waypoint-canvas");
        _contextCanvas = _root.Q<VisualElement>("context-menu-canvas");
        
        _contextMenu = _contextCanvas.Q<VisualElement>("context-menu");
        _contextMenu.focusable = true;
        _contextMenu.BringToFront();
        _contextMenu.RegisterCallback<FocusOutEvent>(OnLostFocus);
        _contextTitle = _contextMenu.Q<Label>("context-title");
        _waypointName = _contextMenu.Q<TextField>("waypoint-name");
        _colorYellow = _contextMenu.Q<Button>("yellow");
        _colorYellow.RegisterCallback<ClickEvent>(OnYellowClicked);
        _colorRed = _contextMenu.Q<Button>("red");
        _colorRed.RegisterCallback<ClickEvent>(OnRedClicked);
        _colorGreen = _contextMenu.Q<Button>("green");
        _colorGreen.RegisterCallback<ClickEvent>(OnGreenClicked);
        _colorBlue = _contextMenu.Q<Button>("blue");
        _colorBlue.RegisterCallback<ClickEvent>(OnBlueClicked);
        _colorGray = _contextMenu.Q<Button>("gray");
        _colorGray.RegisterCallback<ClickEvent>(OnGrayClicked);
        _addWaypoint = _contextMenu.Q<Button>("add");
        _addWaypoint.text = LocalizationStrings.WAYPOINT_BUTTON_ADD;
        _addWaypoint.RegisterCallback<ClickEvent>(OnAddWaypointClicked);
        _updateWaypoint = _contextMenu.Q<Button>("update");
        _updateWaypoint.text = LocalizationStrings.WAYPOINT_BUTTON_UPDATE;
        _updateWaypoint.RegisterCallback<ClickEvent>(OnUpdateWaypointClicked);
        _removeWaypoint = _contextMenu.Q<Button>("remove");
        _removeWaypoint.text = LocalizationStrings.WAYPOINT_BUTTON_REMOVE;
        _removeWaypoint.RegisterCallback<ClickEvent>(OnRemoveWaypointClicked);
        
        StartCoroutine(GetCanvasSize());
        StartCoroutine(RegisterForWindowResize());
        StartCoroutine(RegisterForZoomFactorChanged());
        StartCoroutine(RegisterForPanExecuted());
        InitializeExistingWaypoints();
       
        _LOGGER.LogInfo("Initialized.");
    }

    private IEnumerator GetCanvasSize()
    {
        // wait for 1 frame for the canvas to get its size
        yield return null;
        
        _canvasWidth = _waypointCanvas.layout.width;
        _canvasHeight = _waypointCanvas.layout.height;
        _waypointCanvasInitialized = true;
    }
    
    public void RebuildWaypointMarkers(string body) => StartCoroutine(Rebuild(body));
    private IEnumerator Rebuild(string body)
    {
        while(!_waypointCanvasInitialized)
        {
            yield return null;
        }
        
        _waypointCanvas.Clear();

        var waypoints = SceneController.Instance.Waypoints.FindAll(w => w.Body == body);

        foreach (var waypoint in waypoints)
        {
            PositionMarkerOnTheMap(waypoint.Marker, waypoint.MapPositionPercentage);
            waypoint.Marker.RegisterCallback<PointerDownEvent>(OnMarkerPointerDownEvent);
            waypoint.Marker.RegisterCallback<PointerUpEvent>(evt => OnMarkerPointerUpEvent(evt, waypoint));
            _waypointCanvas.Add(waypoint.Marker);
        }
    }
    
    /// <summary>
    /// Gets the new width and height of the canvas after window is resized by the player.
    /// Then repositions all waypoint markers. 
    /// </summary>
    private IEnumerator RegisterForWindowResize()
    {
        if (ResizeController.Instance == null)
        {
            yield return null;
        }
        
        _windowResizedHandler = (newWidth, newHeight) =>
        {
            _canvasWidth = _waypointCanvas.layout.width;
            _canvasHeight = _waypointCanvas.layout.height;
            RepositionAllWaypointControls();
        };

        ResizeController.Instance.OnWindowResized += _windowResizedHandler;
    }
    
    /// <summary>
    /// Repositions all waypoint markers when zoom factor is changed
    /// </summary>
    private IEnumerator RegisterForZoomFactorChanged()
    {
        if (ZoomAndPanController.Instance == null)
        {
            yield return null;
        }
        
        _zoomFactorChangeHandler = (zoomFactor) => RepositionAllWaypointControls();

        ZoomAndPanController.Instance.OnZoomFactorChanged += _zoomFactorChangeHandler;
    }
    
    /// <summary>
    /// Repositions all waypoint markers when a pan is executed
    /// </summary>
    private IEnumerator RegisterForPanExecuted()
    {
        if (ZoomAndPanController.Instance == null)
        {
            yield return null;
        }

        _panExecutedHandler = (panOffset) => RepositionAllWaypointControls();
        
        ZoomAndPanController.Instance.OnPanExecuted += _panExecutedHandler;
    }

    public void InitializeExistingWaypoints()
    {
        foreach (var waypointModel in SceneController.Instance.Waypoints)
        {
            StartCoroutine(CreateMarkerControl(waypointModel));
        }

        SceneController.Instance.WaypointInitialized = true;
    }

    private IEnumerator CreateMarkerControl(WaypointModel waypointModel)
    {
        waypointModel.Marker = new MapMarkerControl(
            name: waypointModel.Waypoint.Name,
            latitude: waypointModel.Waypoint.Latitude,
            longitude: waypointModel.Waypoint.Longitude,
            isNameVisible: SceneController.Instance.IsMarkerNamesVisible,
            isGeoCoordinatesVisible: SceneController.Instance.IsGeoCoordinatesVisible,
            type: MapMarkerControl.MarkerType.Waypoint);
                
        AddWaypointColor(waypointModel.Marker, waypointModel.Waypoint.WaypointColor);
        waypointModel.Marker.StopMouseEventsToGameInputPropagation();            
        
        waypointModel.Marker.RegisterCallback<PointerDownEvent>(OnMarkerPointerDownEvent);
        waypointModel.Marker.RegisterCallback<PointerUpEvent>(evt => OnMarkerPointerUpEvent(evt, waypointModel));
        
        PositionMarkerOnTheMap(waypointModel.Marker, waypointModel.MapPositionPercentage);
        
        // wait until ZoomAndPanController is initialized before registering control for Pan and Zoom events
        while (ZoomAndPanController.Instance == null)
        {
            yield return null;
        }
            
        ZoomAndPanController.Instance.RegisterControlForPanAndZooming(waypointModel.Marker);
    }
    
    
    ///// ADD WAYPOINT ///// 
    
    private void OnMapContainerRightClicked(PointerUpEvent evt)
    {
        if (evt.button == 1 && SceneController.Instance.SelectedBody != null) // Right mouse button
        {
            _LOGGER.LogDebug("RightClick detected");

            var adjustedCoordinates = UiUtility.GetAdjustedCanvasCoordinatesFromLocalPosition(
                localPosition: evt.localPosition,
                canvasWidth: ((VisualElement)evt.currentTarget).layout.width,
                canvasHeight: ((VisualElement)evt.currentTarget).layout.height);
            
            _waypointToAddPercentPosition = new Vector2(
                evt.localPosition.x / ((VisualElement)evt.currentTarget).layout.width,
                evt.localPosition.y / ((VisualElement)evt.currentTarget).layout.height
            );

            _contextTitle.text = LocalizationStrings.WAYPOINT_CONTEXT_MENU_TITLE_ADD;
            _waypointName.text = LocalizationStrings.WAYPOINT_DEFAULT_NAME;
            _addWaypoint.style.display = DisplayStyle.Flex;
            _updateWaypoint.style.display = DisplayStyle.None;
            _removeWaypoint.style.display = DisplayStyle.None;
            _contextMenu.style.left = adjustedCoordinates.x;
            _contextMenu.style.top = adjustedCoordinates.y;
            _contextMenu.Focus();
            ShowContextCanvas();
        }
    }
    
    // Colors
    private void OnYellowClicked(ClickEvent _)
    {
        _waypointToAddColor = WaypointColor.Yellow;
        _colorYellow.AddToClassList(UssColorButtonSelected);
        _colorRed.RemoveFromClassList(UssColorButtonSelected);
        _colorGreen.RemoveFromClassList(UssColorButtonSelected);
        _colorBlue.RemoveFromClassList(UssColorButtonSelected);
        _colorGray.RemoveFromClassList(UssColorButtonSelected);
    }
    
    private void OnRedClicked(ClickEvent _)
    {
        _waypointToAddColor = WaypointColor.Red;
        _colorYellow.RemoveFromClassList(UssColorButtonSelected);
        _colorRed.AddToClassList(UssColorButtonSelected);
        _colorGreen.RemoveFromClassList(UssColorButtonSelected);
        _colorBlue.RemoveFromClassList(UssColorButtonSelected);
        _colorGray.RemoveFromClassList(UssColorButtonSelected);
    }
    
    private void OnGreenClicked(ClickEvent _)
    {
        _waypointToAddColor = WaypointColor.Green;
        _colorYellow.RemoveFromClassList(UssColorButtonSelected);
        _colorRed.RemoveFromClassList(UssColorButtonSelected);
        _colorGreen.AddToClassList(UssColorButtonSelected);
        _colorBlue.RemoveFromClassList(UssColorButtonSelected);
        _colorGray.RemoveFromClassList(UssColorButtonSelected);
    }
    
    private void OnBlueClicked(ClickEvent _)
    {
        _waypointToAddColor = WaypointColor.Blue;
        _colorYellow.RemoveFromClassList(UssColorButtonSelected);
        _colorRed.RemoveFromClassList(UssColorButtonSelected);
        _colorGreen.RemoveFromClassList(UssColorButtonSelected);
        _colorBlue.AddToClassList(UssColorButtonSelected);
        _colorGray.RemoveFromClassList(UssColorButtonSelected);
    }
    
    private void OnGrayClicked(ClickEvent _)
    {
        _waypointToAddColor = WaypointColor.Gray;
        _colorYellow.RemoveFromClassList(UssColorButtonSelected);
        _colorRed.RemoveFromClassList(UssColorButtonSelected);
        _colorGreen.RemoveFromClassList(UssColorButtonSelected);
        _colorBlue.RemoveFromClassList(UssColorButtonSelected);
        _colorGray.AddToClassList(UssColorButtonSelected);
    }
    
    private void OnAddWaypointClicked(ClickEvent _)
    {
        var waypointModel = new WaypointModel
        {
            Body = SceneController.Instance.SelectedBody,
            MapPositionPercentage = _waypointToAddPercentPosition
        };

        (double latitude, double longitude) geographicCoordinates =
            UiUtility.GetGeographicCoordinatesFromPositionPercent(_waypointToAddPercentPosition.x,
                1 - _waypointToAddPercentPosition.y);

        waypointModel.Waypoint = new OrbitalSurveyWaypoint(
            latitude: geographicCoordinates.latitude,
            longitude: geographicCoordinates.longitude,
            altitudeFromRadius: null,
            bodyName: SceneController.Instance.SelectedBody,
            name: _waypointName.text,
            waypointState: WaypointState.Visible,
            waypointColor: _waypointToAddColor
        );

        StartCoroutine(CreateMarkerControl(waypointModel));

        _waypointCanvas.Add(waypointModel.Marker);
        SceneController.Instance.Waypoints.Add(waypointModel);

        HideContextCanvas();
        
        _mainGuiController.ShowNotification(LocalizationStrings.NOTIFICATIONS[Notification.WaypointAdded]);
        
        _LOGGER.LogInfo($"Waypoint '{waypointModel.Waypoint.Name}' created.");
    }
    
    
    ///// UPDATE WAYPOINT /////

    /// <summary>
    /// Stops pointer event propagation if right click is detected on the control (edit waypoint)
    /// </summary>
    private void OnMarkerPointerDownEvent(PointerDownEvent evt)
    {
        if (evt.button == 1) evt.StopPropagation();   
    }
    
    /// <summary>
    /// Starts Edit waypoint
    /// </summary>
    private void OnMarkerPointerUpEvent(PointerUpEvent evt, WaypointModel waypoint)
    {
        if (evt.button == 1) // Right mouse button
        {
            _selectedWaypointModel = waypoint;
            _contextTitle.text = LocalizationStrings.WAYPOINT_CONTEXT_MENU_TITLE_EDIT;
            _waypointName.text = waypoint.Waypoint.Name;
            _waypointToAddColor = waypoint.Waypoint.WaypointColor;
            switch (_waypointToAddColor)
            {
                case WaypointColor.Yellow: OnYellowClicked(null);  break;
                case WaypointColor.Red:  OnRedClicked(null);  break;
                case WaypointColor.Green: OnGreenClicked(null); break;
                case WaypointColor.Blue: OnBlueClicked(null); break;
                case WaypointColor.Gray: OnGrayClicked(null); break;
                default: OnYellowClicked(null); break;
            }
            _addWaypoint.style.display = DisplayStyle.None;
            _updateWaypoint.style.display = DisplayStyle.Flex;
            _removeWaypoint.style.display = DisplayStyle.Flex;
            _contextMenu.style.left = waypoint.Marker.style.left;
            _contextMenu.style.top = waypoint.Marker.style.top;
            _contextMenu.Focus();
            ShowContextCanvas();
        }
    }
    
    private void OnUpdateWaypointClicked(ClickEvent evt)
    {
        _selectedWaypointModel.Waypoint.Rename(_waypointName.text);
        _selectedWaypointModel.Marker.NameValue = _waypointName.text;
        _selectedWaypointModel.Waypoint.WaypointColor = _waypointToAddColor;
        switch (_waypointToAddColor)
        {
            case WaypointColor.Yellow: _selectedWaypointModel.Marker.SetAsYellow(); break;
            case WaypointColor.Red:  _selectedWaypointModel.Marker.SetAsRed();  break;
            case WaypointColor.Green: _selectedWaypointModel.Marker.SetAsGreen(); break;
            case WaypointColor.Blue: _selectedWaypointModel.Marker.SetAsBlue();; break;
            case WaypointColor.Gray: _selectedWaypointModel.Marker.SetAsGray();; break;
            default: _selectedWaypointModel.Marker.SetAsYellow(); break;
        }
        
        HideContextCanvas();
        _selectedWaypointModel = null;
        
        _mainGuiController.ShowNotification(LocalizationStrings.NOTIFICATIONS[Notification.WaypointUpdated]);
    }

    private void OnRemoveWaypointClicked(ClickEvent evt)
    {
        _selectedWaypointModel.Waypoint.Destroy();
        _waypointCanvas.Remove(_selectedWaypointModel.Marker);
        SceneController.Instance.Waypoints.Remove(_selectedWaypointModel);
        
        HideContextCanvas();
        _selectedWaypointModel = null;
        
        _mainGuiController.ShowNotification(LocalizationStrings.NOTIFICATIONS[Notification.WaypointRemoved]);
    }

    private void PositionMarkerOnTheMap(MapMarkerControl control, Vector2 mapPositionPercentage)
    {
        var scaledCoordinates = UiUtility.GetAdjustedCanvasCoordinatesFromMapPositionPercentage(
            mapPositionPercentage, _canvasWidth, _canvasHeight);
        control.style.left = scaledCoordinates.x;
        control.style.top = scaledCoordinates.y;
    }
    
    /// <summary>
    /// Refreshes positions of all waypoint markers.
    /// Called after a UI event triggered that requires repositioning (zoom, pan, resize)
    /// </summary>
    private void RepositionAllWaypointControls()
    {
        foreach (var waypoint in SceneController.Instance.Waypoints)
        {
            var scaledCoordinates = UiUtility.GetAdjustedCanvasCoordinatesFromMapPositionPercentage(
                waypoint.MapPositionPercentage, _canvasWidth, _canvasHeight);
            waypoint.Marker.style.left = scaledCoordinates.x;
            waypoint.Marker.style.top = scaledCoordinates.y;
        }
    }
    
    private void OnLostFocus(FocusOutEvent evt)
    {
        if (_contextMenu.HasContainerLostFocus((VisualElement)evt.relatedTarget))
        {
            HideContextCanvas();
            _selectedWaypointModel = null;
        }
    }

    private void HideContextCanvas()
    {
        _contextCanvas.style.display = DisplayStyle.None;
    }
    
    private void ShowContextCanvas()
    {
        _contextCanvas.style.display = DisplayStyle.Flex;
    }
    
    public void ToggleGeoCoordinates()
    {
        foreach (var waypoint in SceneController.Instance.Waypoints)
        {
            waypoint.Marker.SetGeoCoordinatesVisibility(SceneController.Instance.IsGeoCoordinatesVisible);
        }
    }

    public void ToggleWaypointNames()
    {
        foreach (var waypoint in SceneController.Instance.Waypoints)
        {
            waypoint.Marker.SetNameVisibility(SceneController.Instance.IsMarkerNamesVisible);
        }
    }

    public void AddWaypointColor(MapMarkerControl control, WaypointColor color)
    {
        switch (color)
        {
            case WaypointColor.Yellow: control.SetAsYellow(); 
                break;
            case WaypointColor.Red: control.SetAsRed(); 
                break;
            case WaypointColor.Green: control.SetAsGreen(); 
                break;
            case WaypointColor.Blue: control.SetAsBlue(); 
                break;
            case WaypointColor.Gray: control.SetAsGray(); 
                break;
            default: control.SetAsYellow();
                break;
        } 
    }
    
    private void OnDestroy()
    {
        _contextMenu.UnregisterCallback<FocusOutEvent>(OnLostFocus);
        _addWaypoint.UnregisterCallback<ClickEvent>(OnAddWaypointClicked);

        // foreach (var waypoint in SceneController.Instance.Waypoints)
        // {
        //     waypoint.Marker.UnregisterCallback<PointerDownEvent>(OnMarkerPointerDownEvent);
        // }
        //_removeWaypoint.UnregisterCallback<ClickEvent>(OnRemoveWaypointClicked);
    }
}