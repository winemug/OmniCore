using System;
using System.Threading;
using Android.App;
using Android.Content;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client.Droid.Platform
{
    public class CoreClient : ICoreClient
    {
        public ICoreServices CoreServices { get; set; }
        public ICoreContainer Container { get; }
        public SynchronizationContext SynchronizationContext => Application.SynchronizationContext;
        public CoreClient(ICoreContainer clientContainer)
        {
            Container = clientContainer;
        }
    }
}