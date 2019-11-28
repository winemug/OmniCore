using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreServices
    {
        IPodProvider[] PodProviders { get; }
        IRadioProvider[] RadioProviders { get; }
        IRepositoryService RepositoryService { get; }
        IApplicationService ApplicationService { get; }
    }
}
