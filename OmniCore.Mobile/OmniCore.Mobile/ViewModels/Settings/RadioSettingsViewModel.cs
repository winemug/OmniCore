using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels.Settings
{
    public class RadioSettingsViewModel : PageViewModel
    {
        public RadioSettingsViewModel(Page page) : base(page)
        {
        }

        protected override void OnDisposeManagedResources()
        {
        }

        protected override async Task<BaseViewModel> BindData()
        {
            return this;
        }
    }
}
