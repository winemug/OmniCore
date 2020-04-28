using System;

namespace OmniCore.Model.Entities
{
    public class BlePeripheralOptions
    {
        public bool PeripheralAutoConnect { get; set; } = true;
        public TimeSpan PeripheralDiscoveryTimeout { get; set; } = TimeSpan.FromSeconds(15);
        public TimeSpan PeripheralDiscoveryCooldown { get; set; } = TimeSpan.FromSeconds(45);

        public TimeSpan PeripheralConnectTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan CharacteristicsDiscoveryTimeout { get; set; } = TimeSpan.FromSeconds(15);
        public TimeSpan CharacteristicResponseTimeout { get; set; } = TimeSpan.FromSeconds(20);
        public TimeSpan PeripheralDisconnectTimeout { get; set; } = TimeSpan.FromSeconds(5);
    }
}