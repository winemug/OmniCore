using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Client.Views.Main;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client
{
    public partial class XamarinApp : Application
    {
        private readonly ICoreClient Client;
        public XamarinApp(ICoreClient client)
        {
            Client = client;
            InitializeComponent();
            MainPage = Client.Container.Get<ShellView>();
        }
    }
}
