using OmniCore.Model.Interfaces;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Home
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RadiosView : IView
    {
        public RadiosView()
        {
            InitializeComponent();
        }
    }
}