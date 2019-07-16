using OmniCore.Mobile.Base;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    [Fody.ConfigureAwait(true)]
    public abstract class PageViewModel : BaseViewModel
    {
        protected Page AssociatedPage;
        private bool HasAppeared { get; set; }

        public PageViewModel(Page page)
        {
            AssociatedPage = page;
            page.Appearing += Page_Appearing;
            page.Disappearing += Page_Disappearing;
            MessagingCenter.Subscribe<App>(this, MessagingConstants.AppResuming, async (_) =>
            {
                var typeStr = OmniCoreServices.AppState.GetString(AppStateConstants.ActivePage, null);
                if (typeStr == this.GetType().FullName)
                {
                    HasAppeared = true;
                    var dataContext = await DataBind();
                    if (AssociatedPage != null)
                        AssociatedPage.BindingContext = dataContext;
                    await OnAppearing();
                }
            });

            MessagingCenter.Subscribe<App>(this, MessagingConstants.AppSleeping, async (_) =>
            {
                if (HasAppeared)
                {
                    Application.Current.Properties[AppStateConstants.ActivePage] = this.GetType().FullName;
                }
            });
        }

        private async void Page_Appearing(object sender, EventArgs e)
        {
            HasAppeared = true;
            var dataContext = await DataBind();
            if (AssociatedPage != null)
                AssociatedPage.BindingContext = dataContext;
            await OnAppearing();
        }

        private async void Page_Disappearing(object sender, EventArgs e)
        {
            HasAppeared = false;
            await OnDisappearing();
            Dispose();
        }

        [method: SuppressMessage("", "CS1998", Justification = "Virtual function")]
        protected async virtual Task OnAppearing()
        {
        }

        [method: SuppressMessage("", "CS1998", Justification = "Virtual function")]
        protected async virtual Task OnDisappearing()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !base.disposedValue)
            {
                if (AssociatedPage != null)
                {
                    AssociatedPage.Appearing -= Page_Appearing;
                    AssociatedPage.Disappearing -= Page_Disappearing;
                }
                MessagingCenter.Unsubscribe<App>(this, MessagingConstants.AppSleeping);
                MessagingCenter.Unsubscribe<App>(this, MessagingConstants.AppResuming);
            }
            base.Dispose(disposing);
        }
    }
}
