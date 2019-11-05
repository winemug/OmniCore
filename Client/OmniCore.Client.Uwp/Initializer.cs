using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using OmniCore.Client;
using OmniCore.Model.Interfaces;
using OmniCore.Client.Uwp.Platform;
using OmniCore.Client.Interfaces;

namespace OmniCore.Client.Uwp
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            //container.RegisterSingleton<IRemoteRequestPublisher, RemoteRequestPublisher>();
            container.RegisterSingleton<IOmniCoreApplication, Application>();
            container.RegisterSingleton<IOmniCoreLogger, Logger>();
            container.RegisterSingleton<IAppState, AppState>();
            container.RegisterSingleton<IBackgroundTaskFactory, BackgroundTaskFactory>();
            container.RegisterSingleton<IBackgroundTaskFactory, BackgroundTaskFactory>();

            OmniCore.Client.Initializer.RegisterTypes(container);
        }
    }
}
