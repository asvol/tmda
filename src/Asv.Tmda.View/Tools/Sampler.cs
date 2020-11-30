using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Asv.Avialab.Core
{
    public class Sample<TValue>
    {
        public TValue Value { get; private set; }
        public Exception Error { get; private set; }
        public bool IsError { get; private set; }

        public Sample(TValue value)
        {
            Value = value;
            IsError = false;
        }

        public Sample(Exception error)
        {
            Error = error;
            IsError = true;
        }
    }

    public abstract class AbstractSampler<T> : IDisposable, IObservable<Sample<T>>
    {
        private readonly IDisposable _subscribe;
        private readonly Subject<Sample<T>> _publisher = new Subject<Sample<T>>();
        private int _requestNotComplete;

        protected AbstractSampler(TimeSpan dueTime, TimeSpan period, CancellationToken cancellationToken)
        {
            _subscribe = Observable.Timer(dueTime, period).Where(_ => IsEnabled).Subscribe(InternalUpdateTick);
            cancellationToken.Register(Dispose);
        }

        protected AbstractSampler(TimeSpan dueTime, TimeSpan period)
        {
            _subscribe = Observable.Timer(dueTime, period).Where(_=>IsEnabled).Subscribe(InternalUpdateTick);
        }

        public void ManualUpdate()
        {
            InternalUpdateTick(-1);
        }

        public bool IsEnabled { get; set; } = true;

        private async void InternalUpdateTick(long l)
        {
            if (_publisher.IsDisposed) return;
            if (Interlocked.CompareExchange(ref _requestNotComplete, 1, 0) != 0) return;
            try
            {
                var value = await GetSample();
                if (!_publisher.IsDisposed)
                    _publisher.OnNext(new Sample<T>(value));
            }
            catch (Exception ex)
            {
                if (!_publisher.IsDisposed)
                {
                    if (!_publisher.IsDisposed) _publisher.OnNext(new Sample<T>(ex));
                }
                
            }
            finally
            {
                Interlocked.Exchange(ref _requestNotComplete, 0);
            }
        }

        protected abstract Task<T> GetSample();

        public void Dispose()
        {
            _subscribe.Dispose();
            _publisher.OnCompleted();
            _publisher.Dispose();
        }

        public IDisposable Subscribe(IObserver<Sample<T>> observer)
        {
            return _publisher.Subscribe(observer);
        }


    }

    public class CallbackSampler<T> : AbstractSampler<T>
    {
        private readonly Func<Task<T>> _getSampleCallback;

        public CallbackSampler(TimeSpan dueTime, TimeSpan period, Func<Task<T>> getSampleCallback,
            CancellationToken cancellationToken) : base(dueTime, period, cancellationToken)
        {
            if (getSampleCallback == null) throw new ArgumentNullException(nameof(getSampleCallback));
            _getSampleCallback = getSampleCallback;
        }

        public CallbackSampler(TimeSpan dueTime, TimeSpan period, Func<Task<T>> getSampleCallback) : base(dueTime, period)
        {
            if (getSampleCallback == null) throw new ArgumentNullException(nameof(getSampleCallback));
            _getSampleCallback = getSampleCallback;
        }

        protected override Task<T> GetSample()
        {
            return _getSampleCallback();
        }
    }
}