using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Data;

namespace OmniCore.Eros
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithOmnipodEros
            (this ICoreContainer<IServerResolvable> container)
        {
            return container
                .One<IPodService, ErosPodServiceBase>()
                .Many<IPodRequest, ErosPodRequest>()
                .Many<IPod, ErosPod>();
        }
    }
}

