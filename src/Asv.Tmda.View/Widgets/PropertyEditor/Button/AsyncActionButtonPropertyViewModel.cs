using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using NLog;
using LogManager = NLog.LogManager;

namespace Asv.Avialab.Core
{
    public class AsyncActionButtonPropertyViewModel : ButtonPropertyViewModel
    {
        private readonly TimeSpan _timeout;
        private CancellationTokenSource _disposeCancel = new CancellationTokenSource();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Func<CancellationToken, Task> _unsafeAction;

        public AsyncActionButtonPropertyViewModel(string displayName, string buttonText, TimeSpan timeout, Func<CancellationToken,Task> unsafeAction) : base(displayName, buttonText)
        {
            _timeout = timeout;
            _unsafeAction = unsafeAction;
            OnClick.Subscribe(Click,_disposeCancel.Token);
        }

        private void Click(Unit unit)
        {
            Task.Factory.StartNew(ClickAsync, TaskCreationOptions.LongRunning);
        }

        private async void ClickAsync()
        {
            var timeoutCancel = new CancellationTokenSource(_timeout);
            var cancel = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancel.Token, _disposeCancel.Token);

            try
            {
                IsEnabled = false;
                await _unsafeAction(cancel.Token);
            }
            catch (Exception e)
            {
                IoC.Get<IWindowManager>().ShowError(string.Format("Error occured to '{0}'", ButtonText), e.Message, e);
                _logger.Error(e, $"Error occured to click '{ButtonText}':{e.Message}");
            }
            finally
            {
                IsEnabled = true;
                timeoutCancel.Dispose();
                cancel.Dispose();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _disposeCancel?.Cancel(false);
            _disposeCancel?.Dispose();
            _disposeCancel = null;
        }
    }
}