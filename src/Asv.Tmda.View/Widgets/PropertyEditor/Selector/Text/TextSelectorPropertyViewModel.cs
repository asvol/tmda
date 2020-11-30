using System;

namespace Asv.Avialab.Core
{
    public class TextSelectorPropertyViewModel: ConductorPropertyViewModel<string>
    {
        private string _value;
        private string _selectedItem;

        public TextSelectorPropertyViewModel(string displayName, string defaultValue)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            DisplayName = displayName;
            _value = defaultValue;
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                BeginDeactivation = true;
            }
            base.OnDeactivate(close);
        }

        public bool BeginDeactivation { get; set; }

        public string SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                // because after deactivation selected item field will be item == null
                if (BeginDeactivation) return;

                if (value == _selectedItem) return;
                _selectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
                Value = _selectedItem;
            }
        }


        public void AddValidationRule(Func<string, bool> condition, string message)
        {
            AddValidationRule(() => Value).Condition(() => condition(Value)).Message(message);
        }


        public string Value
        {
            get { return _value; }
            set
            {
                // because after deactivation selected item field will be item == null
                if (BeginDeactivation) return;

                if (value == _value) return;
                _value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }
    }
}
