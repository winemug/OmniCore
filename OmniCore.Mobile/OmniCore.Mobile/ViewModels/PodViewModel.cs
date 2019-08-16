using OmniCore.Mobile.Interfaces;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class PodViewModel : PageViewModel, IViewModel
    {
        public IPod Pod { get; set; }

        public PodViewModel(IPod pod, Page page) : base(page)
        {
            this.Pod = pod;
        }

        protected override Task<BaseViewModel> BindData()
        {
            throw new NotImplementedException();
        }

        protected override void OnDisposeManagedResources()
        {
            throw new NotImplementedException();
        }
    }
}
