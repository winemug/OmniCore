using System;
using System.Collections.Generic;
using System.Text;
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
        public CoreAutomationService(ICoreContainer<IServerResolvable> container,
            IAutomationComponent[] automationComponents)
        {
            Container = container;
            AutomationComponents = automationComponents;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            foreach (var ac in AutomationComponents)
                await ac.InitializeComponent(this);
        }

        protected override async Task OnStop(CancellationToken cancellationToken)
        {
            foreach (var ac in AutomationComponents)
                ac.Dispose();
        }

        protected override async Task OnPause(CancellationToken cancellationToken)
        {
        }

        protected override async Task OnResume(CancellationToken cancellationToken)
        {
        }
    }
}
