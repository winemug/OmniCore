using OmniCore.Mobile.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels.Help
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel(Page page) : base(page)
        {
        }

        public string Version
        {
            get
            {
                return OmniCoreServices.Application.Version;
            }
        }

        protected override async Task<object> BindData()
        {
            return this;
        }

        protected override void OnDisposeManagedResources()
        {
        }
    }
}
