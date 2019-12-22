using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Eros
{
    public static class Initializer
    {
        public static IUnityContainer WithOmnipodEros(this IUnityContainer container)
        {
            container.RegisterSingleton<IPodService, ErosPodService>(RegistrationConstants.OmnipodEros);

            container.RegisterType<IPodRequest, ErosPodRequest>(RegistrationConstants.OmnipodEros);
            container.RegisterType<IPod, ErosPod>(RegistrationConstants.OmnipodEros);
            return container;
        }
    }
}

