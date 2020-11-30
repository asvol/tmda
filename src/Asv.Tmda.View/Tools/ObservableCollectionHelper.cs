using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Caliburn.Micro;
using Action = System.Action;

namespace Asv.Avialab.Core
{
    public static class ObservableCollectionHelper
    {
        public static void SubscribeCollectionChange<T>(this INotifyCollectionChanged src, Action<T> onAdd, Action<T> onRemove,
            CancellationToken cancel)
        {
            var collChange = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                _ => src.CollectionChanged += _, _ => src.CollectionChanged -= _);

            collChange.Where(_ => _.EventArgs.Action == NotifyCollectionChangedAction.Add)
                .SelectMany(_ => _.EventArgs.NewItems.Cast<T>()).Subscribe(onAdd, cancel);

            collChange.Where(_ => _.EventArgs.Action == NotifyCollectionChangedAction.Remove)
                .SelectMany(_ => _.EventArgs.OldItems.Cast<T>()).Subscribe(onRemove, cancel);

            collChange.Where(_ => _.EventArgs.Action == NotifyCollectionChangedAction.Reset)
                .Subscribe(_ =>
                {
                    throw new NotSupportedException();
                }, cancel);

            if (src is IEnumerable enumerable)
            {
                foreach (var item in enumerable.Cast<T>())
                {
                    onAdd(item);
                }
            }
            
        }

        public static void SubscribeCollectionChange<T>(this INotifyCollectionChanged src, Action<T> onAdd, Action<T> onRemove, Action reset,
            CancellationToken cancel)
        {
            var collChange = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                _ => src.CollectionChanged += _, _ => src.CollectionChanged -= _);

            collChange.Where(_ => _.EventArgs.Action == NotifyCollectionChangedAction.Add)
                .SelectMany(_ => _.EventArgs.NewItems.Cast<T>()).Subscribe(onAdd, cancel);

            collChange.Where(_ => _.EventArgs.Action == NotifyCollectionChangedAction.Remove)
                .SelectMany(_ => _.EventArgs.OldItems.Cast<T>()).Subscribe(onRemove, cancel);

            collChange.Where(_ => _.EventArgs.Action == NotifyCollectionChangedAction.Reset)
                .Subscribe(_ => { reset(); }, cancel);

            if (src is IEnumerable enumerable)
            {
                foreach (var item in enumerable.Cast<T>())
                {
                    onAdd(item);
                }
            }
        }


        public static void SubscribeCollectionChange<T>(this IObservableCollection<T> src, Action<T> onAdd, Action<T> onRemove,
            CancellationToken cancel)
        {
            var collChange = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                _ => src.CollectionChanged += _, _ => src.CollectionChanged -= _);

            collChange.Where(_ => _.EventArgs.Action == NotifyCollectionChangedAction.Add)
                .SelectMany(_ => _.EventArgs.NewItems.Cast<T>()).Subscribe(onAdd, cancel);

            collChange.Where(_ => _.EventArgs.Action == NotifyCollectionChangedAction.Remove)
                .SelectMany(_ => _.EventArgs.OldItems.Cast<T>()).Subscribe(onRemove, cancel);

            collChange.Where(_ => _.EventArgs.Action == NotifyCollectionChangedAction.Reset)
                .Subscribe(_ =>
                {
                    throw new NotSupportedException();
                }, cancel);

            foreach (var item in src)
            {
                onAdd(item);
            }
        }

        public static void SubscribeCollectionChange<T>(this IObservableCollection<T> src, Action<T> onAdd, Action<T> onRemove, Action reset,
            CancellationToken cancel)
        {
            var collChange = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                _ => src.CollectionChanged += _, _ => src.CollectionChanged -= _);

            collChange.Where(_ => _.EventArgs.Action == NotifyCollectionChangedAction.Add)
                .SelectMany(_ => _.EventArgs.NewItems.Cast<T>()).Subscribe(onAdd, cancel);

            collChange.Where(_ => _.EventArgs.Action == NotifyCollectionChangedAction.Remove)
                .SelectMany(_ => _.EventArgs.OldItems.Cast<T>()).Subscribe(onRemove, cancel);

            collChange.Where(_ => _.EventArgs.Action == NotifyCollectionChangedAction.Reset)
                .Subscribe(_ => { reset(); }, cancel);

            foreach (var item in src)
            {
                onAdd(item);
            }
        }
    }
}
