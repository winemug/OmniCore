using OmniCore.Model.Interfaces;
using OmniCore.Repository;
using OmniCore.Repository.Entities;
using OmniCore.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainDebugPage : ContentPage
    {
        public ObservableCollection<Radio> Radios { get; set; }
        public ICommand TestCommand { get; set;}

        private IDisposable ScanSubscription;
        public MainDebugPage()
        {
            InitializeComponent();
            Radios = new ObservableCollection<Radio>();
            BindingContext = this;
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            ScanSubscription?.Dispose();
            Radios.Clear();
            ScanSubscription = App.Instance.PodProvider.ListRadios()
                .ObserveOn(App.Instance.UiSyncContext)
                .Subscribe( (radio) =>
                {
                    Radios.Add(radio);
                });

            TestCommand = new Command(async (o) =>
            {
                var page = new RadioTestPage();
                page.BindingContext = o;
                await Navigation.PushAsync(page);
            });
        }


        private async void ContentPage_Disappearing(object sender, EventArgs e)
        {
            ScanSubscription?.Dispose();
        }
    }
} 