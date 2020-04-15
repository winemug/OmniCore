using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using OmniCore.Client.Views.Home;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public abstract class BaseViewModel : IViewModel
    {
        protected ICoreClient Client { get; }
        private ISubject<IView> ViewSubject;
        private ISubject<object> ParameterSubject;
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


        protected BaseViewModel(ICoreClient client)
        {
            Client = client;
            ViewSubject = new AsyncSubject<IView>();
            ParameterSubject = new Subject<object>();
        }

#pragma warning disable CS0067 // The event 'BaseViewModel.PropertyChanged' is never used
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067 // The event 'BaseViewModel.PropertyChanged' is never used

        public void Initialize(IView view, bool viaShell, object parameter)
        {
            ParameterSubject.OnNext(parameter);
            ((Page)view).BindingContext = this;
            ViewSubject.OnNext(view);
        }
        
        public virtual void Dispose()
        {
        }
    }
}