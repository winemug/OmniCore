using OmniCore.Impl.Eros;
using OmniCore.Mobile.Interfaces;
using OmniCore.Mobile.Services;
using OmniCore.Mobile.ViewModels;
using OmniCore.Mobile.Views;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;

namespace OmniCore.Mobile
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterSingleton<IPodRepository<ErosPod>, ErosPodRepository>();
            container.RegisterSingleton<IRadioAdapter, CrossBleRadioAdapter>();

            OmniCore.Impl.Eros.Initializer.RegisterTypes(container);
        }
    }
}
