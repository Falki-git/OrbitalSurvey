using OrbitalSurvey.Models;
using OrbitalSurvey.Utilities;
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

        if (WindowPosition == null)
            uiDocument.rootVisualElement[0].RegisterCallback<GeometryChangedEvent>((evt) => UiUtility.CenterWindow(evt, uiDocument.rootVisualElement[0]));

        return uiDocument;
    }
}
