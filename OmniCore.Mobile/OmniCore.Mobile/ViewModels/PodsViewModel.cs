using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class PodsViewModel : PageViewModel
    {
        public PodsViewModel(Page page) : base(page)
        {
        }

        public List<ErosPod> Pods { get; set; }

        protected override void OnDisposeManagedResources()
        {
        }

        protected override async Task<BaseViewModel> BindData()
        {
            var repo = await ErosRepository.GetInstance();
            Pods = await repo.GetActivePods();
            return this;
        }
    }
}
