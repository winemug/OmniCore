using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Workflow;
using Unity;

namespace OmniCore.Eros
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterSingleton<IPodProvider, ErosPodProvider>(RegistrationConstants.OmnipodEros);
            container.RegisterSingleton<IExtendedAttribute, ErosPodExtendedAttribute>(RegistrationConstants.OmnipodEros);

            container.RegisterType<IPodRequest, ErosPodRequest>(RegistrationConstants.OmnipodEros);
            container.RegisterType<IPod, ErosPod>(RegistrationConstants.OmnipodEros);
        }
    }
}

