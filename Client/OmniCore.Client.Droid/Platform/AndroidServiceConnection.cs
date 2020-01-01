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

namespace OmniCore.Client.Droid.Platform
{
    public class AndroidServiceConnection : Java.Lang.Object, IServiceConnection, ICoreServicesConnection
    {
        public IObservable<ICoreServices> WhenConnected => ServiceConnected.AsObservable();
        public IObservable<ICoreServicesConnection> WhenDisconnected => ServiceDisconnected.AsObservable();

        private readonly Subject<ICoreServices> ServiceConnected = new Subject<ICoreServices>();
        private readonly Subject<ICoreServicesConnection> ServiceDisconnected = new Subject<ICoreServicesConnection>();

        private AndroidServiceBinder Binder;
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as AndroidServiceBinder;
            if (Binder != null)
                ServiceConnected.OnNext(Binder.CoreServices);
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Binder = null;
            ServiceDisconnected.OnNext(this);
        }
    }
}