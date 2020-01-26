using OmniCore.Model.Interfaces.Platform.Common;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Constants;

namespace OmniCore.Eros
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithOmnipodEros
            (this ICoreContainer<IServerResolvable> container)
        {
            return container
                .One<IPodService, ErosPodService>()
                .Many<IPod, ErosPod>()
                .Many<IPodRequest, ErosPodRequest>()
                .Many<ITaskQueue, ErosTaskQueue>();
        }
    }
}

