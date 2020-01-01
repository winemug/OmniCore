using System;
using System.Reactive.Linq;
using Android.Content;
using Android.OS;
using OmniCore.Client.Droid;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using Application = Xamarin.Forms.Application;
using Debug = System.Diagnostics.Debug;

namespace OmniCore.Client.Droid
{
    public class BaseActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, ICoreClientContext
    {
        private ICoreContainer ClientContainer;
        private IDisposable ServiceConnectSubscription;
        private IDisposable ServiceDisconnectSubscription;
        private ICoreServicesConnection CoreServicesConnection;
        private IServiceConnection ServiceConnection => CoreServicesConnection as IServiceConnection;
        private bool IsServiceConnected = false;
        private bool SavedValue = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Debug.WriteLine("ONCREATE");
            SavedValue = true;

            ClientContainer = Initializer.AndroidClientContainer(this)
                .WithXamarinForms();

            CoreServicesConnection = ClientContainer.Get<ICoreServicesConnection>();
            
            ServiceConnectSubscription.Dispose();
            ServiceConnectSubscription = CoreServicesConnection.WhenConnected.Subscribe((coreServices) =>
            {
                ClientContainer.Get<ICoreClient>().CoreServices = coreServices;
                IsServiceConnected = true;
            });

            ServiceDisconnectSubscription.Dispose();
            ServiceDisconnectSubscription = CoreServicesConnection.WhenDisconnected.Subscribe((connection) =>
            {
                ClientContainer.Get<ICoreClient>().CoreServices = null;
                IsServiceConnected = false;
            });

            ConnectToAndroidService();
        }

        protected override void OnStart()
        {
            Debug.WriteLine($"ONSTART {SavedValue}");
            SavedValue = true;

            ConnectToAndroidService();
            LoadXamarinApplication();
            
            base.OnStart();
        }

        protected override void OnResume()
        {
            Debug.WriteLine($"ONRESUME {SavedValue}");
            SavedValue = true;
            base.OnResume();
        }

        protected override void OnPause()
        {
            Debug.WriteLine($"ONPAUSE {SavedValue}");
            SavedValue = true;
            base.OnPause();
        }

        protected override void OnStop()
        {
            Debug.WriteLine($"ONSTOP {SavedValue}");
            SavedValue = true;
            base.OnStop();
            
            DisconnectFromAndroidService();
        }

        protected override void OnRestart()
        {
            Debug.WriteLine($"ONRESTART {SavedValue}");
            SavedValue = true;
            base.OnRestart();
        }

        protected override void OnDestroy()
        {
            Debug.WriteLine($"ONDESTROY {SavedValue}");
            SavedValue = true;
            base.OnDestroy();
            
            DisconnectFromAndroidService();
        }

        private void LoadXamarinApplication()
        {
            LoadApplication(ClientContainer.Get<XamarinApp>());
        }
        
        private void ConnectToAndroidService()
        {
            if (IsServiceConnected)
                return;
            
            var intent = new Intent(this, typeof(AndroidService));
            if (base.BindService(intent, ServiceConnection, Bind.AutoCreate))
                throw new OmniCoreUserInterfaceException(FailureType.ServiceConnectionFailed);

            CoreServicesConnection.WhenConnected.Take(1).Wait();
        }

        private void DisconnectFromAndroidService()
        {
            if (!IsServiceConnected)
                return;
            
            var connection = ClientContainer.Get<ICoreServicesConnection>();
            var serviceConnection = connection as IServiceConnection;
            
            ServiceDisconnectSubscription?.Dispose();
            ServiceDisconnectSubscription = connection.WhenDisconnected.Subscribe((connection) =>
            {
                IsServiceConnected = false;
            });
            
            base.UnbindService(serviceConnection);
        }
    }
}