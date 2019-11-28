using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Workflow;
using Unity;

namespace OmniCore.Eros
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IPodProvider, ErosPodProvider>();
        }
    }
}

