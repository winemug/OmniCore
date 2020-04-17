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
    public class AndroidServiceConnection : Object, IServiceConnection, IClientConnection
    {
        private AndroidServiceBinder Binder;
        private readonly ISubject<IApi> ApiSubject;
        private readonly IClientFunctions ClientFunctions;

        public AndroidServiceConnection(IClientFunctions clientFunctions)
        {
            ClientFunctions = clientFunctions;
            ApiSubject = new BehaviorSubject<IApi>(null);
        }

        public IObservable<IClientConnection> WhenDisconnected() =>
            ApiSubject.FirstAsync(api => api == null).Select(x => this);
        
        public IObservable<IApi> WhenConnected() => ApiSubject.FirstAsync(api => api != null);
        public Task Connect()
        {
            return ClientFunctions.AttachToService(typeof(AndroidService), this);
        }
        public Task Disconnect()
        {
            return ClientFunctions.DetachFromService(this);
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