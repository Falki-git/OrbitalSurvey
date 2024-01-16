using BepInEx.Logging;
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
    
    public delegate void ZoomFactorChanged(float zoomFactor);
    public event ZoomFactorChanged OnZoomFactorChanged;
    
    public delegate void PanExecuted(Vector2 panOffset);
    public event PanExecuted OnPanExecuted;
    
    private VisualElement _root;
    private VisualElement _mapContainer;
    private Button _inc;
    private Button _dec;

    private bool _isPanningRegistered;
    private bool _isPanning;
    private Vector3 _lastMousePosition;

    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.ZoomController");
    
    public void Start()
    {
        Instance = this;
        _root = GetComponent<UIDocument>().rootVisualElement[0];
        _mapContainer = _root.Q<VisualElement>("map");
        
        _inc = _root.Q<Button>("inc");
        _dec = _root.Q<Button>("dec");
        
        _inc.RegisterCallback<ClickEvent>(evt => IncreaseScale(evt));
        _dec.RegisterCallback<ClickEvent>(evt => DecreaseScale(evt));
        
        _mapContainer.RegisterCallback<WheelEvent>(OnMouseScroll);

        // see if dragging the mouse over the canvas should drag the window or pan the map
        OnZoomFactorChanged += SwitchDraggingOrPanningIfNeeded;
        
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
    
    #endregion
}