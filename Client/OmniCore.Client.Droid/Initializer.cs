using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniCore.Client.Interfaces;
using OmniCore.Mobile.Droid;
using OmniCore.Model.Interfaces;
using Unity;

namespace OmniCore.Client.Droid
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterSingleton<IRemoteRequestPublisher, RemoteRequestPublisher>();
            container.RegisterSingleton<IOmniCoreApplication, OmniCoreApplication>();
            container.RegisterSingleton<IOmniCoreLogger, OmniCoreLogger>();
            container.RegisterSingleton<IAppState, OmniCoreAppState>();
            container.RegisterSingleton(typeof(IBackgroundTaskFactory<>), typeof(BackgroundTaskFactory<>));

            OmniCore.Client.Initializer.RegisterTypes(container);
        }
    }
}