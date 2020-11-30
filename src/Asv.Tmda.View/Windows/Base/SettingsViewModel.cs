namespace Asv.Avialab.Core
{
    public class SettingsViewModel:TabViewModel
    {
        private bool _isOpened;

        public bool IsOpened
        {
            get { return _isOpened; }
            set
            {
                if (value == _isOpened) return;
                _isOpened = value;
                UpdateOpenStatus();
                NotifyOfPropertyChange(() => IsOpened);
            }
        }

        private void UpdateOpenStatus()
        {
            if (ActiveItem == null) return;

            if (IsOpened)
            {
                ActiveItem.Activate();
            }
            else
            {
                ActiveItem.Deactivate(false);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
        }

        public void OpenSettings()
        {
            IsOpened = !IsOpened;
        }
    }
}
