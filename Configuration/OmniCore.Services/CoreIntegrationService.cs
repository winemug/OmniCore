using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services
{
    public class CoreIntegrationService : CoreServiceBase, ICoreIntegrationService
    {
        private readonly ICoreConfigurationService ConfigurationService;
        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly IIntegrationComponent[] IntegrationComponents;
        private readonly ICoreLoggingFunctions Logging;

        public CoreIntegrationService(ICoreContainer<IServerResolvable> container,
            ICoreConfigurationService configurationService,
            IIntegrationComponent[] integrationComponents,
            ICoreLoggingFunctions logging)
        {
            Logging = logging;
            Container = container;
            ConfigurationService = configurationService;
            IntegrationComponents = integrationComponents;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            Logging.Debug("Starting integration service");
            foreach (var ic in IntegrationComponents)
                try
                {
                    await ic.InitializeComponent(this);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logging.Warning($"Failed to initialize integration component {ic.GetType()}", e);
                }

            Logging.Debug("Integration service started");
        }

        protected override Task OnStop(CancellationToken cancellationToken)
        {
            foreach (var ic in IntegrationComponents)
                try
                {
                    ic.Dispose();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logging.Warning($"Failed to dispose integration component {ic.GetType()}", e);
                }
            return Task.CompletedTask;
        }

        protected override Task OnPause(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task OnResume(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}