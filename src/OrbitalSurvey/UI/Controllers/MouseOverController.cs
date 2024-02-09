using BepInEx.Logging;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Models;
using OrbitalSurvey.UI.Controls;
using OrbitalSurvey.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = BepInEx.Logging.Logger;

namespace OrbitalSurvey.UI;

public class MouseOverController: MonoBehaviour
{
    public static MouseOverController Instance;
    
    private VisualElement _root;
    private VisualElement _mapContainer;
    private VisualElement _mouseOverCanvas;
    private MapMarkerControl _mouseOverControl;
    
    private float _canvasWidth;
    private float _canvasHeight;
    private bool _mouseOverCanvasInitialized;

    private bool _isMouseOverActive;

    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.MouseOverController");
    
    public void Start()
    {
        Instance = this;
        
        _root = GetComponent<UIDocument>().rootVisualElement[0];
        
        _mapContainer = _root.Q<VisualElement>("map");
        _mouseOverCanvas = _root.Q<VisualElement>("mouse-over-canvas");
        
        _mapContainer.RegisterCallback<PointerEnterEvent>(OnPointerEnterEvent);
        _mapContainer.RegisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
        _mapContainer.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent);

        _mouseOverControl = new MapMarkerControl(SceneController.Instance.IsMarkerNamesVisible,
            SceneController.Instance.IsGeoCoordinatesVisible, MapMarkerControl.MarkerType.MouseOver);
        _mouseOverCanvas.Add(_mouseOverControl);
    }
    
    private void OnPointerEnterEvent(PointerEnterEvent _)
    {
        if (SceneController.Instance.SelectedBody == null)
            return;
        
        if (SceneController.Instance.IsGeoCoordinatesVisible || SceneController.Instance.IsMarkerNamesVisible)
        {
            _mouseOverControl.style.display = DisplayStyle.Flex;
            _isMouseOverActive = true;
        }
    }
    
    private void OnPointerMoveEvent(PointerMoveEvent evt)
    {
        if (!_isMouseOverActive)
            return;
        
        var adjustedCoordinates = UiUtility.GetAdjustedCanvasCoordinatesFromLocalPosition(
            localPosition: evt.localPosition,
            canvasWidth: ((VisualElement)evt.currentTarget).layout.width,
            canvasHeight: ((VisualElement)evt.currentTarget).layout.height);
        
        _mouseOverControl.style.left = adjustedCoordinates.x;
        _mouseOverControl.style.top = adjustedCoordinates.y;
        
        // update latitude & longitude
        var positionPercentage = new Vector2(
            evt.localPosition.x / ((VisualElement)evt.currentTarget).layout.width,
            evt.localPosition.y / ((VisualElement)evt.currentTarget).layout.height
        );
        
        (double latitude, double longitude) geographicCoordinates =
            UiUtility.GetGeographicCoordinatesFromPositionPercent(positionPercentage.x, 1 - positionPercentage.y);

        _mouseOverControl.LatitudeValue = geographicCoordinates.latitude;
        _mouseOverControl.LongitudeValue = geographicCoordinates.longitude;
        
        // update region name, but only if the region map is complete
        if (Core.Instance.CelestialDataDictionary[SceneController.Instance.SelectedBody]
            .Maps[MapType.Biome].IsFullyScanned)
        {
            _mouseOverControl.NameValue = UiUtility.GetRegionNameFromGeographicCoordinates(SceneController.Instance.SelectedBody,
                geographicCoordinates.latitude, geographicCoordinates.longitude);
        }
        else
        {
            _mouseOverControl.NameValue = string.Empty;
        }
    }

    private void OnPointerLeaveEvent(PointerLeaveEvent _)
    {
        _mouseOverControl.style.display = DisplayStyle.None;
        _isMouseOverActive = false;
    }
    
    public void ToggleGeoCoordinates()
    {
        _mouseOverControl.SetGeoCoordinatesVisibility(SceneController.Instance.IsGeoCoordinatesVisible);
    }

    public void ToggleRegionNames()
    {
        _mouseOverControl.SetNameVisibility(SceneController.Instance.IsMarkerNamesVisible);
    }
}