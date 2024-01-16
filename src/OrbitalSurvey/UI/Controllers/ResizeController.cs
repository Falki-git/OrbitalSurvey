using System.Collections;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = BepInEx.Logging.Logger;

namespace OrbitalSurvey.UI;

/// <summary>
/// Handles the Main GUI resizer control
/// </summary>
public class ResizeController : MonoBehaviour
{
    public static ResizeController Instance;
    
    public delegate void WindowResized(float newWidth, float newHeight);
    public event WindowResized OnWindowResized;
    
    private VisualElement _root;
    private VisualElement _resizeHandle;
    
    private float _minWindowWidth;
    private float _minWindowHeight;
    private float _aspectRatio;
    private VisualElement _canvas;
    
    private bool _isResizing;
    private Vector3 _initialMousePosition;
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.ResizeController");
    
    public void Start()
    {
        Instance = this;
        _root = GetComponent<UIDocument>().rootVisualElement[0];
        
        _resizeHandle = _root.Q("resize-handle__container");
        _resizeHandle.RegisterCallback<PointerDownEvent>(OnResizeHandleMouseDown);
        _resizeHandle.RegisterCallback<PointerMoveEvent>(OnResizeHandleMouseMove);
        _resizeHandle.RegisterCallback<PointerUpEvent>(OnResizeHandleMouseUp);
        
        StartCoroutine(GetMinWindowSize());
        
        _LOGGER.LogInfo("Initialized.");
    }
    
    private IEnumerator GetMinWindowSize()
    {
        // wait for 1 frame for the root to get its size
        yield return null;
        
        _minWindowWidth = _root.layout.width;
        _minWindowHeight = _root.layout.height;
        _aspectRatio = _minWindowWidth / _minWindowHeight;
    }
    
    private void OnResizeHandleMouseDown(PointerDownEvent evt)
    {
        _isResizing = true;
        _initialMousePosition = evt.position;
        _resizeHandle.CapturePointer(evt.pointerId);
        evt.StopPropagation();
    }

    private void OnResizeHandleMouseUp(PointerUpEvent evt)
    {
        _isResizing = false;
        _resizeHandle.ReleasePointer(evt.pointerId);
        evt.StopPropagation();
    }

    private void OnResizeHandleMouseMove(PointerMoveEvent evt)
    {
        if (_isResizing)
        {
            Vector2 delta = evt.position - _initialMousePosition;
            ChangeWindowSize(delta);
            _initialMousePosition = evt.position;
            evt.StopPropagation();
        }
    }

    private void ChangeWindowSize(Vector2 delta2d)
    {
        var resolvedStyle = _root.resolvedStyle;
        var style = _root.style;
        var deltaX = delta2d.x;
        var deltaY = deltaX / _aspectRatio;

        var currentWidth = resolvedStyle.width;
        var currentHeight = resolvedStyle.height;

        var newWidth = Math.Clamp(currentWidth + deltaX, _minWindowWidth, float.MaxValue);
        var newHeight = Math.Clamp(currentHeight + deltaY, _minWindowHeight, float.MaxValue);

        style.width = newWidth;
        style.height = newHeight;
        
        OnWindowResized?.Invoke(newWidth, newHeight);
    }
}