using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NavigationView : IView
    {
        public NavigationView()
        {
            InitializeComponent();
        }
    }
}