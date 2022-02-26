using OmniCore.Mobile.ViewModels;
using OmniCore.Mobile.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using OmniCore.Mobile.Services;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Mobile
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Shelly.CurrentItem = StartPageContent;
            // Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            // Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

        protected override async void OnNavigating(ShellNavigatingEventArgs args)
        {
            var page = Shell.Current?.CurrentPage;
            if (page != null)
            {
                var vbr = App.Container.Resolve<ViewModelBindingRegistry>();
                var bvm = vbr.GetViewModelForInstance(page);
                await Device.InvokeOnMainThreadAsync(async () =>
                {
                    await bvm.OnNavigatedFromAsync();
                });

            }
            base.OnNavigating(args);
        }

        protected override async void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            var page = Shell.Current?.CurrentPage;
            if (page != null)
            {
                var vbr = App.Container.Resolve<ViewModelBindingRegistry>();
                var bvm = vbr.GetViewModelForInstance(page);
                await Device.InvokeOnMainThreadAsync(async () =>
                {
                    await bvm.OnNavigatedToAsync(page);
                });
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Debug.WriteLine($"*** On back button pressed");
            return base.OnBackButtonPressed();
        }
    }
}
