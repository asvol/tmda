using System;
using System.Reactive.Subjects;

namespace Asv.Avialab.Core
{
    public class TextPropertyViewModel<T>:ScreenPropertyViewModel, IEditable
    {
        private T _value;
        private Action<T> _valueChanged;
        private readonly Subject<T> _valueChangedSubject = new Subject<T>();
        private string _units;
        private bool _isReadOnly;
        private bool _isEdit;
        private readonly Subject<bool> _editSubject = new Subject<bool>();

        public TextPropertyViewModel(string displayName, T defaultValue, Action<T> valueChanged = null)
        {
            DisplayName = displayName;
            Value = defaultValue;
            _valueChanged = valueChanged;
        }

        public IObservable<T> OnValueChanged => _valueChangedSubject;

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                if (value == _isReadOnly) return;
                _isReadOnly = value;
                NotifyOfPropertyChange(() => IsReadOnly);
            }
        }

        public virtual bool IsEdit
        {
            get { return _isEdit; }
            set
            {
                if (value == _isEdit) return;
                _isEdit = value;
                _editSubject.OnNext(value);
                NotifyOfPropertyChange(() => IsEdit);
            }
        }

        public IObservable<bool> OnEditChanged => _editSubject;

        public string Units
        {
            get { return _units; }
            set
            {
                if (value == _units) return;
                _units = value;
                NotifyOfPropertyChange(() => Units);
            }
        }

        public T Value
        {
            get { return _value; }
            set
            {
                if (Equals(value, _value)) return;
                _value = value;
                NotifyOfPropertyChange(() => Value);
                if (!HasError)
                {
                    _valueChangedSubject.OnNext(value);
                    _valueChanged?.Invoke(value);
                }
            }
        }

    
        public override void Dispose()
        {
            base.Dispose();
            _valueChanged = null;
            _valueChangedSubject.OnCompleted();
            _valueChangedSubject.Dispose();
            _editSubject.OnCompleted();
            _editSubject.Dispose();
        }

        public void AddValidationRule(Func<T,bool> condition,string message)
        {
            AddValidationRule(() => Value).Condition(()=>condition(Value)).Message(message);
        }


    }
}
