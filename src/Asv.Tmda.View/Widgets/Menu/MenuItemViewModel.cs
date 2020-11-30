using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public class MenuItemViewModel : Screen, IMenuItemViewModel
    {
        private bool _isVisible = true;
        private bool _isCheckable;
        private bool _isChecked;
        private bool _staysOpenOnClick = false;
        private string _iconName;
        protected readonly CancellationTokenSource Cancel = new CancellationTokenSource();
        private int _isDisposed;

        public MenuItemViewModel(string displayName = null, IEnumerable<MenuItemViewModel> subItems = null, string iconName = null) : this(subItems)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            DisplayName = displayName;
            IconName = iconName;
        }

        public MenuItemViewModel(IEnumerable<MenuItemViewModel> subItems)
        {
            Items.SubscribeCollectionChange(_ => _.Parent = this, _ => _.Parent = null, Cancel.Token);
            if (subItems != null)
            {
                foreach (var item in subItems)
                {
                    Items.Add(item);
                }
                
            }
        }

        public MenuItemViewModel()
        {

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

        public int Order { get; set; } = 0;

        public IObservableCollection<IMenuItemViewModel> Items { get; } = new BindableCollection<IMenuItemViewModel>();

        public bool StaysOpenOnClick
        {
            get { return _staysOpenOnClick; }
            set
            {
                if (value == _staysOpenOnClick) return;
                _staysOpenOnClick = value;
                NotifyOfPropertyChange(() => StaysOpenOnClick);
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

        public bool IsCheckable
        {
            get { return _isCheckable; }
            set
            {
                if (value == _isCheckable) return;
                _isCheckable = value;
                NotifyOfPropertyChange(() => IsCheckable);
            }
        }

        public virtual bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                NotifyOfPropertyChange(() => IsChecked);
            }
        }

        public object Tag { get; set; }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                Dispose();
            }
            base.OnDeactivate(close);
        }

        public virtual void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            Cancel?.Cancel(false);
            Cancel?.Dispose();
        }

        public virtual void Execute()
        {

        }
    }
}
