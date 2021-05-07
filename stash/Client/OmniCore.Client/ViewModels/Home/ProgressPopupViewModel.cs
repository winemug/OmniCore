using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces;

namespace OmniCore.Client.ViewModels.Home
{
    public class ProgressPopupViewModel : BaseViewModel
    {
        public ProgressPopupViewModel(IClient client) : base(client)
        {
        }
    }
}