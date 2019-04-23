using nexus.protocols.ble;
using nexus.protocols.ble.scan;
using OmniCore.Model.Protocol.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Radio.RileyLink
{
    public class RileyLink : IPacketRadio
    {
        private IBlePeripheral _peripheral;
        private IBleGattServerConnection _connection;
        private readonly IBluetoothLowEnergyAdapter Ble;
        private bool _variablesInitialized = false;
        private Timer _disconnectTimer;

        private Queue<byte[]> ResponseQueue;
        private ManualResetEventSlim NotificationQueued = new ManualResetEventSlim();

        private Guid RileyLinkServiceUUID = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");
        private Guid RileyLinkDataCharacteristicUUID = Guid.Parse("c842e849-5028-42e2-867c-016adada9155");
        private Guid RileyLinkResponseCharacteristicUUID = Guid.Parse("6e6c7910-b89e-43a5-a0fe-50c5e2b81f4a");

        public RileyLink(IBluetoothLowEnergyAdapter ble)
        {
            this.Ble = ble;
        }

        private async Task<IBlePeripheral> GetRileyLink()
        {
            if (this.Ble.CurrentState.IsDisabledOrDisabling() && this.Ble.AdapterCanBeEnabled)
            {
                Console.WriteLine("Enabling ble adapter");
                await this.Ble.EnableAdapter();
            }

            if (_peripheral != null)
                return _peripheral;

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
                       Console.WriteLine($"Found peripheral at address {peripheral.Address}, name: {peripheral.Advertisement.DeviceName}");

                       _peripheral = peripheral;
                       cts.Cancel();
                   }, cts.Token);
            }
            return _peripheral;
        }

        private async Task<IBleGattServerConnection> GetRLConnection()
        {
            var peripheral = await this.GetRileyLink();
            if (peripheral == null)
            {
                throw new Exception("Failed to find RL");
            }

            if (_connection == null || _connection.State != ConnectionState.Connected)
            {
                Console.WriteLine($"Starting new connection");
                var connectionRequest = await this.Ble.ConnectToDevice(_peripheral, TimeSpan.FromSeconds(10));
                if (connectionRequest.IsSuccessful())
                {
                    Console.WriteLine($"Connected");
                    _connection = connectionRequest.GattServer;
                    if (_disconnectTimer != null)
                    {
                        _disconnectTimer.Dispose();
                    }
                    _disconnectTimer = new Timer(
                        async (state) =>
                        {
                            var conn = (IBleGattServerConnection)state;
                            if (conn != null && conn.State == ConnectionState.Connected)
                                await conn.Disconnect();
                        },
                        _connection, Timeout.Infinite, Timeout.Infinite);

                    ResponseQueue = new Queue<byte[]>();

                    _connection.NotifyCharacteristicValue(RileyLinkServiceUUID, RileyLinkResponseCharacteristicUUID,
                    async (byte[] data) =>
                    {
                        Console.WriteLine($"Notify counter: {data[0]}");
                        var response = await _connection.ReadCharacteristicValue(RileyLinkServiceUUID, RileyLinkDataCharacteristicUUID);
                        ResponseQueue.Enqueue(response);
                        NotificationQueued.Set();
                        ResetTimer();
                    });

                    Console.WriteLine($"Emptying queue");
                    await _connection.WriteCharacteristicValue(RileyLinkServiceUUID, RileyLinkDataCharacteristicUUID, new byte[] { 0x01, 0x00 });

                    while (true)
                    {
                        NotificationQueued.Wait(200);
                        if (ResponseQueue.Count > 0)
                            ResponseQueue.Dequeue();
                        else
                            break;
                        NotificationQueued.Reset();
                    }
                    Console.WriteLine($"Queue emptied");
                }
            }
            ResetTimer();
            return _connection;
        }

        public async Task Initialize()
        {
            var conn = await GetRLConnection();
            if (conn == null)
                throw new Exception("Couldn't create a connection to RL");

            if (_variablesInitialized)
                return;


            Console.WriteLine("Initializing variables");
            await SendCommand(conn, RileyLinkCommandType.ResetRadioConfig);
            await SendCommand(conn, RileyLinkCommandType.SetSwEncoding, new byte[] { (byte)RileyLinkSoftwareEncoding.Manchester });
            var frequency = (int)(433910000.0 / (24000000.0 / Math.Pow(2, 16)));
            await SendCommand(conn, RileyLinkCommandType.SetPreamble, new byte[] { 0x66, 0x65 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREQ0, (byte)(frequency & 0xff) });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREQ1, (byte)((frequency >> 8) & 0xff) });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREQ2, (byte)((frequency >> 16) & 0xff) });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.PKTCTRL1, 0x20 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.PKTCTRL0, 0x00 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FSCTRL1, 0x06 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.MDMCFG4, 0xCA });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.MDMCFG3, 0xBC });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.MDMCFG2, 0x06 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.MDMCFG1, 0x70 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.MDMCFG0, 0x11 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.DEVIATN, 0x44 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.MCSM0, 0x18 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FOCCFG, 0x17 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FSCAL3, 0xE9 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FSCAL2, 0x2A });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FSCAL1, 0x00 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FSCAL0, 0x1F });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.TEST1, 0x31 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.TEST0, 0x09 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.PATABLE0, 0x84 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.SYNC1, 0xA5 });
            await SendCommand(conn, RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.SYNC0, 0x5A });

            Console.WriteLine("Done initializing");
            _variablesInitialized = true;
        }

        public async Task<byte[]> GetPacket(uint timeout)
        {
            var conn = await GetRLConnection();
            if (conn == null)
                throw new Exception("Couldn't create a connection to RL");

            var cmdParams = new byte[] { 0 };
            await SendCommand(conn, RileyLinkCommandType.GetPacket, new byte[] { 0 });
            throw new NotImplementedException();
        }

        public async Task<byte[]> SendPacketAndExpectSilence(byte[] packetData)
        {
            throw new NotImplementedException();
        }

        public async Task<byte[]> SendPacketAndGetPacket(byte[] packetData)
        {
            throw new NotImplementedException();
        }

        public async Task SetLowTx()
        {
            throw new NotImplementedException();
        }

        public async Task SetNormalTx()
        {
            throw new NotImplementedException();
        }

        private async Task<byte[]> SendCommand(IBleGattServerConnection connection, RileyLinkCommandType cmd, byte[] cmdData = null, int timeout = 2000)
        {
            ResetTimer();
            byte[] data;
            if (cmdData == null)
            {
                data = new byte[] { 1, (byte)cmd };
            }
            else
            {
                data = new byte[cmdData.Length + 2];
                data[0] = (byte)(cmdData.Length + 1);
                data[1] = (byte)cmd;
                Buffer.BlockCopy(cmdData, 0, data, 2, cmdData.Length);
            }

            NotificationQueued.Reset();
            Console.WriteLine("writing data");
            await connection.WriteCharacteristicValue(RileyLinkServiceUUID, RileyLinkDataCharacteristicUUID, data);
            if (!NotificationQueued.Wait(timeout))
                throw new Exception("timed out expecting a response from rileylink");

            NotificationQueued.Reset();
            byte[] result = null;
            if (ResponseQueue.Count > 0)
            {
                result = ResponseQueue.Dequeue();
            }
            if (result == null || result.Length == 0)
                throw new Exception("RL returned no result");
            else if (result[0] == (byte)RileyLinkResponseType.OK)
            {
                if (result.Length > 1)
                {
                    var response = new byte[result.Length - 1];
                    Buffer.BlockCopy(result, 1, response, 0, response.Length);
                    return response;
                }
                else
                    return null;
            }
            else if (result[0] == (byte)RileyLinkResponseType.Interrupted)
            {
                return await SendCommand(connection, cmd, cmdData, timeout);
            }
            else
                throw new Exception($"RL returned error code {result[0]}");
        }

        private void ResetTimer()
        {
            _disconnectTimer?.Change(2500, Timeout.Infinite);
        }

        public Task SetHighTx()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetPacket(long timeout)
        {
            throw new NotImplementedException();
        }
    }
}
