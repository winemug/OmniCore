using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Extensions
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

        public static IObservable<T> WrapAndConvert<T,U>(this IObservable<U> observable, Func<U,T> typeConversion)
        {
            return Observable.Create<T>((observer) =>
                {
                    var subscription = observable.Subscribe((u) =>
                    {
                        observer.OnNext(typeConversion.Invoke(u));
                    });

                    return Disposable.Create( () => { subscription.Dispose(); });
                });
        }
    }
}
