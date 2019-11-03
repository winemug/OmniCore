using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainDebugPage : ContentPage
    {
        public ObservableCollection<string> RadioNames { get; set; }
        public bool StartEnabled { get; set; }
        public bool StopEnabled { get; set; }

        public MainDebugPage()
        {
            InitializeComponent();
            RadioNames = new ObservableCollection<string>();
            StartEnabled = true;
            StopEnabled = false;
            BindingContext = this;
        }

        private IDisposable radioObservable;

        private async void SearchStart_Clicked(object sender, EventArgs e)
        {
            StartEnabled = false;
            RadioNames.Clear();
            radioObservable = App.Instance.PodProvider.ListAllRadios().Subscribe( radio =>
            {
                RadioNames.Add($"Name: {radio.DeviceName} Address: {radio.DeviceId}");
            });
            StopEnabled = true;
        }

        private async void SearchStop_Clicked(object sender, EventArgs e)
        {
            StopEnabled = false;
            radioObservable.Dispose();
            StartEnabled = true;
        }
    }
}