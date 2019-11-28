using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Configuration
{
    public class LocalServices : ICoreServices
    {
        public LocalServices(
            IPodProvider[] podProviders,
            IRadioProvider[] radioProviders,
            IRepositoryService repositoryService,
            IApplicationService applicationService)
        {
            PodProviders = podProviders;
            RadioProviders = radioProviders;
            RepositoryService = repositoryService;
            ApplicationService = applicationService;
        }

        public IPodProvider[] PodProviders { get; }
        public IRadioProvider[] RadioProviders { get; }
        public IRepositoryService RepositoryService { get; }
        public IApplicationService ApplicationService { get;}
    }
}
