using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Client.Models;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services.Facade;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Test
{
    public class Test1ViewModel : BaseViewModel
    {

        public Command AcquireCommand { get; }
        public Command RadioScanStartCommand { get;  }
        public Command RadioScanStopCommand { get;  }

        public IPodRequest ActiveRequest { get; private set; }
        
        public RadioModel SelectedRadio { get; set; }

        private IDisposable RadioScanSubsription;
        
        public ObservableCollection<RadioModel> Radios { get; set; }
        
        public Test1ViewModel(ICoreClient client) : base(client)
        {
            AcquireCommand = new Command(async () => await Acquire());
            RadioScanStartCommand = new Command(() => RadioScanStart());
            RadioScanStopCommand = new Command(() => RadioScanStop());
            Radios = new ObservableCollection<RadioModel>();
        }

        private async Task Acquire()
        {
            RadioScanStop();

            var user = await Api.CoreConfigurationService.GetDefaultUser();
            var med = await Api.CoreConfigurationService.GetDefaultMedication();
            var pod = await Api.CorePodService.NewErosPod(user, med, CancellationToken.None);
            IRadio selectedRadio = null;
            foreach (var radioModel in Radios)
            {
                if (radioModel.IsChecked)
                {                    
                    selectedRadio = radioModel.Radio;
                    break;
                }
            }

            ActiveRequest = await pod.Acquire(selectedRadio, CancellationToken.None);
        }

        private void RadioScanStart()
        {
            Radios.Clear();

            RadioScanSubsription?.Dispose();
            RadioScanSubsription = Api.CorePodService.ListErosRadios().Subscribe(
                radio => { Radios.Add(new RadioModel(radio)); });
        }

        private void RadioScanStop()
        {
            RadioScanSubsription?.Dispose();
            RadioScanSubsription = null;
        }
    }
}
