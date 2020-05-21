using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.UI.Core.AnimationMetrics;
using OmniCore.Eros.Annotations;
using OmniCore.Model.Constants;
using OmniCore.Model.Utilities.Extensions;

namespace OmniCore.Client.Console
{
    public static class DebugTests
    {
        private static BluetoothLEAdvertisementWatcher AdvertisementWatcher
            = new BluetoothLEAdvertisementWatcher();

        private static HashSet<ulong> Discovered;
        public static void StartScan()
        {
            AdvertisementWatcher.ScanningMode = BluetoothLEScanningMode.Active;
            // Only activate the watcher when we're recieving values >= -80
            //watcher.SignalStrengthFilter.InRangeThresholdInDBm = -80;

            // Stop watching if the value drops below -90 (user walked away)
            //watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -90;

            Discovered = new HashSet<ulong>();
            AdvertisementWatcher.AdvertisementFilter = new BluetoothLEAdvertisementFilter()
            {
                Advertisement = new BluetoothLEAdvertisement()
                {
                    ServiceUuids = {Uuids.RileyLinkServiceUuid}
                }
            };

            // Register callback for when we see an advertisements
            AdvertisementWatcher.Received += (sender, eventArgs) =>
            {
                if (!Discovered.Contains(eventArgs.BluetoothAddress))
                {
                    //Discovered.Add(eventArgs.BluetoothAddress);
                    System.Console.WriteLine($"found {ToGuid(eventArgs.BluetoothAddress).AsMacAddress()} " +
                                             $"Rssi: {eventArgs.RawSignalStrengthInDBm} " +
                                             $"LocalName: {eventArgs.Advertisement.LocalName}");
                   
                }
            };

            // Wait 5 seconds to make sure the device is really out of range
            //watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(5000);
            //watcher.SignalStrengthFilter.SamplingInterval = TimeSpan.FromMilliseconds(2000);

            // Starting watching for advertisements
            AdvertisementWatcher.Start();
        }

        public static void StopScan()
        {
            AdvertisementWatcher.Stop();
        }

        public static async Task Connect()
        {
            var u = ulong.Parse("0x886B0FF93BA7");
            using var leDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(u);
            System.Console.WriteLine($"Connection status: {leDevice.ConnectionStatus}");
            System.Console.WriteLine($"Name: {leDevice.Name}");
            
            leDevice.ConnectionStatusChanged += (sender, args) =>
            {
                System.Console.WriteLine($"Connection status changed: {sender.ConnectionStatus}");
            };

            leDevice.NameChanged += (sender, args) =>
            {
                System.Console.WriteLine($"Name changed: {sender.Name}");
            };

            var services = leDevice.GetGattService(Uuids.RileyLinkServiceUuid).GetAllCharacteristics();
        }
        
        private static Guid ToGuid(ulong u)
        {
            var b = new byte[16];
            b[15] = (byte) (u       & 0xff);
            b[14] = (byte) (u >>  8 & 0xff);
            b[13] = (byte) (u >> 16 & 0xff);
            b[12] = (byte) (u >> 24 & 0xff);
            b[11] = (byte) (u >> 32 & 0xff);
            b[10] = (byte) (u >> 40 & 0xff);
            return new Guid(b);
        }

        private static ulong ToUlong(Guid g)
        {
            var b = g.ToByteArray();
            return b[15]
                   | (ulong) b[14] << 8
                   | (ulong) b[13] << 16
                   | (ulong) b[12] << 24
                   | (ulong) b[11] << 32
                   | (ulong) b[10] << 40;
        }
    }
}