using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Client.Extensions
{
    public static class RxExtensions
    {
        public static IObservable<string> OnPropertyChanges<T>(this T source)
            where T : INotifyPropertyChanged
        {
            return Observable.Create<string>(observer =>
            {
                PropertyChangedEventHandler handler = (s, e) => observer.OnNext(e.PropertyName);
                source.PropertyChanged += handler;
                return Disposable.Create(() => source.PropertyChanged -= handler);
            });
        }

        public static async Task<T> RunAsyncWithTimeoutAndCancellation<T>(this IObservable<T> observable, TimeSpan timeout, CancellationToken cancellationToken)
        {
            try
            {
                var observableTask = observable.ToTask();
                var resultTask = await Task.WhenAny(observableTask, Task.Delay(timeout, cancellationToken));
                if (resultTask == observableTask)
                    return await observableTask;
            }
            catch (TaskCanceledException)
            {
            }
            return default(T);
        }
    }
}
