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
using Java.Time.Temporal;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Client.Droid.Services
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { 
                "com.eveningoutpost.dexdrip.BgEstimate",
                "com.eveningoutpost.dexdrip.Extras.Sender",
                "com.eveningoutpost.dexdrip.Extras.BgEstimate",
                "com.eveningoutpost.dexdrip.Extras.BgSlope",
                "com.eveningoutpost.dexdrip.Extras.BgSlopeName",
                "com.eveningoutpost.dexdrip.Extras.SensorBattery",
                "com.eveningoutpost.dexdrip.Extras.Time",
                "com.eveningoutpost.dexdrip.Extras.Raw",
                "com.eveningoutpost.dexdrip.Extras.Noise",
                "com.eveningoutpost.dexdrip.Extras.NoiseWarning",
                "com.eveningoutpost.dexdrip.Extras.NoiseBlockLevel",
                "com.eveningoutpost.dexdrip.Extras.NsNoiseLevel",
                "com.eveningoutpost.dexdrip.Extras.SourceDesc",
                "com.eveningoutpost.dexdrip.Extras.SourceInfo",
                "com.eveningoutpost.dexdrip.Extras.VersionInfo",
                "com.eveningoutpost.dexdrip.Extras.CalibrationInfo",
                "com.eveningoutpost.dexdrip.Extras.CalibrationPluginInfo",
                "com.eveningoutpost.dexdrip.NewCalibration",
                "com.eveningoutpost.dexdrip.BgEstimateNoData",
                "com.eveningoutpost.dexdrip.StatusUpdate",
                "com.eveningoutpost.dexdrip.Snooze",
                "com.eveningoutpost.dexdrip.VehicleMode",
                "com.eveningoutpost.dexdrip.VehicleMode.Enabled",
                "com.eveningoutpost.dexdrip.Extras.Collector.NanoStatus"
    })]

    public class XdripReceiver : BroadcastReceiver, IIntegrationComponent
    {
        private const string EXTRA_BG_ESTIMATE = "com.eveningoutpost.dexdrip.Extras.BgEstimate";
        private const string EXTRA_BG_SLOPE = "com.eveningoutpost.dexdrip.Extras.BgSlope";
        private const string EXTRA_BG_SLOPE_NAME = "com.eveningoutpost.dexdrip.Extras.BgSlopeName";
        private const string EXTRA_SENSOR_BATTERY = "com.eveningoutpost.dexdrip.Extras.SensorBattery";
        private const string EXTRA_TIMESTAMP = "com.eveningoutpost.dexdrip.Extras.Time";
        private const string EXTRA_NOISE = "com.eveningoutpost.dexdrip.Extras.Noise";
        private const string EXTRA_NOISE_BLOCK_LEVEL = "com.eveningoutpost.dexdrip.Extras.NoiseBlockLevel";
        private const string XDRIP_DATA_SOURCE_DESCRIPTION = "com.eveningoutpost.dexdrip.Extras.SourceDesc";
        private const string XDRIP_VERSION_INFO = "com.eveningoutpost.dexdrip.Extras.VersionInfo";
        private const string XDRIP_CALIBRATION_INFO = "com.eveningoutpost.dexdrip.Extras.CalibrationInfo";

        public override void OnReceive(Context context, Intent intent)
        {
            // Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();
            var bg = intent.GetDoubleExtra(EXTRA_BG_ESTIMATE, double.MinValue);
            var timeStamp = intent.GetLongExtra(EXTRA_TIMESTAMP, long.MinValue);
            var noise = intent.GetDoubleExtra(EXTRA_NOISE, double.MinValue);
            var bgSlope = intent.GetDoubleExtra(EXTRA_BG_SLOPE, double.MinValue);
            var bgSlopeName = intent.GetStringExtra(EXTRA_BG_SLOPE_NAME);
            var sensorBattery = intent.GetIntExtra(EXTRA_SENSOR_BATTERY, int.MinValue);

            var dataSource = intent.GetStringExtra(XDRIP_DATA_SOURCE_DESCRIPTION);
            var versionInfo = intent.GetStringExtra(XDRIP_VERSION_INFO); 
            var calibrationInfo = intent.GetStringExtra(XDRIP_CALIBRATION_INFO);
            var noiseBlockLevel = intent.GetIntExtra(EXTRA_NOISE_BLOCK_LEVEL, int.MinValue);

            System.Diagnostics.Debug.WriteLine(
                $"Xdrip broadcast: {intent.Action} - {timeStamp}, {bg}, {noise}, {bgSlope}, {bgSlopeName}, {sensorBattery}, {dataSource}, {versionInfo}, {calibrationInfo}, {noiseBlockLevel}");
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