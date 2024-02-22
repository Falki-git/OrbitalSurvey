using BepInEx.Logging;
using JetBrains.Annotations;
using OrbitalSurvey.Models;
using OrbitalSurvey.UI.Controls;
using OrbitalSurvey.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = BepInEx.Logging.Logger;

namespace OrbitalSurvey.UI;

/// <summary>
/// Handles zooming and panning of the Main GUI
/// </summary>
public class ZoomAndPanController: MonoBehaviour
{
    public static ZoomAndPanController Instance;
    public float ZoomFactor => _mapContainer.transform.scale.x;

    private Vector2 _panOffset;
    public Vector2 PanOffset
    {
        get => _panOffset;
        set
        {
            _panOffset = value;
            OnPanExecuted?.Invoke(_panOffset);
        }
    }

    public event Action<float> OnZoomFactorChanged;
    public event Action<Vector2> OnPanExecuted;
    
    private VisualElement _root;
    private MainGuiController _mainGuiController;
    private VisualElement _mapContainer;
    private VisualElement _zoomControls;
    private Label _zoomFactorLabel;
    private Button _trackVessel;
    private Button _zoomIn;
    private Button _zoomOut;
    
    private bool _isPanningRegistered;
    private bool _isPanning;
    private Vector3 _lastMousePosition;
    private bool _isTrackingActiveVessel;

    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.ZoomController");
    private const string _TRACK_VESSEL_USS = "toggled"; 
    
    public void Start()
    {
        Instance = this;
        _mainGuiController = GetComponent<MainGuiController>();
        _root = GetComponent<UIDocument>().rootVisualElement[0];
        _mapContainer = _root.Q<VisualElement>("map");
        _mapContainer.StopMouseEventsToGameInputPropagation();
        
        _zoomControls = _root.Q<VisualElement>("zoom-controls");
        _zoomFactorLabel = _root.Q<Label>("zoom-factor"); 
        _trackVessel = _root.Q<Button>("track-vessel");
        _zoomIn = _root.Q<Button>("zoom-in");
        _zoomOut = _root.Q<Button>("zoom-out");
        
        _trackVessel.RegisterCallback<ClickEvent>(evt => OnTrackVesselClicked(evt));
        _zoomIn.RegisterCallback<ClickEvent>(evt => IncreaseScale(evt));
        _zoomOut.RegisterCallback<ClickEvent>(evt => DecreaseScale(evt));
        
        _mapContainer.RegisterCallback<WheelEvent>(OnMouseScroll);

        // see if dragging the mouse over the canvas should drag the window or pan the map
        OnZoomFactorChanged += SwitchDraggingOrPanningIfNeeded;
        OnZoomFactorChanged += UpdateZoomFactorLabel;
        
        _LOGGER.LogInfo("Initialized.");
    }

    /// <summary>
    /// If zoom factor is 1.0 (no zoom), then window dragging should be enabled.
    /// If it's > 1.0 then map needs to pan when dragging on canvas
    /// </summary>
    /// <param name="zoomFactor"></param>
    private void SwitchDraggingOrPanningIfNeeded(float zoomFactor)
    {
        bool panningShouldBeRegistered = zoomFactor > 1.0f;

        if (panningShouldBeRegistered && !_isPanningRegistered)
        {
            _mapContainer.RegisterCallback<PointerDownEvent>(OnPanStarting);
            _mapContainer.RegisterCallback<PointerMoveEvent>(OnPanMoving);
            _mapContainer.RegisterCallback<PointerUpEvent>(OnPanEnding);
            _isPanningRegistered = true;
        }
        else if (!panningShouldBeRegistered && _isPanningRegistered)
        {
            _mapContainer.UnregisterCallback<PointerDownEvent>(OnPanStarting);
            _mapContainer.UnregisterCallback<PointerMoveEvent>(OnPanMoving);
            _mapContainer.UnregisterCallback<PointerUpEvent>(OnPanEnding);
            ResetPan();
            _isPanningRegistered = false;
        }
    }

    #region Zooming

    /// <summary>
    /// Initiates zoom IN
    /// </summary>
    /// <param name="factor">If null or 0 is passed, then the default zoom factor will be used</param>
    private void IncreaseScale(ClickEvent _, float factor = 0)
    {
        factor = factor == 0 ? Settings.ZoomFactor.Value : factor;
        
        ChangeScale(factor);
    }

    /// <summary>
    /// Initiates zoom OUT
    /// </summary>
    /// <param name="factor">If null or 0 is passed, then the default zoom factor will be used</param>
    private void DecreaseScale(ClickEvent _, float factor = 0)
    {
        factor = factor == 0 ? Settings.ZoomFactor.Value : factor;
        
        ChangeScale(1 / factor);
    }

    /// <summary>
    /// Executes zoom IN or zoom OUT
    /// </summary>
    /// <param name="factor">Factor at which the scale (zoom) will be adjusted</param>
    private void ChangeScale(float factor)
    {
        _mapContainer.transform.scale *= factor;
        if (_mapContainer.transform.scale.x < 1f)
        {
            _mapContainer.transform.scale = new Vector3(1f, 1f, 0);
        }
        else if (_mapContainer.transform.scale.x > Settings.MaxZoom.Value)
        {
            _mapContainer.transform.scale = new Vector3(Settings.MaxZoom.Value, Settings.MaxZoom.Value, 0);
        }
        
        //_mapContainer.transform.position = new Vector3(PanOffset.x * ZoomFactor, PanOffset.y * ZoomFactor, 0);
        AdjustPanOffsetIfMaxReached();
        
        OnZoomFactorChanged?.Invoke(ZoomFactor);
    }
    
    /// <summary>
    /// Initiates a zoom in or zoom out
    /// </summary>
    public void OnMouseScroll(WheelEvent evt)
    {
        /*
        // attempt at panning the canvas to the mouse position on where the wheelevent occured.
        // something was missing in this implementation, so we'll shelve this for now and revisit
        // when someone smarter tackles it
        var position = evt.localMousePosition;
        var width = ((VisualElement)evt.currentTarget).resolvedStyle.width;
        var height = ((VisualElement)evt.currentTarget).resolvedStyle.height;
        var positionPercentage = new Vector2((position.x - width / 2) / width, (position.y - height / 2) / height);
        var panOffSetAfterWheelEvent = new Vector2(position.x - width / 2, position.y - height / 2);
        panOffSetAfterWheelEvent *= -1;
        */
        
        if (evt.delta.y < 0)
        {
            IncreaseScale(null, Settings.ZoomFactor.Value);
        }
        else
        {
            DecreaseScale(null, Settings.ZoomFactor.Value);
        }
        
        evt.StopPropagation();
    }
    
    private void UpdateZoomFactorLabel(float zoomFactor)
    {
        _zoomFactorLabel.text = $"{zoomFactor:F1}";
    }
    
    #endregion
    
    #region Panning
    
    /// <summary>
    /// Initiates canvas panning
    /// </summary>
    public void OnPanStarting(PointerDownEvent evt)
    {
        if (evt.button == 0) // Left mouse button
        {
            _isPanning = true;
            _lastMousePosition = evt.position;
            _mapContainer.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }
    }

    /// <summary>
    /// Handles panning. Clamps panning to the max offset to prevent the map from panning too far inside the canvas
    /// </summary>
    public void OnPanMoving(PointerMoveEvent evt)
    {
        if (_isPanning)
        {
            Vector3 delta = evt.position - _lastMousePosition;
            
            //calculate the maximum pan offset given the zoom factor
            var canvasWidth = _mapContainer.layout.width;
            var canvasHeight = _mapContainer.layout.height;
            var maxPanOffset = new Vector2((canvasWidth - canvasWidth / ZoomFactor) / 2, (canvasHeight - canvasHeight / ZoomFactor) / 2);

            // calculate the new pan offset and clamp it to the maximum value
            var newPanOffset = new Vector2(
                Mathf.Clamp(delta.x / ZoomFactor + PanOffset.x, -maxPanOffset.x, maxPanOffset.x),
                Mathf.Clamp(delta.y / ZoomFactor + PanOffset.y, -maxPanOffset.y, maxPanOffset.y));
            
            // set the new position
            _mapContainer.transform.position = new Vector3(newPanOffset.x * ZoomFactor, newPanOffset.y * ZoomFactor, 0);
            
            _lastMousePosition = evt.position;

            // update the PanOffset property (will trigger vessel marker readjustment)
            PanOffset = newPanOffset;
            
            evt.StopPropagation();
        }
    }

    /// <summary>
    /// Keeps the pan offset in check by preventing the texture from being moved outside of canvas bounds
    /// </summary>
    private void AdjustPanOffsetIfMaxReached()
    {
        var canvasWidth = _mapContainer.layout.width;
        var canvasHeight = _mapContainer.layout.height;
        var maxPanOffset = new Vector2((canvasWidth - canvasWidth / ZoomFactor) / 2, (canvasHeight - canvasHeight / ZoomFactor) / 2);

        // clamp the pan offset to the maximum value
        var newPanOffset = new Vector2(
            Mathf.Clamp(PanOffset.x, -maxPanOffset.x, maxPanOffset.x),
            Mathf.Clamp(PanOffset.y, -maxPanOffset.y, maxPanOffset.y));
            
        // set the new position
        _mapContainer.transform.position = new Vector3(newPanOffset.x * ZoomFactor, newPanOffset.y * ZoomFactor, 0);

        // update the PanOffset property (will trigger vessel marker readjustment)
        PanOffset = newPanOffset;
    }

    public void OnPanEnding(PointerUpEvent evt)
    {
        if (_isPanning && evt.button == 0) // Left mouse button
        {
            _isPanning = false;
            _mapContainer.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }
    }

    private void ResetPan()
    {
        _mapContainer.transform.position = new Vector3(0, 0, 0);
        PanOffset = new Vector2(0, 0);
    }

    /// <summary>
    /// Tracks the current active vessel
    /// </summary>
    /// <param name="_"></param>
    /// <param name="forceNewState">Optional override for tracking</param>
    private void OnTrackVesselClicked(ClickEvent _, bool? forceNewState = null)
    {
        var newState = forceNewState ?? !_isTrackingActiveVessel;
        
        if (newState)
        {
            _trackVessel.AddToClassList(_TRACK_VESSEL_USS);
            VesselController.Instance.OnActiveVesselMapGuiPositionChanged += PanCanvasToActiveVesselPosition;
        }
        else
        {
            _trackVessel.RemoveFromClassList(_TRACK_VESSEL_USS);
            VesselController.Instance.OnActiveVesselMapGuiPositionChanged -= PanCanvasToActiveVesselPosition;
        }
        
        // send notification
        var notificationText = newState ?
            LocalizationStrings.NOTIFICATIONS[Notification.VesselTrackingOn] :
            LocalizationStrings.NOTIFICATIONS[Notification.VesselTrackingOff];
        _mainGuiController.ShowNotification(notificationText);

        _isTrackingActiveVessel = newState;
    }

    /// <summary>
    /// Moves the canvas to follow the active vessel (if active vessel tracking is toggled on)
    /// </summary>
    /// <param name="vesselPosition">At what texture % _should_ the vessel be positioned if canvas didn't follow it</param>
    private void PanCanvasToActiveVesselPosition((float percentX, float percentY) vesselPosition)
    {
        // calculate the scaled position the vessel _should_ be
        var newPosition = new Vector2(
            -1 * (_mapContainer.layout.width * vesselPosition.percentX - _mapContainer.layout.width / 2) * ZoomFactor,
            -1 * (_mapContainer.layout.height * (1 - vesselPosition.percentY) - _mapContainer.layout.height / 2) * ZoomFactor
        );
        
        // set the scaled position to the canvas.
        // this locks the canvas to where the vessel should be (but isn't because canvas is following it) 
        _mapContainer.transform.position = new Vector3(newPosition.x, newPosition.y, 0);
        PanOffset = newPosition / ZoomFactor;
        
        // clamping if the canvas is near the edge
        AdjustPanOffsetIfMaxReached();
    }

    #endregion

    [PublicAPI]
    public void RegisterControlForPanAndZooming(MapMarkerControl control)
    {
        control.RegisterCallback<PointerDownEvent>(OnPanStarting);
        control.RegisterCallback<PointerMoveEvent>(OnPanMoving);
        control.RegisterCallback<PointerUpEvent>(OnPanEnding);
        control.RegisterCallback<WheelEvent>(OnMouseScroll);
    }
    
    private void OnDestroy()
    {
        OnZoomFactorChanged -= SwitchDraggingOrPanningIfNeeded;
        OnZoomFactorChanged -= UpdateZoomFactorLabel;

        if (VesselController.Instance != null)
        {
            VesselController.Instance.OnActiveVesselMapGuiPositionChanged -= PanCanvasToActiveVesselPosition;    
        }
    }
}