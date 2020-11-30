using System;
using System.Reactive;
using System.Reactive.Subjects;

namespace Asv.Avialab.Core
{
    public class ButtonPropertyViewModel : ScreenPropertyViewModel
    {
        private readonly Action<ButtonPropertyViewModel> _clickCallback;
        private readonly Subject<Unit> _click = new Subject<Unit>();
        private bool _isIconVisible;
        private string _buttonText;

        public ButtonPropertyViewModel(string displayName, string buttonText, Action<ButtonPropertyViewModel> clickCallback = null)
        {
            _clickCallback = clickCallback;
            DisplayName = displayName;
            ButtonText = buttonText;
            IconName = null;
        }

        public string Units => null;

        public string ButtonText
        {
            get { return _buttonText; }
            set
            {
                if (value == _buttonText) return;
                _buttonText = value;
                NotifyOfPropertyChange(() => ButtonText);
            }
        }

        public IObservable<Unit> OnClick => _click;

        public bool IsIconVisible
        {
            get { return _isIconVisible; }
            set
            {
                if (value == _isIconVisible) return;
                _isIconVisible = value;
                NotifyOfPropertyChange(() => IsIconVisible);
            }
        }

        public void Click()
        {
            _click.OnNext(Unit.Default);
            _clickCallback?.Invoke(this);
        }

        public override void Dispose()
        {
            base.Dispose();
            _click.OnCompleted();
            _click.Dispose();
        }
    }
}
