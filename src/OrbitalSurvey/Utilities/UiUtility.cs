using UnityEngine;
using UitkForKsp2.API;
using UnityEngine.UIElements;

namespace OrbitalSurvey.Utilities;

public static class UiUtility
{
    /// <summary>
    /// Centered UITK window on GeometryChangedEvent
    /// </summary>
    /// <param name="evt"></param>
    /// <param name="element">Root element for which width and height will be taken</param>
    public static void CenterWindow(GeometryChangedEvent evt, VisualElement element)
    {
        if (evt.newRect.width == 0 || evt.newRect.height == 0)
            return;

        element.transform.position = new Vector2((ReferenceResolution.Width - evt.newRect.width) / 2, (ReferenceResolution.Height - evt.newRect.height) / 2);
        element.UnregisterCallback<GeometryChangedEvent>((evt) => CenterWindow(evt, element));
    }
    
}