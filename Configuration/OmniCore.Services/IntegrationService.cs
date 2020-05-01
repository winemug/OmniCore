using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services
{
    public class IntegrationService : ServiceBase, IIntegrationService
    {
        private readonly IConfigurationService ConfigurationService;
        private readonly IContainer Container;
        private readonly IIntegrationComponent[] IntegrationComponents;
        private readonly ILogger Logger;

        public IntegrationService(IContainer container,
            IConfigurationService configurationService,
            IIntegrationComponent[] integrationComponents,
            ILogger logger)
        {
            Logger = logger;
            Container = container;
            ConfigurationService = configurationService;
            IntegrationComponents = integrationComponents;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            Logger.Debug("Starting integration service");
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
                    Logger.Warning($"Failed to initialize integration component {ic.GetType()}", e);
                }

            Logger.Debug("Integration service started");
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
                    Logger.Warning($"Failed to dispose integration component {ic.GetType()}", e);
                }
            return Task.CompletedTask;
        }
    }
}