using KSP.Game;
using UnityEngine.UIElements;

namespace OrbitalSurvey.Utilities;

/// <summary>
/// Taken from Science Arkive: https://github.com/Kerbalight/ScienceArkive/blob/main/src/ScienceArkive/API/Extensions/UIToolkitExtensions.cs#L19
/// </summary>
public static class UIToolkitExtensions
{
    public static GameInstance Game => GameManager.Instance.Game;

    /// <summary>
    /// Stop the mouse events (scroll and click) from propagating to the game (e.g. zoom).
    /// The only place where the Click still doesn't get stopped is in the MapView, neither the Focus or the Orbit mouse events.
    /// </summary>
    /// <param name="element"></param>
    public static void StopMouseEventsPropagation(this VisualElement element)
    {
        element.RegisterCallback<PointerEnterEvent>(OnVisualElementPointerEnter);
        element.RegisterCallback<PointerLeaveEvent>(OnVisualElementPointerLeave);
    }

    private static void OnVisualElementPointerEnter(PointerEnterEvent evt)
    {
        Game.Input.Flight.CameraZoom.Disable();
        Game.Input.Flight.mouseDoubleTap.Disable();
        Game.Input.Flight.mouseSecondaryTap.Disable();

        Game.Input.MapView.cameraZoom.Disable();
        Game.Input.MapView.Focus.Disable();
        Game.Input.MapView.mousePrimary.Disable();
        Game.Input.MapView.mouseSecondary.Disable();
        Game.Input.MapView.mouseTertiary.Disable();
        Game.Input.MapView.mousePosition.Disable();

        Game.Input.VAB.cameraZoom.Disable();
        Game.Input.VAB.mousePrimary.Disable();
        Game.Input.VAB.mouseSecondary.Disable();
    }

    private static void OnVisualElementPointerLeave(PointerLeaveEvent evt)
    {
        Game.Input.Flight.CameraZoom.Enable();
        Game.Input.Flight.mouseDoubleTap.Enable();
        Game.Input.Flight.mouseSecondaryTap.Enable();

        Game.Input.MapView.cameraZoom.Enable();
        Game.Input.MapView.Focus.Enable();
        Game.Input.MapView.mousePrimary.Enable();
        Game.Input.MapView.mouseSecondary.Enable();
        Game.Input.MapView.mouseTertiary.Enable();
        Game.Input.MapView.mousePosition.Enable();

        Game.Input.VAB.cameraZoom.Enable();
        Game.Input.VAB.mousePrimary.Enable();
        Game.Input.VAB.mouseSecondary.Enable();
    }
}