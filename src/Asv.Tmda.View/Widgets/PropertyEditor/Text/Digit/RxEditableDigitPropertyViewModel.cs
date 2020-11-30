using System;
using System.Reactive.Linq;
using Asv.Tmda.Core;

namespace Asv.Avialab.Core
{
    public class RxEditableDigitPropertyViewModel<T> : DigitPropertyViewModel<T>
    {
        private bool _internalChange2;
        private readonly IDisposable _subscribe1;
        private readonly IDisposable _subscribe2;
        private bool _internalChange1;

        public RxEditableDigitPropertyViewModel(string displayName, IRxEditableValue<T> value) : base(displayName, value.Value)
        {
            _subscribe1 = OnValueChanged.Where(_=>_internalChange2 == false).Subscribe(_=>
            {
                _internalChange1 = true;
                value.OnNext(_);
                _internalChange1 = false;
            });
            _subscribe2 = value.Where(_=>_internalChange1 == false).Subscribe(_ =>
            {
                _internalChange2 = true;
                Value = _;
                _internalChange2 = false;
            });
        }

        public override void Dispose()
        {
            _subscribe1.Dispose();
            _subscribe2.Dispose();
            base.Dispose();
        }
    }
}