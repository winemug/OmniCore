using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Client.ViewModels.Home
{
    public class ProgressPopupViewModel : BaseViewModel
    {
        public ProgressPopupViewModel(ICoreClient client) : base(client)
        {
        }

        public ITaskProgress Progress => (ITaskProgress) Parameter;

        protected override Task OnPageAppearing()
        {
            return base.OnPageAppearing();
        }
    }
}