using UnityEngine;
using UnityEngine.UIElements;

namespace OrbitalSurvey.UI.Controls;

public class LegendKeyControl: VisualElement
{
    public static string UssClassName = "legend-key";
    public static string UssColorClassName = UssClassName + "__color";
    public static string UssTextClassName = UssClassName + "__text";

    public VisualElement ColorElement;
    public Label TextLabel;

    public Color32 ColorValue
    {
        get => ColorElement.style.backgroundColor.value;
        set => ColorElement.style.backgroundColor = (Color)value;
    }
    
    public string TextValue
    {
        get => TextLabel.text;
        set => TextLabel.text = value;
    }

    public LegendKeyControl(Color32 color, string text) : this()
    {
        ColorValue = color;
        TextValue = text;
    }
    
    public LegendKeyControl()
    {
        AddToClassList(UssClassName);

        ColorElement = new VisualElement()
        {
            name = "color-element"
        };
        ColorElement.AddToClassList(UssColorClassName);
        hierarchy.Add(ColorElement);
        
        TextLabel = new Label()
        {
            name = "text-label",
            text = string.Empty
        };
        TextLabel.AddToClassList(UssTextClassName);
        hierarchy.Add(TextLabel);
    }
    
    public new class UxmlFactory : UxmlFactory<LegendKeyControl, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        private UxmlColorAttributeDescription _color = new UxmlColorAttributeDescription()
            { name = "color", defaultValue = Color.black };
        UxmlStringAttributeDescription _text = new UxmlStringAttributeDescription()
            { name = "text", defaultValue = "NameOfControl" };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            if (ve is LegendKeyControl control)
            {
                control.ColorValue = _color.GetValueFromBag(bag, cc);
                control.TextValue = _text.GetValueFromBag(bag, cc);
            }
        }
    }
}