using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Java.Sql;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using Object = Java.Lang.Object;

namespace OmniCore.Client.Droid.Services
{
    public class ForegroundTaskServiceConnection : Object, IServiceConnection
    {
        private ForegroundTaskServiceBinder ForegroundTaskServiceBinder;
        private readonly ISubject<IForegroundTaskService> ServiceInstanceSubject;
        public bool IsConnected { get; private set; }

        public IObservable<IForegroundTaskService> WhenConnected() =>
            ServiceInstanceSubject.AsObservable()
                .Where(instance => instance != null);

        public ForegroundTaskServiceConnection()
        {
            IsConnected = false;
            ServiceInstanceSubject = new BehaviorSubject<IForegroundTaskService>(null);
        }
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            ForegroundTaskServiceBinder = service as ForegroundTaskServiceBinder;
            if (ForegroundTaskServiceBinder != null)
            {
                IsConnected = true;
                ServiceInstanceSubject.OnNext(ForegroundTaskServiceBinder.ServiceInstance);
            }
        }
        public void OnServiceDisconnected(ComponentName name)
        {
            ForegroundTaskServiceBinder = null;
            IsConnected = false;
            ServiceInstanceSubject.OnNext(null);
        }
    }
}