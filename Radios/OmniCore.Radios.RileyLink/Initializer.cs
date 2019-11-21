using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Operational;
using OmniCore.Radios.RileyLink;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;

namespace OmniCore.Radios.RileyLink
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IRadioProvider, RileyLinkRadioProvider>(nameof(RileyLinkRadioProvider));

            container.RegisterFactory<IRadio>((container, type, name) => { return new RileyLinkRa });

            UnityContainer
            var r = Invoke.
        }
    }
}
