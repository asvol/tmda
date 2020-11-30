using System;

namespace Asv.Avialab.Core
{
    public class MaskedTextPropertyViewModel: TextPropertyViewModel<string>
    {
        private string _inputMask;
        private string _promtChar;

        public MaskedTextPropertyViewModel(string displayName, string defaultValue, Action<string> valueChanged = null):base(displayName, defaultValue, valueChanged)
        {
        }

        public string PromtChar
        {
            get { return _promtChar; }
            set
            {
                if (value == _promtChar) return;
                _promtChar = value;
                NotifyOfPropertyChange(() => PromtChar);
            }
        }

        public string InputMask
        {
            get { return _inputMask; }
            set
            {
                if (value == _inputMask) return;
                _inputMask = value;
                NotifyOfPropertyChange(() => InputMask);
            }
        }
    }
}
