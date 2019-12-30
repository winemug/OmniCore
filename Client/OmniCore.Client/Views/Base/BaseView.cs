using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Services;
using Xamarin.Forms;

namespace OmniCore.Client.Views.Base
{
    public abstract class BaseView<T> : ContentPage, IView<T> where T : IViewModel
    {
        public T ViewModel { get; set; }

        public BaseView(T viewModel)
        {
            viewModel.View = (IView<IViewModel>) this;
            ViewModel = viewModel;
            SetBinding(ContentPage.TitleProperty, new Binding(nameof(IViewModel.Title)));
        }

        protected override async void OnAppearing()
        {
            await ViewModel.Initialize();
            BindingContext = ViewModel;
            base.OnAppearing();
        }

        protected override async void OnDisappearing()
        {
            await ViewModel.Dispose();
            base.OnDisappearing();
        }
    }
}
