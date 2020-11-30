using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Asv.Tmda.Core
{
    public static class ObservableExtensions
    {
        /// <summary>
        /// Скользящее окно
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="buffering"></param>
        /// <returns></returns>
        public static IObservable<T[]> RollingBuffer<T>(
            this IObservable<T> @this,
            TimeSpan buffering)
        {
            return Observable.Create<T[]>(o =>
            {
                var list = new LinkedList<Timestamped<T>>();
                return @this.Timestamp().Subscribe(tx =>
                {
                    list.AddLast(tx);
                    while (list.First.Value.Timestamp < DateTime.Now.Subtract(buffering))
                    {
                        list.RemoveFirst();
                    }
                    o.OnNext(list.Select(tx2 => tx2.Value).ToArray());
                }, o.OnError, o.OnCompleted);
            });
        }
    }
}
