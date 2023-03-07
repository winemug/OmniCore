using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Services.Interfaces.Radio;
using Unity;
using Unity.Lifetime;

namespace OmniCore.Services;

public static class Initializer
{
    public static void RegisterTypesForServices(IUnityContainer container)
    {
        container.RegisterType<IConfigurationService, ConfigurationService>(new ContainerControlledLifetimeManager());
        container.RegisterType<IDataService, DataService>(new ContainerControlledLifetimeManager());
        container.RegisterType<IPodService, PodService>(new ContainerControlledLifetimeManager());
        container.RegisterType<IRadioService, RadioService>(new ContainerControlledLifetimeManager());
        container.RegisterType<IAmqpService, AmqpService>(new ContainerControlledLifetimeManager());
        container.RegisterType<ICoreService, CoreService>(new ContainerControlledLifetimeManager());

        container.RegisterType<IRadio, Radio>();
        container.RegisterType<IRadioConnection, RadioConnection>();
        container.RegisterType<IPod, Pod>();
        container.RegisterType<IPodConnection, PodConnection>();
        container.RegisterType<IPodMessage, PodMessage>();
        container.RegisterType<IPodPacket, PodPacket>();
        
        // container.RegisterType<BleService>(new ContainerControlledLifetimeManager());
        // container.RegisterType<BgcService>(new ContainerControlledLifetimeManager());
        // container.RegisterType<XDripWebServiceClient>(new ContainerControlledLifetimeManager());
        // container.RegisterType<SyncClient>(new ContainerControlledLifetimeManager());
        // container.RegisterType<ApiClient>(new ContainerControlledLifetimeManager());
    }
}