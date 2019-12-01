using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Services
{
    public class PodService : IPodService
    {
        public IPodProvider[] Providers { get; }

        public PodService(IPodProvider[] providers)
        {
            Providers = providers;
        }
    }
}