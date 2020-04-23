using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IServiceApi 
    {
        IObservable<CoreApiStatus> ApiStatus { get; }
        IConfigurationService ConfigurationService { get; }
        IPodService PodService { get; }
        IIntegrationService IntegrationService { get; }
        IAutomationService AutomationService { get; }
        Task StartServices(CancellationToken cancellationToken);
        Task StopServices(CancellationToken cancellationToken);
    }
}