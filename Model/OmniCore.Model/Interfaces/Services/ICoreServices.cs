using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreServices
    {
        IApplicationService ApplicationService { get; }
        IPodService PodService { get; }
        IRadioService RadioService { get; }
        IRepositoryService RepositoryService { get; }
        IUserService UserService { get; }
    }
}
