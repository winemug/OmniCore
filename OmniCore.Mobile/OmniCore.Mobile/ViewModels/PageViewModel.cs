using OmniCore.Mobile.Base;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public PageViewModel(Page page)
        {
            AssociatedPage = page;
            page.Appearing += Page_Appearing;
            page.Disappearing += Page_Disappearing;
        }

        private async void Page_Appearing(object sender, EventArgs e)
        {
            var dataContext = await DataBind();
            if (AssociatedPage != null)
                AssociatedPage.BindingContext = dataContext;
            await OnAppearing();
        }

        private async void Page_Disappearing(object sender, EventArgs e)
        {
            await OnDisappearing();
            Dispose();
        }

        protected async virtual Task OnAppearing()
        {
        }

        protected async virtual Task OnDisappearing()
        {
        }

        protected override void OnDisposeManagedResources()
        {
            if (AssociatedPage != null)
            {
                AssociatedPage.Appearing -= Page_Appearing;
                AssociatedPage.Disappearing -= Page_Disappearing;
            }
        }
    }
}
