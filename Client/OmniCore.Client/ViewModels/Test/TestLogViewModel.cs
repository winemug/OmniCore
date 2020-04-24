using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces;

namespace OmniCore.Client.ViewModels.Test
{
    public class TestLogViewModel : BaseViewModel
    {
        public TestLogViewModel(IClient client) : base(client)
        {
        }
    }
}