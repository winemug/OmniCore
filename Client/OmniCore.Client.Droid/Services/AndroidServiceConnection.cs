using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.Content;
using Android.OS;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services;
using Object = Java.Lang.Object;

namespace OmniCore.Client.Droid.Services
{
    public class AndroidServiceConnection : Object, IServiceConnection, ICoreClientConnection
    {
        private AndroidServiceBinder Binder;
        private readonly ISubject<ICoreApi> CoreServicesSubject;

        public AndroidServiceConnection()
        {
            CoreServicesSubject = new BehaviorSubject<ICoreApi>(null);
        }

        public IObservable<ICoreApi> WhenConnectionChanged()
        {
            return CoreServicesSubject.AsObservable();
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as AndroidServiceBinder;
            CoreServicesSubject.OnNext(Binder.Api);
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Binder = null;
            CoreServicesSubject.OnNext(null);
        }
    }
}