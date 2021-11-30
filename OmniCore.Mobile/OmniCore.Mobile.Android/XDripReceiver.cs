using Android.App;
using Android.Content;
using Debug = System.Diagnostics.Debug;

namespace OmniCore.Mobile.Droid
{
    // Xdrip intent action names, not sure if all are broadcast globally
    
    // com.eveningoutpost.dexdrip.StatusUpdate
    // com.eveningoutpost.dexdrip.BgEstimate
    // com.eveningoutpost.dexdrip.BgEstimateNoData
    // com.eveningoutpost.dexdrip.Snooze
    // com.eveningoutpost.dexdrip.VehicleMode
    
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[]
    {
        "com.eveningoutpost.dexdrip.StatusUpdate",
        "com.eveningoutpost.dexdrip.BgEstimate",
        "com.eveningoutpost.dexdrip.BgEstimateNoData"
    })]
    public class XDripReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent != null)
            {
                Debug.WriteLine($"Received intent action: {intent.Action}");
            }
        }
    }
}