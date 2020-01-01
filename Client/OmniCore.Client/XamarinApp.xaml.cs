using System.Threading;
using OmniCore.Client.Views.Main;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client
{
    public partial class XamarinApp : Application, IPlatformApplication
    {
        public XamarinApp(ICoreClient client)
        {
            InitializeComponent();
            MainPage = client.Container.Get<ShellView>();
        }
    }
}
