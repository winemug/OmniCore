using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Workflow;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PodDebugPage : ContentPage
    {
        public List<IPod> Pods { get; set; }

        public PodDebugPage()
        {
            InitializeComponent();
            BindingContext = this;
        }
        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
//            Pods = await App.Instance.PodProvider.GetActivePods();
            OnPropertyChanged(nameof(Pods));
        }
    }
}