using UnityEngine;
using UnityEngine.UIElements;

namespace OrbitalSurvey.UI.Controls
{
    [UxmlElement]
    public partial class LegendKeyControl: VisualElement
    {
        public static string UssClassName = "legend-key";
        public static string UssColorClassName = UssClassName + "__color";
        public static string UssTextClassName = UssClassName + "__text";

        public VisualElement ColorElement;
        public Label TextLabel;

        [UxmlAttribute]
        public Color32 ColorValue
        {
            get => ColorElement.style.backgroundColor.value;
            set => ColorElement.style.backgroundColor = (Color)value;
        }
        
        [UxmlAttribute]
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
    }
}

