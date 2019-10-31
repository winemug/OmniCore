using OmniCore.Impl.Eros;
using OmniCore.Client.Interfaces;
using OmniCore.Client.Repositories;
using OmniCore.Client.Services;
using OmniCore.Client.ViewModels;
using OmniCore.Client.Views;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace OmniCore.Client
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
            container.RegisterSingleton<IPodResultRepository<ErosResult>, ErosPodResultRepository>();
        }
    }
}
