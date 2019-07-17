using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels.Pod
{
    public class ActivityViewModel : PageViewModel
    {
        public ActivityViewModel(Page page) : base(page)
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
