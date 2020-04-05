using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Test
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Test1View : IView
    {
        public Test1View()
        {
            InitializeComponent();
        }
    }
}