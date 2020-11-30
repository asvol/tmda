using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Asv.Avialab.Core
{
    public class DigitPropertyViewModel<T>:TextPropertyViewModel<T>
    {
        private T _min;
        private T _max;
        private string _stringFormat;
        private T _interval;

        public void ClearFocus()
        {
            if (!IsEdit) return;
            var view = GetView() as DigitPropertyView;
            view?.text?.Focus();
        }



        public DigitPropertyViewModel(string displayName, T defaultValue, Action<T> valueChanged = null) : base(displayName, defaultValue, valueChanged)
        {

        }

        public T Interval
        {
            get { return _interval; }
            set
            {
                if (Equals(value, _interval)) return;
                _interval = value;
                NotifyOfPropertyChange(() => Interval);
            }
        }

        public string StringFormat
        {
            get { return _stringFormat; }
            set
            {
                if (value == _stringFormat) return;
                _stringFormat = value;
                NotifyOfPropertyChange(() => StringFormat);
            }
        }

        public T Min
        {
            get { return _min; }
            set
            {
                if (Equals(value, _min)) return;
                _min = value;
                NotifyOfPropertyChange(() => Min);
            }
        }

        public T Max
        {
            get { return _max; }
            set
            {
                if (Equals(value, _max)) return;
                _max = value;
                NotifyOfPropertyChange(() => Max);
            }
        }
    }
}
