using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Windows.Input;
using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public class WindowHeaderViewModel:Screen,IDisposable
    {
        private string _iconName;
        private readonly Subject<Unit> _onIconClickSubject = new Subject<Unit>();

        public WindowHeaderViewModel()
        {
            Main.Parent = this;
            Main.ActivateWith(this);
            Main.DeactivateWith(this);

            Right.Parent = this;
            Right.ActivateWith(this);
            Right.DeactivateWith(this);

            Tools.Parent = this;
            Tools.ActivateWith(this);
            Tools.DeactivateWith(this);
        }

        public void IconClick()
        {
            _onIconClickSubject.OnNext(Unit.Default);
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                Dispose();
            }
            base.OnDeactivate(close);
        }

        public IObservable<Unit> OnIconClick => _onIconClickSubject;

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

        public Conductor<IScreen>.Collection.AllActive Tools { get; } = new Conductor<IScreen>.Collection.AllActive();
        public Conductor<IMenuItemViewModel>.Collection.AllActive Right { get; } = new Conductor<IMenuItemViewModel>.Collection.AllActive();
        public Conductor<IMenuItemViewModel>.Collection.AllActive Main { get; } = new Conductor<IMenuItemViewModel>.Collection.AllActive();

        public void Dispose()
        {
            _onIconClickSubject?.Dispose();
        }
    }
}