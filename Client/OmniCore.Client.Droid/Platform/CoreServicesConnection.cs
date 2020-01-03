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
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client.Droid.Platform
{
    public class CoreServicesConnection : Java.Lang.Object, IServiceConnection, ICoreServicesConnection
    {
        private AndroidServiceBinder Binder;
        private ISubject<ICoreServices> CoreServicesSubject;
        private IObservable<ICoreServices> ServicesObservable;

        public CoreServicesConnection()
        {
            CoreServicesSubject = new BehaviorSubject<ICoreServices>(null);
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as AndroidServiceBinder;
            CoreServicesSubject.OnNext(Binder.CoreServices);
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Binder = null;
            CoreServicesSubject.OnNext(null);
        }

        public IObservable<ICoreServices> WhenConnectionChanged()
        {
            return CoreServicesSubject.AsObservable();
        }
    }
}