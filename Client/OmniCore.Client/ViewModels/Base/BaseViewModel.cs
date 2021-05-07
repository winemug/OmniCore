using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
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
        
        private readonly ISubject<Page> AppearingSubject;
        private readonly ISubject<Page> DisappearingSubject;

        private readonly ISubject<object> ParameterSubject;
        private readonly ISubject<string> PropertyChangedSubject;

        protected BaseViewModel(IClient client)
        {
            Client = client;
            AppearingSubject = new Subject<Page>();
            DisappearingSubject = new Subject<Page>();
            ParameterSubject = new Subject<object>();
            PropertyChangedSubject = new Subject<string>();
            CompositeDisposable = new CompositeDisposable();
        }
        
        protected IObservable<object> WhenParameterSet() => ParameterSubject.AsObservable();

        protected IObservable<Page> WhenPageAppears() => AppearingSubject.AsObservable();

        protected IObservable<Page> WhenPageDisappears() => DisappearingSubject.AsObservable();

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

        private Page LastPage = null;
        public void Initialize(IView view, bool viaShell, object parameter)
        {
            ParameterSubject.OnNext(parameter);

            var page = (Page) view;
            page.BindingContext = this;

            if (LastPage != null && page != LastPage)
            {
                LastPage.Appearing -= PageAppearing;
                LastPage.Disappearing -= PageDisappearing;
            }

            page.Appearing += PageAppearing;
            page.Disappearing += PageDisappearing;
            LastPage = page;
        }

        private void PageAppearing(object sender, EventArgs e)
        {
            AppearingSubject.OnNext((Page)sender);
        }

        private void PageDisappearing(object sender, EventArgs e)
        {
            DisappearingSubject.OnNext((Page)sender);
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