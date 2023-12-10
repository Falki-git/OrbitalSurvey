using KSP.UI.Binding;
using OrbitalSurvey.Models;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace OrbitalSurvey.UI;

public class SceneController
{
    public static SceneController Instance { get; } = new();
    public UIDocument MainGui { get; set; }

    public string SelectedBody;
    public MapType SelectedMapType;
    public Vector3? WindowPosition;
    
    private bool _showMainGui;
    public bool ShowMainGui
    {
        get => _showMainGui;
        set
        {
            _showMainGui = value;
            MainGui = RebuildUi(MainGui, value, Uxmls.Instance.MainGui, "MainGui", typeof(MainGuiController));
        }
    }
    
    private UIDocument RebuildUi(UIDocument uidocument, bool showWindow, VisualTreeAsset visualTree, string windowId, Type controllerType)
    {
        DestroyObject(uidocument);
        if (showWindow)
            return BuildUi(visualTree, windowId, uidocument, controllerType);
        else
            return null;
    }

    public void DestroyObject(UIDocument document)
    {
        if (document != null && document.gameObject != null)
            document.gameObject.DestroyGameObject();
        GameObject.Destroy(document);
    }

    private UIDocument BuildUi(VisualTreeAsset visualTree, string windowId, UIDocument uiDocument, Type controllerType)
    {
        uiDocument = Window.CreateFromUxml(visualTree, windowId, null, true);
        uiDocument.gameObject.AddComponent(controllerType);

        // this can center the window, but we're not using it
        // if (WindowPosition == null)
        //     uiDocument.rootVisualElement[0].RegisterCallback<GeometryChangedEvent>((evt) => UiUtility.CenterWindow(evt, uiDocument.rootVisualElement[0]));

        return uiDocument;
    }

    public void ToggleUI(bool state)
    {
        ShowMainGui = state;
        
        GameObject.Find(OrbitalSurveyPlugin.ToolbarFlightButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(state);
        GameObject.Find(OrbitalSurveyPlugin.ToolbarOABButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(state);
    }

    public void ToggleUI() => ToggleUI(!ShowMainGui);
}
