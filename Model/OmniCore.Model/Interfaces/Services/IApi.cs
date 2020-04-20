using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IApi 
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