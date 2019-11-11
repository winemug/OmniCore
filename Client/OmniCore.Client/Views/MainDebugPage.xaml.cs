using Microsoft.AppCenter.Crashes;
using OmniCore.Model.Interfaces;
using OmniCore.Repository;
using OmniCore.Repository.Entities;
using OmniCore.Repository.Enums;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
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
    [Fody.ConfigureAwait(true)]
    public partial class MainDebugPage : ContentPage
    {
        public ObservableCollection<Radio> Radios { get; set; }
        public ICommand TestCommand { get; set;}

        private IDisposable ScanSubscription;
        public MainDebugPage()
        {
            InitializeComponent();
            Radios = new ObservableCollection<Radio>();
            TestCommand = new Command(async (o) =>
            {
                ScanSubscription?.Dispose();
                var page = new RadioTestPage();
                page.BindingContext = o;
                await Navigation.PushAsync(page);
            },
            (_) => true);
            BindingContext = this;
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            await EnsurePermissions();
            ScanSubscription?.Dispose();
            Radios.Clear();
            ScanSubscription = App.Instance.PodProvider.ListRadios()
                .ObserveOn(App.Instance.UiSyncContext)
                .Subscribe( (radio) =>
                {
                    Radios.Add(radio);
                });

        }


        private async void ContentPage_Disappearing(object sender, EventArgs e)
        {
            ScanSubscription?.Dispose();
        }

        private async Task EnsurePermissions()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.LocationAlways);
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Missing Permissions", "Please grant the location permission to this application in order to be able to connect to bluetooth devices.", "OK");
                    var request = await CrossPermissions.Current.RequestPermissionsAsync(Permission.LocationAlways);
                    if (request[Permission.LocationAlways] != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Missing Permissions", "OmniCore cannot run without the necessary permissions.", "OK");
                        App.Instance.OmniCoreApplication.Exit();
                    }
                }
            }
            catch (Exception e)
            {
                await DisplayAlert("Missing Permissions", "Error while querying / acquiring permissions", "OK");
                Crashes.TrackError(e);
            }
        }
    }
} 