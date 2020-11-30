

using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public class ChromelessButtonViewModel:Screen
    {
        private readonly System.Action _callback;
        private bool _isVisible = true;
        private string _iconName;
        private bool _canClick;
        private bool _isEnabled = true;

        public ChromelessButtonViewModel(System.Action callback)
        {
            _callback = callback;
        }

        public string IconName
        {
            get { return _iconName; }
            set
            {
                if (value == _iconName) return;
                _iconName = value;
                NotifyOfPropertyChange(() => IconName);
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange(() => IsEnabled);
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (value == _isVisible) return;
                _isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        public bool CanClick
        {
            get { return _canClick; }
            set
            {
                if (value == _canClick) return;
                _canClick = value;
                NotifyOfPropertyChange(() => CanClick);
            }
        }

        public virtual void Click()
        {
            _callback();
        }
    }
}