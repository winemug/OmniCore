using OmniCore.Model.Interfaces;
using OmniCore.Radio.RileyLink;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;

namespace OmniCore.Eros
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IRadioProvider, RileyLinkRadioProvider>(nameof(RileyLinkRadioProvider));
            container.RegisterType<IPodProvider<ErosPod>, ErosPodProvider>();
        }
    }
}

