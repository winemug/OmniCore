using OmniCore.Impl.Eros;
using OmniCore.Mobile.Interfaces;
using OmniCore.Mobile.Services;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class PodsViewModel : PageViewModel, IViewModel
    {
        public IPodRepository<ErosPod> PodRepository { get; }

        public PodsViewModel(Page page) : base(page)
        {
        }

        public IList<ErosPod> Pods { get; set; }

        protected override void OnDisposeManagedResources()
        {
        }

        protected override async Task<BaseViewModel> BindData()
        {
            Pods = await PodRepository.GetActivePods();
            return this;
        }
    }
}
