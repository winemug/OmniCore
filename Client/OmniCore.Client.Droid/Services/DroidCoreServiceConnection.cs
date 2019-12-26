using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Javax.Security.Auth;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    public class DroidCoreServiceConnection : Java.Lang.Object, IServiceConnection
    {
        private Subject<ICoreServices> ServiceConnected = new Subject<ICoreServices>();
        private Subject<IServiceConnection> ServiceDisconnected = new Subject<IServiceConnection>();

        private DroidCoreServiceBinder Binder;
        
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as DroidCoreServiceBinder;
            if (Binder != null)
            {
                ServiceConnected.OnNext(Binder.Services);
                ServiceConnected.OnCompleted();
            }
        }
        public void OnServiceDisconnected(ComponentName name)
        {
            Binder = null;
            ServiceDisconnected.OnNext(this);
            ServiceDisconnected.OnCompleted();
        }

        public IObservable<ICoreServices> WhenConnected()
        {
            return ServiceConnected.AsObservable();
        }

        public IObservable<IServiceConnection> WhenDisconnected()
        {
            return ServiceDisconnected.AsObservable();
        }

    }
}