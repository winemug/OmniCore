using nexus.protocols.ble;
using nexus.protocols.ble.scan;
using Omni.Py;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Py
{
    public class pr_rileylink: PacketRadio
    {
        private byte[] PA_LEVELS = new byte[] { 0x12,
             0x0E, 0x0E,
             0x1D, 0x1D,
             0x34, 0x34, 0x34,
             0x2C, 0x2C, 0x2C, 0x2C,
             0x60, 0x60, 0x60, 0x60,
             0x84, 0x84, 0x84, 0x84, 0x84,
             0xC8, 0xC8, 0xC8, 0xC8, 0xC8,
             0xC0, 0xC0 };

        private Guid RileyLinkServiceUUID = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");
        private Guid RileyLinkDataCharacteristicUUID = Guid.Parse("c842e849-5028-42e2-867c-016adada9155");
        private Guid RileyLinkResponseCharacteristicUUID = Guid.Parse("6e6c7910-b89e-43a5-a0fe-50c5e2b81f4a");

        private IBluetoothLowEnergyAdapter Ble;
        private logger Logger;
        private IBlePeripheral Peripheral;

        public pr_rileylink(IBluetoothLowEnergyAdapter ble)
        {
            this.Ble = ble;
            this.Logger = definitions.getLogger();
        }

        public void connect(bool force_initialize = false)
        {
        }

        public void disconnect(bool ignore_errors = true)
        {
        }

        public byte[] get_packet(int timeout = 5000)
        {
            return null;
        }

        public byte[] send_and_receive_packet(byte[] packet, int repeat_count, int delay_ms, int timeout_ms, int retry_count, int preamble_ext_ms)
        {
            return null;
        }

        public void set_tx_power(TxPower tx_power)
        {
        }

        public void tx_down()
        {
        }

        public void tx_up()
        {
        }

        private async Task<IBlePeripheral> FindRileyLink()
        {
            if (this.Ble.CurrentState.IsDisabledOrDisabling() && this.Ble.AdapterCanBeEnabled)
            {
                Console.WriteLine("Enabling ble adapter");
                await this.Ble.EnableAdapter();
            }

            if (this.Peripheral != null)
                return this.Peripheral;

            using (var cts = new CancellationTokenSource(10000))
            {
                Console.WriteLine("Scanning");
                await this.Ble.ScanForBroadcasts(
                   new ScanSettings()
                   {
                       Mode = ScanMode.LowPower,
                       Filter = new ScanFilter()
                       {
                           AdvertisedServiceIsInList = new List<Guid>() { RileyLinkServiceUUID },
                       },
                       IgnoreRepeatBroadcasts = true
                   },
                   (peripheral) =>
                   {
                       this.Logger.log($"Found RL at address {peripheral.Address}, name: {peripheral.Advertisement.DeviceName}");

                       this.Peripheral = peripheral;
                       cts.Cancel();
                   }, cts.Token);
            }
            return this.Peripheral;
        }
    }
}
