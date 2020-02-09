using OmniCore.Model.Interfaces.Platform.Common;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Platform.Server;

namespace OmniCore.Eros
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithOmnipodEros
            (this ICoreContainer<IServerResolvable> container)
        {
            return container
                .One<IErosPodProvider, ErosPodProvider>()
                .Many<ErosPod>()
                .Many<IPodRequest, ErosPodRequest>()
                .Many<ITaskQueue, ErosTaskQueue>();
        }
    }
}

