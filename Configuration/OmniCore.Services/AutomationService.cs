using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services
{
    public class AutomationService : ServiceBase, IAutomationService
    {
        private readonly IAutomationComponent[] AutomationComponents;
        private readonly IContainer<IServiceInstance> Container;
        private readonly ILogger Logger;

        public AutomationService(
            IContainer<IServiceInstance> container,
            IAutomationComponent[] automationComponents,
            ILogger logger)
        {
            Logger = logger;
            Container = container;
            AutomationComponents = automationComponents;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            Logger.Debug("Starting automation service");
            foreach (var ac in AutomationComponents)
            {
                try
                {
                    await ac.InitializeComponent(this);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.Warning($"Failed to initialize automation component {ac.GetType()}", e);
                }
            }
            Logger.Debug("Automation service started");
        }

        protected override Task OnStop(CancellationToken cancellationToken)
        {
            foreach (var ac in AutomationComponents)
                try
                {
                    ac.Dispose();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.Warning($"Failed to dispose automation component {ac.GetType()}", e);
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