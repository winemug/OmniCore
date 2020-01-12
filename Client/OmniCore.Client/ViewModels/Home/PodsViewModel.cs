using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.ViewModels.Wizards;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Main;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Common.Data;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Home
{
    public class PodsViewModel : BaseViewModel
    {
        public List<IPod> Pods { get; set; }

        public ICommand SelectCommand { get; set; }

        public ICommand AddCommand { get; set; }

        private ICoreApplicationFunctions ApplicationFunctions => ServiceApi.ApplicationFunctions;

        private IPodService PodService => ServiceApi.PodService;

        public PodsViewModel(ICoreClient client) : base(client)
        {
            SelectCommand = new Command<IPod>(async pod => await SelectPod(pod));
            AddCommand = new Command(async _ => await AddPod());
        }

        protected override async Task OnPageAppearing()
        {
            
            Pods = new List<IPod>();
            await foreach (var pod in PodService.ActivePods())
            {
                Pods.Add(pod);
            }
        }

        private async Task AddPod()
        {
            await Shell.Current.Navigation.PushAsync(Client.ViewPresenter.GetView<PodWizardMainView>(false));
        }

        private Task SelectPod(IPod pod)
        {
            throw new NotImplementedException();
        }
    }
}
