using System;
using System.Threading;

namespace Asv.Tmda.Core
{
    public interface IRxValue<out TValue> : IObservable<TValue>
    {
        TValue Value { get; }
    }

    public interface IRxEditableValue<TValue> : IRxValue<TValue>, IObserver<TValue>
    {

    }

}