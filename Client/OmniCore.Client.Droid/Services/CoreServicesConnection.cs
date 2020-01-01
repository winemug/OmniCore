using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.App;
using Android.Content;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client.Droid.Services
{
    public class CoreServicesConnection : ICoreServicesConnection
    {
        
        private AndroidServiceConnection ServiceConnection;
        private readonly Subject<ICoreServices> ServiceConnected;
        private readonly Subject<ICoreServicesConnection> ServiceDisconnected;
        
        public CoreServicesConnection()
        {
            ServiceConnected = new Subject<ICoreServices>();
            ServiceDisconnected = new Subject<ICoreServicesConnection>();
            ServiceConnection = new AndroidServiceConnection();
            ServiceConnection.WhenConnected().Subscribe((androidService) =>
            {
                CoreServices = androidService.CoreServices;
                ServiceConnected.OnNext(CoreServices);
            });

            ServiceConnection.WhenDisconnected().Subscribe((_) =>
            {
                CoreServices = null;
                ServiceDisconnected.OnNext(this);
            });
        }
        
        public bool Connect()
        {
            var intent = new Intent(Application.Context, typeof(AndroidService));
            return Application.Context.BindService(intent, ServiceConnection, Bind.AutoCreate);
        }

        public void Disconnect()
        {
            Application.Context.UnbindService(ServiceConnection);
            ServiceConnection = null;
        }

        public IObservable<ICoreServices> WhenConnected()
        {
            return ServiceConnected.AsObservable();
        }

        public ICoreServices CoreServices { get; private set; }

        public IObservable<ICoreServicesConnection> WhenDisconnected()
        {
            return ServiceDisconnected.AsObservable();
        }
    }
}