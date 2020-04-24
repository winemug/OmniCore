using OmniCore.Model.Interfaces;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Test
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestControlView : IView
    {
        public TestControlView()
        {
            InitializeComponent();
        }
    }
}