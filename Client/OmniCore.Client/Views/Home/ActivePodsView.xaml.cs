using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Home
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivePodsView : IView
    {
        public ActivePodsView()
        {
            InitializeComponent();
        }
    }
}