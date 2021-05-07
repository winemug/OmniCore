using OmniCore.Model.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Home
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RadioScanView : ContentPage, IView
    {
        public RadioScanView()
        {
            InitializeComponent();
        }
    }
}