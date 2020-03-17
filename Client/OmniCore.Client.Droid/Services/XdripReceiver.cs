using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Client.Droid.Services
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "com.eveningoutpost.dexdrip.BgEstimate" })]
    public class XdripReceiver : BroadcastReceiver, IIntegrationComponent
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();
        }

        public string ComponentName => "Xdrip Local";
        public string ComponentDescription => "Registers blood glucose metrics received from the Xdrip application installed on the same device.";

        public bool ComponentEnabled
        {
            get => IsComponentEnabled;
            set => SetComponentState(value);
        }

        private bool IsComponentEnabled;
        private ICoreService ParentService;

        public async Task InitializeComponent(ICoreService parentService)
        {
            ParentService = parentService;
        }

        private void SetComponentState(bool enable)
        {
            if (IsComponentEnabled == enable)
                return;

            if (enable)
                EnableComponent();
            else
                DisableComponent();
        }

        private void EnableComponent()
        {
            IsComponentEnabled = true;
        }

        private void DisableComponent()
        {

            IsComponentEnabled = false;
        }
    }
}