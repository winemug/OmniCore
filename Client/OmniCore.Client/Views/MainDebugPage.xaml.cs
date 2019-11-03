using OmniCore.Model.Interfaces;
using OmniCore.Repository;
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
        public ObservableCollection<IRadio> Radios { get; set; }
        public bool StartEnabled { get; set; }
        public bool StopEnabled { get; set; }
        public bool ConfirmEnabled { get; set; }

        public MainDebugPage()
        {
            InitializeComponent();
            Radios = new ObservableCollection<IRadio>();
            StartEnabled = true;
            StopEnabled = false;
            ConfirmEnabled = false;
            BindingContext = this;
        }

        private IDisposable radioObservable;

        private async void SearchStart_Clicked(object sender, EventArgs e)
        {
            StartEnabled = false;
            Radios.Clear();
            radioObservable = App.Instance.PodProvider.ListAllRadios().Subscribe( radio =>
            {
                Radios.Add(radio);
            });
            StopEnabled = true;
        }

        private async void SearchStop_Clicked(object sender, EventArgs e)
        {
            StopEnabled = false;
            radioObservable.Dispose();
            StartEnabled = true;
        }

        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfirmEnabled = e.CurrentSelection.Any();
        }

        private async void Confirm_Clicked(object sender, EventArgs e)
        {
            var upr = new UserProfileRepository();
            var q = await upr.ForQuery();
            var userProfile = await q.FirstAsync();

            var radioList = new List<IRadio>();
            foreach(IRadio radio in RadioCollection.SelectedItems)
            {
                radioList.Add(radio);
            }
            var pod = await App.Instance.PodProvider.New(userProfile, radioList);
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PodDebugPage());
        }
    }
}