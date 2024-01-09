using BepInEx.Logging;
using KSP.Audio;
using UnityEngine.UIElements;
using Logger = BepInEx.Logging.Logger;
using Settings = OrbitalSurvey.Utilities.Settings;

namespace OrbitalSurvey.UI.Controls
{
    public class SideToggleControl : Button
    {
        public const string UssClassName = "side-toggle";
        public const string UssClassName_Connector = UssClassName + "__connector";
        public const string UssClassName_Container = UssClassName + "__container";
        public const string UssClassName_Led = UssClassName + "__led";
        public const string UssClassName_Text = UssClassName + "__text";
        
        public const string UssHover = UssClassName_Container + "--hover";
        public const string UssActive = UssClassName_Container + "--active";
        public const string UssLedChecked = UssClassName_Led + "--checked";
        public const string UssLedUnchecked = UssClassName_Led + "--unchecked";
        public const string UssLedDisabled = UssClassName_Led + "--disabled";
        public const string UssTextDisabled = UssClassName_Text + "--disabled";

        public bool IsToggled { get; private set; }
        public bool IsEnabled { get; private set; }

        private VisualElement _connector;
        private VisualElement _container;
        private VisualElement _led;
        private Label _text;
        
        private readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.SideToggleControl");

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
                name = "text"
            };
            _text.AddToClassList(UssClassName_Text);
            _container.Add(_text);

            hierarchy.Add((_container));

            RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
            RegisterCallback<PointerDownEvent>(OnPointerDownEvent,TrickleDown.TrickleDown);
            RegisterCallback<ClickEvent>(OnClickEvent);
            RegisterCallback<PointerEnterEvent>(OnPointerEnterEvent);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent);

            // set as toggled off when first built
            TriggerToggle(false, false);
            
            // set as disabled when first built
            SetEnabled(false);

            //RegisterCallback<WheelEvent>((evt) => _LOGGER.LogDebug($"(inside) WheelEvent {TextValue}"));
        }

        private void OnPointerEnterEvent(PointerEnterEvent _)
        {
            if (!IsEnabled)
                return;
            
            _container.AddToClassList(UssHover);
        }
        
        private void OnPointerLeaveEvent(PointerLeaveEvent _)
        {
            if (!IsEnabled)
                return;
            
            _container.RemoveFromClassList(UssHover);
            _container.RemoveFromClassList(UssActive);
        }
        
        private void OnPointerDownEvent(PointerDownEvent _)
        {
            if (!IsEnabled)
                return;
            
            _container.RemoveFromClassList(UssHover);
            _container.AddToClassList(UssActive);
        }
        
        private void OnPointerUpEvent(PointerUpEvent _)
        {
            if (!IsEnabled)
                return;
            
            _container.RemoveFromClassList(UssActive);
        }

        private void OnClickEvent(ClickEvent _)
        {
            if (!IsEnabled)
                return;
            
            TriggerToggle(!IsToggled);
        }

        public void TriggerToggle(bool state, bool playSound = true)
        {
            _LOGGER.LogInfo($"OnClickEvent for '{TextValue}'. Control set to '{state}'.");
            IsToggled = state;
            if (IsToggled)
            {
                _led.RemoveFromClassList(UssLedUnchecked);
                _led.AddToClassList(UssLedChecked);
                if (playSound && Settings.PlayUiSounds.Value) { KSPAudioEventManager.onPartManagerVisibilityChanged(true); }
            }
            else
            {
                _led.RemoveFromClassList(UssLedChecked);
                _led.AddToClassList(UssLedUnchecked);
                if (playSound && Settings.PlayUiSounds.Value) { KSPAudioEventManager.onPartManagerVisibilityChanged(false); }
            }
        }
        
        public new void SetEnabled(bool state)
        {
            _LOGGER.LogInfo($"Setting control '{TextValue}' enabled property to 'state'.");
            TriggerToggle(false, false);
            
            IsEnabled = state;
            if (IsEnabled)
            {
                _led.RemoveFromClassList(UssLedDisabled);
                _text.RemoveFromClassList(UssTextDisabled);
                _led.AddToClassList(UssLedUnchecked);
            }
            else
            {
                _led.RemoveFromClassList(UssLedUnchecked);
                _led.RemoveFromClassList(UssLedChecked);
                _led.AddToClassList(UssLedDisabled);
                _text.AddToClassList(UssTextDisabled);                
            }
        }
        
        public new class UxmlFactory : UxmlFactory<SideToggleControl, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _name = new()
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