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
using OmniCore.Mobile.Base.Interfaces;
using OmniCore.Mobile.Interfaces;
using Unity;

namespace OmniCore.Mobile.Android
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterSingleton<IRemoteRequestPublisher, RemoteRequestPublisher>();
            container.RegisterSingleton<IOmniCoreApplication, OmniCoreApplication>();
            container.RegisterSingleton<IOmniCoreLogger, OmniCoreLogger>();
            container.RegisterSingleton<IAppState, OmniCoreAppState>();

            OmniCore.Mobile.Initializer.RegisterTypes(container);
        }
    }
}