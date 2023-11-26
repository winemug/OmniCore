using OmniCore.Client.Views;

namespace OmniCore.Client
{
    public partial class App : Application
    {
        public App(EmptyPage emptyPage)
        {
            InitializeComponent();

            MainPage = emptyPage;
        }
    }
}
