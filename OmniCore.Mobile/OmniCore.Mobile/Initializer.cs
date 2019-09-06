using OmniCore.Impl.Eros;
using OmniCore.Mobile.Interfaces;
using OmniCore.Mobile.Repositories;
using OmniCore.Mobile.Services;
using OmniCore.Mobile.ViewModels;
using OmniCore.Mobile.Views;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace OmniCore.Mobile
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            RegisterRepositories(container);
            container.RegisterSingleton<IRadioAdapter, CrossBleRadioAdapter>();

            OmniCore.Impl.Eros.Initializer.RegisterTypes(container);
        }

        private static void RegisterRepositories(IUnityContainer container)
        {
            container.RegisterSingleton<IPodRepository<ErosPod>, ErosPodRepository>();
            container.RegisterSingleton<IPodRequestRepository<ErosRequest>, ErosPodRequestRepository>();
        }
    }
}
