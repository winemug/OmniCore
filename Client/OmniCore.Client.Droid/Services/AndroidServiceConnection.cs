using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Javax.Security.Auth;
using Nito.AsyncEx;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    public class AndroidServiceConnection : Java.Lang.Object, IServiceConnection, ICoreClientConnection
    {
        private AndroidServiceBinder Binder;
        private ISubject<ICoreApi> CoreServicesSubject;

        public AndroidServiceConnection()
        {
            CoreServicesSubject = new BehaviorSubject<ICoreApi>(null);
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

        public IObservable<ICoreApi> WhenConnectionChanged()
        {
            return CoreServicesSubject.AsObservable();
        }
    }
}