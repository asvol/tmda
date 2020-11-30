using System;

namespace Asv.Avialab.Core
{
    public class ToggleSwitchPropertyViewModel:ScreenPropertyViewModel
    {
        private readonly Action<bool> _valueChangedCallback;
        private string _trueLabel;
        private string _falseLabel;
        private bool _value;

        public ToggleSwitchPropertyViewModel(string displayName, string trueLabel = null, string falseLabel = null, Action<bool> valueChangedCallback = null, bool initValue = false)
        {
            _valueChangedCallback = valueChangedCallback;
            TrueLabel = trueLabel;
            FalseLabel = falseLabel;
            // ReSharper disable once VirtualMemberCallInConstructor
            DisplayName = displayName;
            _value = initValue;
        }

        public string Units => null;

        public string TrueLabel
        {
            get { return _trueLabel; }
            set
            {
                if (value == _trueLabel) return;
                _trueLabel = value;
                NotifyOfPropertyChange(() => TrueLabel);
            }
        }

        public string FalseLabel
        {
            get { return _falseLabel; }
            set
            {
                if (value == _falseLabel) return;
                _falseLabel = value;
                NotifyOfPropertyChange(() => FalseLabel);
            }
        }

        public bool Value
        {
            get { return _value; }
            set
            {
                if (value == _value) return;
                _value = value;
                NotifyOfPropertyChange(() => Value);
                _valueChangedCallback?.Invoke(value);
            }
        }
    }
}
