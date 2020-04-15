using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services;
using Object = Java.Lang.Object;

namespace OmniCore.Client.Droid.Services
{
    public class AndroidServiceConnection : Object, IServiceConnection, ICoreClientConnection
    {
        private AndroidServiceBinder Binder;
        private readonly ISubject<ICoreApi> ApiSubject;
        private readonly ICorePlatformClient PlatformClient;

        public AndroidServiceConnection(ICorePlatformClient platformClient)
        {
            PlatformClient = platformClient;
            ApiSubject = new BehaviorSubject<ICoreApi>(null);
        }

        public IObservable<ICoreClientConnection> WhenDisconnected() =>
            ApiSubject.FirstAsync(api => api == null).Select(x => this);
        
        public IObservable<ICoreApi> WhenConnected() => ApiSubject.FirstAsync(api => api != null);
        public Task Connect()
        {
            return PlatformClient.AttachToService(typeof(AndroidService), this);
        }
        public Task Disconnect()
        {
            return PlatformClient.DetachFromService(this);
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as AndroidServiceBinder;
            if (Binder == null)
            {
                ApiSubject.OnError(new OmniCoreWorkflowException(FailureType.PlatformGeneralError,
                    "IBinder instance is not of expected type"));
            }
            else
            {
                ApiSubject.OnNext(Binder.Api);
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Binder = null;
            ApiSubject.OnNext(null);
        }
    }
}