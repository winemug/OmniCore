using OmniCore.Model.Interfaces;
using OmniCore.Radios.RileyLink;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Workflow;
using Unity;

namespace OmniCore.Radios.RileyLink
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterSingleton<IRadioProvider, RileyLinkRadioProvider>(RegistrationConstants.RileyLink);
            container.RegisterType<IRadio, RileyLinkRadio>(RegistrationConstants.RileyLink);
            container.RegisterType<IRadioConnection, RileyLinkRadioConnection>(RegistrationConstants.RileyLink);
        }
    }
}
