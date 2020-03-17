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
    public class CoreIntegrationService : CoreServiceBase, ICoreIntegrationService
    {
        private readonly IIntegrationComponent[] IntegrationComponents;
        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreConfigurationService ConfigurationService;
        public CoreIntegrationService(ICoreContainer<IServerResolvable> container,
            ICoreConfigurationService configurationService,
            IIntegrationComponent[] integrationComponents)
        {
            Container = container;
            ConfigurationService = configurationService;
            IntegrationComponents = integrationComponents;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            foreach (var ic in IntegrationComponents)
            {
                await ic.InitializeComponent(this);
            }
        }

        protected override async Task OnStop(CancellationToken cancellationToken)
        {
            foreach (var ic in IntegrationComponents)
                ic.Dispose();
        }

        protected override async Task OnPause(CancellationToken cancellationToken)
        {
        }

        protected override async Task OnResume(CancellationToken cancellationToken)
        {
        }
    }
}
