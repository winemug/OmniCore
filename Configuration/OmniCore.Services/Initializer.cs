﻿using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Services.Integration;

namespace OmniCore.Services
{
    public static class Initializer
    {
        public static IContainer WithDefaultServices
            (this IContainer container)
        {
            return container
                .One<IRepositoryService, RepositoryService>()
                .One<IConfigurationService, ConfigurationService>()
                .One<IPodService, PodService>()
                .One<IAutomationService, AutomationService>()
                .One<IIntegrationService, IntegrationService>()
                .One<IIntegrationComponent, MqttIntegration>(nameof(MqttIntegration))
                .One<IIntegrationComponent, XdripIntegration>(nameof(XdripIntegration))
                .One<IServiceApi, ServiceApi>();
        }
    }
}