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

namespace OmniCore.Client.Droid.Services
{
    public class AndroidServiceConnection : Java.Lang.Object, IServiceConnection
    {
        private readonly Subject<AndroidService> ServiceConnected = new Subject<AndroidService>();
        private readonly Subject<AndroidServiceConnection> ServiceDisconnected = new Subject<AndroidServiceConnection>();

        private AndroidServiceBinder Binder;
        
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as AndroidServiceBinder;
            if (Binder != null)
            {
                ServiceConnected.OnNext(Binder.AndroidService);
            }
        }
        public void OnServiceDisconnected(ComponentName name)
        {
            Binder = null;
            ServiceDisconnected.OnNext(this);
        }

        public IObservable<AndroidService> WhenConnected()
        {
            return ServiceConnected.AsObservable();
        }

        public IObservable<AndroidServiceConnection> WhenDisconnected()
        {
            return ServiceDisconnected.AsObservable();
        }
    }
}