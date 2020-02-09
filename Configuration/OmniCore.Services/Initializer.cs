using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Base;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Services;

namespace OmniCore.Services
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithDefaultServices
            (this ICoreContainer<IServerResolvable> container)
        {
            return container
                .One<IRepositoryService, RepositoryService>()
                .One<IPodService, OmniCorePodService>();
        }
    }
}
