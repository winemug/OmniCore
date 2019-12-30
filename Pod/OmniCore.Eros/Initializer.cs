using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Eros
{
    public static class Initializer
    {
        public static ICoreContainer WithOmnipodEros(this ICoreContainer container)
        {
            return container
                .One<IPodService, ErosPodService>()
                .Many<IPodRequest, ErosPodRequest>()
                .Many<IPod, ErosPod>();
        }
    }
}

