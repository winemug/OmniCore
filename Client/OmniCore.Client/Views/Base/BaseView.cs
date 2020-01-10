using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using Xamarin.Forms;

namespace OmniCore.Client.Views.Base
{
    public abstract class BaseView<TModel> : ContentPage, IView<TModel>
        where TModel : IViewModel
    {
        public TModel ViewModel { get; private set; }

        private ISubject<BaseView<TModel>> AppearingSubject = new Subject<BaseView<TModel>>();
        private ISubject<BaseView<TModel>> DisappearingSubject = new Subject<BaseView<TModel>>();
        public void SetViewModel(TModel viewModel)
        {
            this.ViewModel = viewModel;
            BindingContext = ViewModel;
        }
        
        public async Task SetViewModel(ICoreContainer<IClientResolvable> clientContainer)
        {
            this.ViewModel = clientContainer.Get<TModel>();
            BindingContext = ViewModel;
        }

        public IObservable<IView> WhenAppearing() => AppearingSubject.AsObservable();
        public IObservable<IView> WhenDisappearing() => DisappearingSubject.AsObservable();
        public bool RetainView { get; set; } = false;

        protected override void OnAppearing()
        {
            AppearingSubject.OnNext(this);
        }

        protected override void OnDisappearing()
        {
            DisappearingSubject.OnNext(this);
        }
    }
}
