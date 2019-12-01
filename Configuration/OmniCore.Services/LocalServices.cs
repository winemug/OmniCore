using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Services
{
    public class LocalServices : ICoreServices
    {
        public LocalServices(
            IPodService podService,
            IRadioService radioService,
            IRepositoryService repositoryService,
            IApplicationService applicationService)
        {
            PodService = podService;
            RadioService = radioService;
            RepositoryService = repositoryService;
            ApplicationService = applicationService;
        }

        public IPodService PodService { get; }
        public IRadioService RadioService { get; }
        public IRepositoryService RepositoryService { get; }
        public IApplicationService ApplicationService { get;}
    }
}
