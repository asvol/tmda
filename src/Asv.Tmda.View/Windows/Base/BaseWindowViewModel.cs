using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public class BaseWindowViewModel<T>:Screen
    {
        private T _content;
        private WindowHeaderViewModel _windowHeader;
        private SettingsViewModel _settings;
        private Conductor<IScreen>.Collection.AllActive _statusBar;

        public BaseWindowViewModel()
        {
            StatusBar = new Conductor<IScreen>.Collection.AllActive();
            WindowHeader = new WindowHeaderViewModel();
            Settings = new SettingsViewModel();
        }

        public T Content
        {
            get { return _content; }
            set
            {
                if (Equals(value, _content)) return;
                _content = value;

                var child = value as IChild;
                if (child != null)
                {
                    child.Parent = this;
                }

                var deactivate = value as IDeactivate;
                deactivate?.DeactivateWith(this);

                var activate = value as IActivate;
                activate?.ActivateWith(this);
                NotifyOfPropertyChange(() => Content);
            }
        }

        public SettingsViewModel Settings
        {
            get { return _settings; }
            set
            {
                if (Equals(value, _settings)) return;
                _settings = value;
                value.Parent = this;
                _settings.DeactivateWith(this);
                _settings.ActivateWith(this);
                NotifyOfPropertyChange(() => Settings);
            }
        }

        public WindowHeaderViewModel WindowHeader
        {
            get { return _windowHeader; }
            set
            {
                if (Equals(value, _windowHeader)) return;
                _windowHeader = value;
                value.Parent = this;
                _windowHeader.DeactivateWith(this);
                _windowHeader.ActivateWith(this);
                NotifyOfPropertyChange(() => WindowHeader);
            }
        }

        public Conductor<IScreen>.Collection.AllActive StatusBar
        {
            get { return _statusBar; }
            set
            {
                if (Equals(value, _statusBar)) return;
                _statusBar = value;
                value.Parent = this;
                _statusBar.DeactivateWith(this);
                _statusBar.ActivateWith(this);
                NotifyOfPropertyChange(() => StatusBar);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
        }
    }
}