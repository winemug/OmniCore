using OmniCore.Services.Interfaces;
using Unity;
using Unity.Lifetime;

namespace OmniCore.Services;

public static class Initializer
{
    public static void RegisterTypes(IUnityContainer container)
    {
        container.RegisterType<IForegroundService, CoreService>(new ContainerControlledLifetimeManager());
        container.RegisterType<ConfigurationStore>(new ContainerControlledLifetimeManager());
        container.RegisterType<DataService>(new ContainerControlledLifetimeManager());
        container.RegisterType<PodService>(new ContainerControlledLifetimeManager());
        container.RegisterType<RadioService>(new ContainerControlledLifetimeManager());
        container.RegisterType<AmqpService>(new ContainerControlledLifetimeManager());

        // container.RegisterType<BleService>(new ContainerControlledLifetimeManager());
        // container.RegisterType<BgcService>(new ContainerControlledLifetimeManager());
        // container.RegisterType<XDripWebServiceClient>(new ContainerControlledLifetimeManager());
        // container.RegisterType<SyncClient>(new ContainerControlledLifetimeManager());
        // container.RegisterType<ApiClient>(new ContainerControlledLifetimeManager());
    }
}