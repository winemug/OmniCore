using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Client;

namespace OmniCore.Client.ViewModels.Base
{
    public class SplashViewModel : BaseViewModel
    {
        public SplashViewModel(IClient client) : base(client)
        {
        }
    }
}