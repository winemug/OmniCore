using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreServices
    {
        IPodService PodService { get; }
        IRadioService RadioService { get; }
        IRepositoryService RepositoryService { get; }
    }
}
