using System.Diagnostics;
using Android.Content;

namespace OmniCore.Client.Droid.Services
{
    public class GenericBroadcastReceiver : BroadcastReceiver
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

            Debug.WriteLine(
                $"Xdrip broadcast: {intent.Action} - {timeStamp}, {bg}, {noise}, {bgSlope}, {bgSlopeName}, {sensorBattery}, {dataSource}, {versionInfo}, {calibrationInfo}, {noiseBlockLevel}");
        }
    }
}