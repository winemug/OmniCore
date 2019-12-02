using Fody;
using Microsoft.AppCenter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Client.ViewModels.Test
{
    [ConfigureAwait(true)]
    public class RadioTestingViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<IRadio> Radios { get; set; }
        public ObservableCollection<IPod> Pods { get; set; }
        public ICommand NextPageCommand { get; set; }

        public IRadio Radio { get; set; }

        public IPod Pod { get; set; }
        private IDisposable ScanSubscription;

        public RadioTestingViewModel()
        {
            Radios = new ObservableCollection<IRadio>();
            Pods = new ObservableCollection<IPod>();
        }

        public async Task LoadPods()
        {
            Pods.Clear();
//            using(var pr = RepositoryProvider.Instance.PodRepository)
//            {
//                var allPods = await pr.GetActivePods();
//                foreach (var pod in allPods)
//                    Pods.Add(pod);
//            }
        }

        public async Task StartScanning()
        {
            ScanSubscription?.Dispose();
            Radios.Clear();
//            ScanSubscription = App.Instance.PodProvider.ListRadios()
//                .ObserveOn(App.Instance.UiSyncContext)
//                .Subscribe((radio) =>
//                {
//                    Radios.Add(radio);
//                });
        }

        public async Task StopScanning()
        {
            ScanSubscription?.Dispose();
        }

        public async Task AddPod(uint radioId)
        {
//            var p = new Pod { RadioAddress = radioId };
//            using (var pr = RepositoryProvider.Instance.PodRepository)
//            {
//                await pr.Create(p);
//            }
//            await Xamarin.Forms.Device.InvokeOnMainThreadAsync(() => Pods.Insert(0, p));
        }
    }
}
