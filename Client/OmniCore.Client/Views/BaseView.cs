using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Client.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Client.Views
{
    public abstract class BaseView<T> : ContentPage, IView<T> where T : IViewModel
    {
        public T ViewModel { get; set; }
        protected BaseView()
        {

        }

        public BaseView(T viewModel)
        {
            ViewModel = viewModel;
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
