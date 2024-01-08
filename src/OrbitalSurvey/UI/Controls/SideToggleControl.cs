using UnityEngine.UIElements;

namespace OrbitalSurvey.UI.Controls
{
    public class SideToggleControl : Toggle
    {
        public const string UssClassName = "side-button";
        public const string UssClassName_Connector = UssClassName + "__connector";
        public const string UssClassName_Container = UssClassName + "__container";
        public const string UssClassName_Led = UssClassName + "__led";
        public const string UssClassName_Text = UssClassName + "__text";

        private VisualElement _connector;
        private VisualElement _container;
        private VisualElement _led;
        private Label _text;
        
        public string TextValue
        {
            get => _text.text;
            set => _text.text = value;
        }
        
        public SideToggleControl()
        {
            AddToClassList(UssClassName);

            _connector = new VisualElement()
            {
                name = "connector"
            };
            _connector.AddToClassList(UssClassName_Connector);
            hierarchy.Add(_connector);
            
            _container = new VisualElement()
            {
                name = "container"
            };
            _container.AddToClassList(UssClassName_Container);
            hierarchy.Add(_container);
            
            _led = new VisualElement()
            {
                name = "led"
            };
            _led.AddToClassList(UssClassName_Led);
            _container.Add(_led);
            
            _text = new Label()
            {
                name = "led"
            };
            _text.AddToClassList(UssClassName_Text);
            _container.Add(_text);
            
            hierarchy.Add((_container));
        }
        
        public new class UxmlFactory : UxmlFactory<SideToggleControl, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _name = new ()
                { name = "Text", defaultValue = "Toggle" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is SideToggleControl control)
                {
                    control.TextValue = _name.GetValueFromBag(bag, cc);
                }
            }
        }
    }
}