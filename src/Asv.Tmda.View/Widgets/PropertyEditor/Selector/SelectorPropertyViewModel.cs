using System;
using System.Reactive.Subjects;
using System.Windows.Media;
using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public class SelectorItem<TValue> : PropertyChangedBase, IHaveDisplayName
    {
        private string _displayName;
        private string _iconName;
        private TValue _value;
        private Brush _iconBrush;

        public SelectorItem(string displayName, TValue value, string iconName = null, Brush iconBrush = null)
        {
            Value = value;
            DisplayName = displayName;
            IconName = iconName;
            IconBrush = iconBrush;
        }

        public Brush IconBrush
        {
            get { return _iconBrush; }
            set
            {
                if (Equals(value, _iconBrush)) return;
                _iconBrush = value;
                NotifyOfPropertyChange(() => IconBrush);
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

        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (value == _displayName) return;
                _displayName = value;
                NotifyOfPropertyChange(() => DisplayName);
            }
        }

        public TValue Value
        {
            get { return _value; }
            set
            {
                if (Equals(value, _value)) return;
                _value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }
    }

    public class SelectorPropertyViewModel<TValue>:ConductorPropertyViewModel<SelectorItem<TValue>>
    {
        private Action<SelectorItem<TValue>> _onChanged;
        private readonly Subject<SelectorItem<TValue>> _selectedItemChanged = new Subject<SelectorItem<TValue>>();
        private SelectorItem<TValue> _selectedItem;
        private bool _disableChanges = false;
        private string _units;

        public SelectorPropertyViewModel(string displayName, Action<SelectorItem<TValue>> onChanged = null)
        {
            _onChanged = onChanged;
            // ReSharper disable once VirtualMemberCallInConstructor
            DisplayName = displayName;
        }

        public string Units
        {
            get => _units;
            set
            {
                if (value == _units) return;
                _units = value;
                NotifyOfPropertyChange(() => Units);
            }
        }

        public IObservable<SelectorItem<TValue>> OnSelectionChanged => _selectedItemChanged;

        public SelectorItem<TValue> SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (Equals(value, _selectedItem)) return;
                if (_disableChanges) return;
                _selectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
                _selectedItemChanged.OnNext(value);
                _onChanged?.Invoke(value);
                if (value == null) return;
                ActivateItem(value);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                _disableChanges = true;
            }
            base.OnDeactivate(close);
        }

        public override void Dispose()
        {
            _onChanged = null;
            _selectedItemChanged?.OnCompleted();
            _selectedItemChanged?.Dispose();
            base.Dispose();
            
        }
    }
}
