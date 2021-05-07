using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Client.Console.Platform;
using OmniCore.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;
using OmniCore.Radios.RileyLink;
using OmniCore.Repository;
using OmniCore.Services;
using OmniCore.Simulation;
using Unity;

namespace OmniCore.Client.Console
{
    public static class ConsoleContainer
    {
        public static IContainer Instance { get; }
        static ConsoleContainer()
        {
            Instance = new Container(new UnityContainer());
            Instance
                .Existing(Instance)
                .WithDefaultServices()
                .WithOmnipodEros()
                .WithRileyLinkRadio()
                .WithWindowsBleAdapter()
                .WithEfCoreRepository()
                .WithWindowsPlatformServices()
                .WithEfCoreRepository();
        }

        public static IContainer WithWindowsPlatformServices(this IContainer container)
        {
            // ILogger logger,
            //     IPlatformFunctions platformFunctions,
            // IUserActivity userActivity,
            //     IServiceApi serviceApi,
            // IPlatformConfiguration platformConfiguration)
            return container
                .One<IPlatformConfiguration, ConsoleConfiguration>()
                .One<IPlatformFunctions, ConsoleFunctions>()
                .One<ILogger, ConsoleLogger>();
        }
        public static IContainer WithWindowsBleAdapter(this IContainer container)
        {
            return container
                .One<IBlePeripheralAdapter, WinBlePeripheralAdapter>()
                .Many<IBlePeripheral, WinBlePeripheral>()
                .Many<IBlePeripheralConnection, WinBlePeripheralConnection>();
        }
    }
}
