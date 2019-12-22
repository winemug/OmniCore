using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    [Service]
    public class DroidCoreService : Service
    {

        public DroidCoreServiceBinder Binder { get; private set; }
        
     
        public override IBinder OnBind(Intent intent)
        {
            Binder = new DroidCoreServiceBinder(this);
            return Binder;
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override bool OnUnbind(Intent intent)
        {
            return base.OnUnbind(intent);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}