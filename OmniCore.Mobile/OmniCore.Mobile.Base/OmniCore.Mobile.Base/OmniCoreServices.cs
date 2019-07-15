using OmniCore.Mobile.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace OmniCore.Mobile.Base
{
    public static class OmniCoreServices
    {
        private static IOmniCoreLogger logger;
        private static IOmniCoreApplication application;
        private static IRemoteRequestPublisher publisher;
        private static IAppState appState;

        public static IOmniCoreLogger Logger
        {
            get
            {
                if (logger == null)
                {
                    logger = DependencyService.Get<IOmniCoreLogger>();
                }
                return logger;
            }
        }

        public static IOmniCoreApplication Application
        {
            get
            {
                if (application == null)
                {
                    application = DependencyService.Get<IOmniCoreApplication>();
                }
                return application;
            }
        }

        public static IRemoteRequestPublisher Publisher
        {
            get
            {
                if (publisher == null)
                {
                    publisher = DependencyService.Get<IRemoteRequestPublisher>();
                }
                return publisher;
            }
        }

        public static IAppState AppState
        {
            get
            {
                if (appState == null)
                {
                    appState = DependencyService.Get<IAppState>();
                }
                return appState;
            }
        }

        public static SynchronizationContext UiSyncContext { get; set; }

    }
}
