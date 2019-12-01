using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Services
{
    public class RadioService : IRadioService
    {
        public IRadioProvider[] Providers { get; }

        public RadioService(IRadioProvider[] providers)
        {
            Providers = providers;
        }
    }
}