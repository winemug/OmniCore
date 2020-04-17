using OmniCore.Model.Interfaces.Client;

namespace OmniCore.Client.ViewModels.Base
{
    public class EmptyViewModel : BaseViewModel
    {
        public EmptyViewModel(IClient client) : base(client)
        {
        }
    }
}