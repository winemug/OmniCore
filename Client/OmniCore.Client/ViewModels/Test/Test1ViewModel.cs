using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Client.Models;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Test
{
    public class Test1ViewModel : TaskViewModel
    {
        public Test1ViewModel(ICoreClient client) : base(client)
        {
            IdentifyCommand = new Command(async () => await Identify());
            Radios = new ObservableCollection<RadioModel>();
        }

        public Command IdentifyCommand { get; }

        public ObservableCollection<RadioModel> Radios { get; set; }

        protected override Task OnPageAppearing()
        {
            Disposables.Add(Api.PodService.ListErosRadios().Subscribe(
                radio => { Radios.Add(new RadioModel(radio)); }));
            return base.OnPageAppearing();
        }

        private async Task Identify()
        {
            var radio = Radios.First(r => r.IsChecked).Radio;
            var user = await Api.ConfigurationService.GetDefaultUser();
            var med = await Api.ConfigurationService.GetDefaultMedication();
            var pod = await Api.PodService.NewErosPod(user, med, CancellationToken.None);

            await pod.Acquire(radio, CancellationToken.None);
        }
    }
}