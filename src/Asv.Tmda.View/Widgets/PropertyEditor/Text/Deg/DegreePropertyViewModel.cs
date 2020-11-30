using System;
using System.Reactive.Subjects;

namespace Asv.Avialab.Core
{
    public class DegreePropertyViewModel : ScreenPropertyViewModel, IEditable
    {
        private bool _isEdit;
        private string _textValue;
        private bool _isReadOnly;
        private string _units;
        private decimal _value;
        private Action<decimal> _valueChanged;
        private readonly Subject<decimal> _valueChangedSubject = new Subject<decimal>();

        public DegreePropertyViewModel(string displayName, decimal defaultValue, Action<decimal> valueChanged = null)
        {
            Value = defaultValue;
            _valueChanged = valueChanged;
        }

        public IObservable<decimal> OnValueChanged => _valueChangedSubject;

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

        public decimal Value
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
        }

        public void AddValidationRule(Func<decimal, bool> condition, string message)
        {
            AddValidationRule(() => Value).Condition(() => condition(Value)).Message(message);
        }

        public string TextValue
        {
            get => _textValue;
            set
            {
                if (value == _textValue) return;
                _textValue = value;
                NotifyOfPropertyChange(() => TextValue);
            }
        }

        public bool IsEdit
        {
            get => _isEdit;
            set
            {
                if (value == _isEdit) return;
                _isEdit = value;
                NotifyOfPropertyChange(() => IsEdit);
            }
        }
    }
}
