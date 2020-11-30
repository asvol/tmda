using System;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace Asv.Avialab.Core
{
    public class SelectorScreenSerialPort: SelectorPropertyViewModel<string>
    {
        private readonly bool _selectFirstByDefault;
        private readonly Action<SelectorItem<string>> _onChanged;
        private readonly CallbackSampler<string[]> _serialPortUpdater;
        private readonly IDisposable _subscribe;

        public SelectorScreenSerialPort(string displayName, TimeSpan updatePeriod, bool selectFirstByDefault = true, Action<SelectorItem<string>> onChanged = null) : base(displayName, onChanged)
        {
            _selectFirstByDefault = selectFirstByDefault;
            _onChanged = onChanged;
            _serialPortUpdater = new CallbackSampler<string[]>(updatePeriod, updatePeriod, () => Task.Factory.StartNew(SerialPort.GetPortNames));
            _subscribe = _serialPortUpdater.Subscribe(UpdateSerialPorts);
            _serialPortUpdater.IsEnabled = false;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            _serialPortUpdater.IsEnabled = true;
            _serialPortUpdater.ManualUpdate();
        }

        private void UpdateSerialPorts(Sample<string[]> serialPorts)
        {
            if (serialPorts.IsError)
            {
                this.
                State = State.Error;
                StateMessage = serialPorts.Error.Message;
            }
            else
            {
                State = State.None;
                var exist = Items.Select(_ => _.Value).ToArray();

                var toDelete = exist.Except(serialPorts.Value).ToArray();
                var toAdd = serialPorts.Value.Except(exist).ToArray();

                foreach (var item in toDelete)
                {
                    DeactivateItem(Items.First(_ => _.Value == item), true);
                }

                foreach (var item in toAdd)
                {
                    ActivateItem(new SelectorItem<string>(item, item));
                }
            }
        }

        public override void ActivateItem(SelectorItem<string> item)
        {
            base.ActivateItem(item);
            if (SelectedItem == null && _selectFirstByDefault) SelectedItem = item;
        }


        protected override void OnDeactivate(bool close)
        {
            _serialPortUpdater.IsEnabled = false;
            if (close)
            {
                Dispose();
            }
            base.OnDeactivate(close);
        }

        public override void Dispose()
        {
            base.Dispose();
            _subscribe?.Dispose();
            _serialPortUpdater.Dispose();
        }
    }
}
