namespace Asv.Avialab.Core
{
    public abstract class ScreenPropertyViewModel : ValidatingScreen, IPropertyViewModel
    {
        private string _description;
        private bool _isEnabled = true;
        private bool _isVisible = true;
        private State _state;
        private string _stateMessage;
        private string _iconName;
        private string _group;

        public State State
        {
            get { return _state; }
            set
            {
                if (value == _state) return;
                _state = value;
                NotifyOfPropertyChange(() => State);
            }
        }

        public string Group
        {
            get { return _group; }
            set
            {
                if (value == _group) return;
                _group = value;
                NotifyOfPropertyChange(() => Group);
            }
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

        public string StateMessage
        {
            get { return _stateMessage; }
            set
            {
                if (value == _stateMessage) return;
                _stateMessage = value;
                NotifyOfPropertyChange(() => StateMessage);
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (value == _description) return;
                _description = value;
                NotifyOfPropertyChange(() => Description);
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

        public virtual void Dispose()
        {

        }
    }
}
