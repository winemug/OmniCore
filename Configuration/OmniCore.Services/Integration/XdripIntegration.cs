using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services.Integration
{
    public class XdripIntegration : IIntegrationComponent
    {
        private IService ParentService;
        public string ComponentName => "Xdrip Local";

        public string ComponentDescription =>
            "Registers blood glucose metrics received from the Xdrip application installed on the same device.";

        public bool ComponentEnabled { get; set; }

        public Task InitializeComponent(IService parentService)
        {
            ParentService = parentService;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}