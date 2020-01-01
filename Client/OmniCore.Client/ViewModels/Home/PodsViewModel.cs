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
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Main;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Home
{
    public class PodsViewModel : BaseViewModel
    {
        public List<IPod> Pods { get; set; }

        public ICommand SelectCommand { get; set; }

        public ICommand AddCommand { get; set; }

        private ICoreApplicationService ApplicationService => Services.ApplicationService;

        private IPodService PodService => Services.PodService;

        public PodsViewModel(ICoreClient client) : base(client)
        {
            Title = "Pods";
            SelectCommand = new Command<IPod>(async pod => await SelectPod(pod));
            AddCommand = new Command(async _ => await AddPod());
        }

        public override async Task OnInitialize()
        {
            
            Pods = new List<IPod>();
            await foreach (var pod in PodService.ActivePods())
            {
                Pods.Add(pod);
            }
        }

        public override async Task OnDispose()
        {
            Pods = null;
        }

        private async Task AddPod()
        {
            await Shell.Current.Navigation.PushAsync(Services.Container.Get<PodWizardMainView>());
        }

        private async Task SelectPod(IPod pod)
        {
            //var view = Container.Resolve<RadioDetailView>();
            //view.ViewModel.Radio = radio;
            //await Shell.Current.Navigation.PushAsync(view);
        }
    }
}
