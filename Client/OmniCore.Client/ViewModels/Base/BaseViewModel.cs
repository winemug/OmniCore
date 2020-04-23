using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OmniCore.Client.Annotations;
using OmniCore.Client.Views.Home;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Utilities.Extensions;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public abstract class BaseViewModel : IViewModel, ICompositeDisposableProvider
    {
        protected IClient Client { get; }
        public CompositeDisposable CompositeDisposable { get; }
        
        private readonly ISubject<IView> ViewSubject;
        private readonly ISubject<object> ParameterSubject;
        private readonly ISubject<string> PropertyChangedSubject;

        protected BaseViewModel(IClient client)
        {
            Client = client;
            ViewSubject = new AsyncSubject<IView>();
            ParameterSubject = new Subject<object>();
            PropertyChangedSubject = new Subject<string>();
            CompositeDisposable = new CompositeDisposable();
        }
        
        protected IObservable<object> WhenParameterSet() => ParameterSubject.AsObservable();
        protected IObservable<Page> WhenPageAppears() =>
            ViewSubject.AsObservable()
                .Cast<Page>()
                .Select(p =>
                    Observable.FromEvent<EventHandler, Page>(handler => { p.Appearing += handler; },
                        handler => { p.Appearing -= handler; }))
                .Switch();

        protected IObservable<Page> WhenPageDisappears() =>
            ViewSubject.AsObservable()
                .Cast<Page>()
                .Select(p =>
                    Observable.FromEvent<EventHandler, Page>(handler => { p.Disappearing += handler; },
                        handler => { p.Disappearing -= handler; }))
                .Switch();

        protected IObservable<TProperty> WhenPropertyChanged<T, TProperty>(T source,
            Expression<Func<T, TProperty>> property)
            where T : INotifyPropertyChanged
        {
            
            var propertySelector = property.Compile();
            return PropertyChangedSubject.AsObservable()
                .Where(name => name == GetPropertyInfo(property).Name)
                .Select(_ => propertySelector(source));
        }

        private bool PropertyChangedObservableSuspended = false;
        protected IDisposable SuspendPropertyFeedbackObservable()
        {
            PropertyChangedObservableSuspended = true;
            return Disposable.Create(() =>
            {
                PropertyChangedObservableSuspended = false;
            });
        }
        
        public void Initialize(IView view, bool viaShell, object parameter)
        {
            ParameterSubject.OnNext(parameter);
            ((Page)view).BindingContext = this;
            ViewSubject.OnNext(view);
        }
        
        public void Dispose()
        {
            OnDisposing();
            CompositeDisposable.Dispose();
        }

        protected virtual void OnDisposing()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!PropertyChangedObservableSuspended)
                PropertyChangedSubject.OnNext(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private PropertyInfo GetPropertyInfo<TSource, TValue>(Expression<Func<TSource, TValue>> property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            var body = property.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("Expression is not a property", "property");
            }

            var propertyInfo = body.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("Expression is not a property", "property");
            }

            return propertyInfo;
        }
       
    }
}