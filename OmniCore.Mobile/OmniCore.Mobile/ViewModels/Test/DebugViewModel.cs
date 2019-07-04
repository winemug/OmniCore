using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels.Test
{
    public class DebugViewModel : PageViewModel
    {
        public DebugViewModel(Page page) : base(page)
        {
        }

        protected async override Task<BaseViewModel> BindData()
        {
            return this;
        }

        protected override void OnDisposeManagedResources()
        {
        }
    }
}
