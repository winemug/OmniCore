using OmniCore.Model.Interfaces;
using OmniCore.Radios.RileyLink;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform;
using Unity;

namespace OmniCore.Radios.RileyLink
{
    public static class Initializer
    {
        public static IUnityContainer WithRileyLinkRadio(this IUnityContainer container)
        {
            container.RegisterSingleton<IRadioService, RileyLinkRadioService>(RegistrationConstants.RileyLink);
            container.RegisterType<IRadio, RileyLinkRadio>(RegistrationConstants.RileyLink);
            container.RegisterType<IRadioLease, RileyLinkRadioLease>(RegistrationConstants.RileyLink);
            return container;
        }
    }
}
