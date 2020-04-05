using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services
{
    public class CoreAutomationService : CoreServiceBase, ICoreAutomationService
    {
        private readonly IAutomationComponent[] AutomationComponents;
        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreLoggingFunctions Logging;

        public CoreAutomationService(
            ICoreContainer<IServerResolvable> container,
            IAutomationComponent[] automationComponents,
            ICoreLoggingFunctions logging)
        {
            Logging = logging;
            Container = container;
            AutomationComponents = automationComponents;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            Logging.Debug("Starting automation service");
            foreach (var ac in AutomationComponents)
                await ac.InitializeComponent(this);
            Logging.Debug("Automation service started");
        }

        protected override Task OnStop(CancellationToken cancellationToken)
        {
            foreach (var ac in AutomationComponents)
                ac.Dispose();
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