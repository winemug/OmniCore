using System.Threading;
using Android.App;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client.Droid.Platform
{
    public class CoreClient : ICoreClient
    {
        public CoreClient(ICoreContainer clientContainer, ICoreServicesConnection servicesConnection)
        {
            Container = clientContainer;
            ServicesConnection = servicesConnection;
        }
        public ICoreContainer Container { get; }
        public ICoreServicesConnection ServicesConnection { get; }
        public SynchronizationContext UiSynchronizationContext => Application.SynchronizationContext;
    }
}