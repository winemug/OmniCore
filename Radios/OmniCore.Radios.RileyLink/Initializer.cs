using OmniCore.Model.Interfaces;
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
        }
    }
}
