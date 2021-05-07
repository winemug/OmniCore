using System;

namespace OmniCore.Model.Entities
{
    public class BleOptions
    {
        public bool PeripheralAutoConnect { get; set; } = true;
        public TimeSpan EnableAdapterTimeout { get; set; } = TimeSpan.FromSeconds(10); 
        public TimeSpan PeripheralDiscoveryTimeout { get; set; } = TimeSpan.FromSeconds(15);
        public TimeSpan PeripheralDiscoveryCooldown { get; set; } = TimeSpan.FromSeconds(45);

        public TimeSpan PeripheralConnectTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan ServicesDiscoveryTimeout { get; set; } = TimeSpan.FromSeconds(15);
        public TimeSpan CharacteristicsDiscoveryTimeout { get; set; } = TimeSpan.FromSeconds(15);
        public TimeSpan CharacteristicReadTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan CharacteristicWriteTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan PeripheralDisconnectTimeout { get; set; } = TimeSpan.FromSeconds(5);
    }
}