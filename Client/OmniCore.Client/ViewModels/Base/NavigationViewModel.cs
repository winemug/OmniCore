using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public abstract class NavigationViewModel : BaseViewModel
    {
        protected ContentPage RootPage { get; set; }
        public override Task Initialize()
        {
            return Task.CompletedTask;
        }

        public override Task Dispose()
        {
            return Task.CompletedTask;
        }

        protected NavigationViewModel(ICoreServices services) : base(services)
        {
        }
    }
}
