using OmniCore.Services.Interfaces;
using Unity;
using Unity.Lifetime;

namespace OmniCore.Services;

public static class Initializer
{
    public static void RegisterTypes(IUnityContainer container)
    {
        container.RegisterType<IForegroundService, ForegroundService>(new ContainerControlledLifetimeManager());
        container.RegisterType<ConfigurationService>(new ContainerControlledLifetimeManager());
        container.RegisterType<IDataService, DataService>(new ContainerControlledLifetimeManager());
        container.RegisterType<IPodService, PodService>(new ContainerControlledLifetimeManager());
        container.RegisterType<IRadioService, RadioService>(new ContainerControlledLifetimeManager());
        container.RegisterType<IAmqpService, AmqpService>(new ContainerControlledLifetimeManager());

        // container.RegisterType<BleService>(new ContainerControlledLifetimeManager());
        // container.RegisterType<BgcService>(new ContainerControlledLifetimeManager());
        // container.RegisterType<XDripWebServiceClient>(new ContainerControlledLifetimeManager());
        // container.RegisterType<SyncClient>(new ContainerControlledLifetimeManager());
        // container.RegisterType<ApiClient>(new ContainerControlledLifetimeManager());
    }
}