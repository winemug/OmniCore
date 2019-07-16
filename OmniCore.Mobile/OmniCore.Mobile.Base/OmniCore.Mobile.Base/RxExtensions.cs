using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace OmniCore.Mobile.Base
{
    public static class RxExtensions
    {
        public static IObservable<PropertyChangedEventArgs> OnPropertyChanges<T>(this T source)
            where T : INotifyPropertyChanged
        {
            return Observable.Create<PropertyChangedEventArgs>(observer =>
            {
                PropertyChangedEventHandler handler = (s, e) => observer.OnNext(e);
                source.PropertyChanged += handler;
                return Disposable.Create(() => source.PropertyChanged -= handler);
            });
        }
    }
}
