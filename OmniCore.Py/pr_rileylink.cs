using nexus.core;
using nexus.protocols.ble;
using nexus.protocols.ble.scan;
using Omni.Py;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Py
{
    public class PrRileyLink : IPacketRadio
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
        private IBleGattServerConnection GattServerConnection;
        private TaskCompletionSource<byte[]> MessageCounterNotifier;
        private Action<Guid, byte[]> MessageCounterNotifierAction;
        private IDisposable MessageCounterObserver;

        private Timer DisconnectTimer;

        private IBlePeripheral Peripheral;
        private bool VersionVerified;
        private bool WorkaroundRequired;
        private bool RadioInitialized;


        public PrRileyLink(IBluetoothLowEnergyAdapter ble)
        {
            this.Ble = ble;
            this.Logger = definitions.getLogger();
        }

        public async Task reset()
        {
            try
            {
                await this.Disconnect();
                this.RadioInitialized = false;

                //if (this.Ble.AdapterCanBeDisabled && this.Ble.AdapterCanBeEnabled)
                //{
                //    await this.Ble.DisableAdapter();
                //    await this.Ble.EnableAdapter();
                //}

                await this.InitializeRadio();
            }
            catch (Exception)
            {
                // ignoring reset errors
            }
        }
        public async Task<Bytes> get_packet(uint timeout = 5000)
        {
            try
            {
                var cmdParams = new Bytes((byte)0);
                cmdParams.Append(timeout);

                var result = await this.SendCommand(RileyLinkCommandType.SendAndListen, cmdParams, (int)timeout + 500);
                if (result != null)
                {
                    return result.Sub(0, 2).Append(manchester.Decode(result.Sub(2).ToArray()));
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                throw new PacketRadioError("Error while receiving data with RL", e);
            }
        }

        public async Task send_packet(Bytes packet, byte repeat_count, ushort delay_ms, ushort preamble_ext_ms)
        {
            try
            {
                Debug.WriteLine($"SEND radio packet: {packet}");
                var data = manchester.Encode(packet.ToArray());
                var cmdParams = new Bytes((byte)0).Append(repeat_count);
                cmdParams.Append(delay_ms);
                cmdParams.Append(preamble_ext_ms);
                cmdParams.Append(data);
                await this.SendCommand(RileyLinkCommandType.SendAndListen, cmdParams, 30000);
            }
            catch (Exception e)
            {
                throw new PacketRadioError("Error while sending data with RL", e);
            }
        }

        public async Task<Bytes> send_and_receive_packet(Bytes packet, byte repeat_count, ushort delay_ms, uint timeout_ms, byte retry_count, ushort preamble_ext_ms)
        {
            try
            {
                Debug.WriteLine($"SEND radio packet: {packet}");
                var data = manchester.Encode(packet.ToArray());
                var cmdParams = new Bytes()
                    .Append((byte)0)
                    .Append(repeat_count)
                    .Append(delay_ms)
                    .Append((byte)0)
                    .Append(timeout_ms)
                    .Append(retry_count)
                    .Append(preamble_ext_ms)
                    .Append(data);

                var result = await this.SendCommand(RileyLinkCommandType.SendAndListen, cmdParams, 5000);
                if (result != null)
                {
                    var decoded = new Bytes(manchester.Decode(result.Sub(2).ToArray()));
                    Debug.WriteLine($"RECV radio packet: {decoded.ToHex()}");
                    return result.Sub(0, 2).Append(decoded);
                }
                else
                    return null;
            }
            catch(Exception e)
            {
                throw new PacketRadioError("Error while sending and receiving data with RL", e);
            }
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

        private void StartTicking()
        {
            //this.DisconnectTimer?.Change(3000, Timeout.Infinite);
        }

        private void StopTicking()
        {
            //this.DisconnectTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private async Task Disconnect()
        {
            this.Logger.log("Disconnecting from RL");
            //this.DisconnectTimer?.Dispose();
            //this.DisconnectTimer = null;

            this.MessageCounterObserver?.Dispose();
            this.MessageCounterNotifier?.TrySetCanceled();

            this.MessageCounterObserver = null;
            this.MessageCounterNotifier = null;

            if (this.GattServerConnection != null && this.GattServerConnection.State != ConnectionState.Disconnected)
            {
                try
                {
                    await this.GattServerConnection.Disconnect();
                }
                catch { }
            }
            this.GattServerConnection = null;
        }

        private async Task<Bytes> SendCommand(RileyLinkCommandType cmd, Bytes cmdData, int timeout = 2000)
        {
            return new Bytes(await SendCommand(cmd, cmdData.ToArray(), timeout));
        }

        private async Task<byte[]> SendCommand(RileyLinkCommandType cmd, byte[] cmdData = null, int timeout = 5000)
        {
            try
            {
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

                var result = await WriteAndRead(data, false, timeout);

                if (result == null || result.Length == 0)
                    throw new PacketRadioError("RL returned no result");

                else if (result[0] == (byte)RileyLinkResponseType.OK
                    || result[0] == (byte)RileyLinkResponseType.Interrupted)
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
                else
                    throw new PacketRadioError($"RL returned error code {result[0]}");
            }
            catch(PacketRadioError)
            {
                throw;
            }
            catch(Exception e)
            {
                throw new PacketRadioError("Error while sending a command via BLE", e);
            }
        }

        private async Task<byte[]> WriteAndRead(byte[] dataToWrite, bool noWait = false, int timeout = 5000)
        {
            try
            {
                var conn = await SetupConnection();
                StopTicking();
                Debug.WriteLine($"Write {BitConverter.ToString(dataToWrite)}");
                if (!noWait)
                    this.MessageCounterNotifier = new TaskCompletionSource<byte[]>();
                await conn.WriteCharacteristicValue(RileyLinkServiceUUID, RileyLinkDataCharacteristicUUID, dataToWrite);
                Debug.WriteLine($"Written");
                if (!noWait)
                {
                    await this.MessageCounterNotifier.Task;
                    Debug.WriteLine($"Reading");
                    var read = await conn.ReadCharacteristicValue(RileyLinkServiceUUID, RileyLinkDataCharacteristicUUID);
                    Debug.WriteLine($"Read {BitConverter.ToString(read)}");
                    return read;
                }
                return null;
            }
            catch(PacketRadioError)
            {
                throw;
            }
            catch(Exception e)
            {
                throw new PacketRadioError("Error while writing to and reading from RL");
            }
            finally
            {
                //this.MessageCounterNotifier = null;
                StartTicking();
            }
        }

        private void CreateNotifyObserver()
        {
            this.MessageCounterNotifier?.TrySetCanceled();
            this.MessageCounterNotifier = null;
            this.MessageCounterObserver?.Dispose();
            this.MessageCounterObserver = null;

            if (this.MessageCounterNotifierAction == null)
                this.MessageCounterNotifierAction = new Action<Guid, byte[]>(
                    (g, data) => { this.MessageCounterNotifier?.TrySetResult(data); }
                    );

            this.MessageCounterObserver = this.GattServerConnection.NotifyCharacteristicValue(RileyLinkServiceUUID, RileyLinkResponseCharacteristicUUID,
                                                this.MessageCounterNotifierAction);
        }

        private async Task<IBleGattServerConnection> SetupConnection()
        {
            if (this.GattServerConnection != null && this.GattServerConnection.State == ConnectionState.Connected)
                return this.GattServerConnection;

            if (this.GattServerConnection != null && this.GattServerConnection.State != ConnectionState.Connected)
            {
                await Disconnect();
            }

            try
            {
                this.GattServerConnection = await GetConnection();

                CreateNotifyObserver();

                //if (this.DisconnectTimer != null)
                //{
                //    this.DisconnectTimer.Dispose();
                //    this.DisconnectTimer = null;
                //}

                if (!this.VersionVerified)
                {
                    await this.VerifyVersion();
                }

                if (!this.RadioInitialized)
                {
                    await this.InitializeRadio();
                }

                //this.DisconnectTimer = new Timer(async (state) =>
                //{
                //    if (this.GattServerConnection != null && this.GattServerConnection.State == ConnectionState.Connected)
                //        await this.Disconnect();
                //}, null, Timeout.Infinite, Timeout.Infinite);

            }
            catch (Exception e)
            {
                await Disconnect();
                throw new PacketRadioError("Failed to set up the BLE connection", e);
            }
            return this.GattServerConnection;
        }
        private async Task InitializeRadio()
        {
            this.Logger.log("Initializing radio variables");
            await SendCommand(RileyLinkCommandType.ResetRadioConfig);
            await SendCommand(RileyLinkCommandType.SetSwEncoding, new byte[] { (byte) RileyLinkSoftwareEncoding.None });
            await SendCommand(RileyLinkCommandType.SetPreamble, new byte[] { 0x66, 0x65 });

            //var frequency = (int)(433910000 / (24000000 / Math.Pow(2, 16)));
            //await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREQ0, (byte)(frequency & 0xff) });
            //await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREQ1, (byte)((frequency >> 8) & 0xff) });
            //await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREQ2, (byte)((frequency >> 16) & 0xff) });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREQ0, 0x5f });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREQ1, 0x14 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREQ2, 0x12 });

            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.DEVIATN, 0x44 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.PKTCTRL1, 0x20 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.PKTCTRL0, 0x00 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.PKTLEN, 0x50 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FSCTRL1, 0x06 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.MDMCFG4, 0xCA });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.MDMCFG3, 0xBC });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.MDMCFG2, 0x06 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.MDMCFG1, 0x70 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.MDMCFG0, 0x11 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.MCSM0, 0x18 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FOCCFG, 0x17 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FSCAL3, 0xE9 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FSCAL2, 0x2A });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FSCAL1, 0x00 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FSCAL0, 0x1F });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.TEST1, 0x35 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.TEST0, 0x09 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.PATABLE0, 0x84 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREND0, 0x00 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.SYNC1, 0xA5 });
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.SYNC0, 0x5A });

            var result = await SendCommand(RileyLinkCommandType.GetState);
            if (result.Length != 2 || result[0] != 'O' || result[1] != 'K')
                throw new PacketRadioError("RL returned status not OK.");

            this.Logger.log("Initialization completed.");
            this.RadioInitialized = true;
        }

        private async Task VerifyVersion()
        {
            this.Logger.log("Verifying RL version");
            try
            {
                var versionData = await SendCommand(RileyLinkCommandType.GetVersion);
                if (versionData != null && versionData.Length > 0)
                {
                    var versionString = Encoding.ASCII.GetString(versionData);
                    this.Logger.log($"RL reports version string: {versionString}");
                    var m = Regex.Match(versionString, ".+([0-9]+)\\.([0-9]+)");
                    var v_major = int.Parse(m.Groups[1].ToString());
                    var v_minor = int.Parse(m.Groups[2].ToString());

                    if (v_major < 2)
                        throw new PacketRadioError("Firmware Version below 2, cannot be used for omnipod.");

                    if (v_major == 2 && v_minor < 3)
                        this.WorkaroundRequired = true;

                    this.VersionVerified = true;
                }
                else
                    throw new PacketRadioError("Version info couldn't be obtained from RL");
            }
            catch (PacketRadioError) { throw; }
            catch (Exception e)
            {
                throw new PacketRadioError("Error verifying RL version", e);
            }
        }

        private async Task<IBleGattServerConnection> GetConnection()
        {
            try
            {
                if (this.Ble.CurrentState.IsDisabledOrDisabling() && this.Ble.AdapterCanBeEnabled)
                {
                    this.Logger.log("Enabling BLE adapter");
                    await this.Ble.EnableAdapter();
                }
            }
            catch(Exception e)
            {
                throw new PacketRadioError("Failed to enable BLE adapter", e);
            }

            if (this.Peripheral == null)
            {
                this.VersionVerified = false;
                this.RadioInitialized = false;
                var p = await FindRileyLink();
                if (p == null)
                    throw new PacketRadioError("Couldn't find RileyLink!");
                this.Peripheral = p;
            }

            try
            {
                var connection = await this.Ble.ConnectToDevice(this.Peripheral);
                if (connection.IsSuccessful())
                {

                    return connection.GattServer;
                }
                else
                {
                    throw new PacketRadioError("Failed to establish BLE connection");
                }
            }
            catch (PacketRadioError) { throw; }
            catch (Exception e)
            {
                throw new PacketRadioError("Error while connecting to BLE device", e);
            }
        }

        private async Task<IBlePeripheral> FindRileyLink()
        {

            try
            {
                IBlePeripheral pFound = null;
                using (var cts = new CancellationTokenSource(15000))
                {
                    this.Logger.log("Scanning for RileyLink");
                    await this.Ble.ScanForBroadcasts(
                        new ScanSettings()
                        {
                            Mode = ScanMode.LowPower,
                            IgnoreRepeatBroadcasts = true,
                            Filter = new ScanFilter()
                            {
                                AdvertisedServiceIsInList = new List<Guid>() { RileyLinkServiceUUID },
                            }
                        },
                        (peripheral) =>
                        {
                            this.Logger.log($"Found RL at address {peripheral.Address}, name: {peripheral.Advertisement.DeviceName}");
                            pFound = peripheral;
                            cts.Cancel();
                        }, cts.Token);
                }
                return pFound;
            }
            catch (TaskCanceledException)
            {
                throw new PacketRadioError("Timed out while searching for RL");
            }
            catch(Exception e)
            {
                throw new PacketRadioError("Error during BLE scan", e);
            }
        }
    }

    public enum RileyLinkCommandType
    {
        GetState = 1,
        GetVersion = 2,
        GetPacket = 3,
        SendPacket = 4,
        SendAndListen = 5,
        UpdateRegister = 6,
        Reset = 7,
        Led = 8,
        ReadRegister = 9,
        SetModeRegisters = 10,
        SetSwEncoding = 11,
        SetPreamble = 12,
        ResetRadioConfig = 13,

    }

    public enum RileyLinkRegister : byte
    {
        SYNC1 = 0x00,
        SYNC0 = 0x01,
        PKTLEN = 0x02,
        PKTCTRL1 = 0x03,
        PKTCTRL0 = 0x04,
        FSCTRL1 = 0x07,
        FREQ2 = 0x09,
        FREQ1 = 0x0a,
        FREQ0 = 0x0b,
        MDMCFG4 = 0x0c,
        MDMCFG3 = 0x0d,
        MDMCFG2 = 0x0e,
        MDMCFG1 = 0x0f,
        MDMCFG0 = 0x10,
        DEVIATN = 0x11,
        MCSM0 = 0x14,
        FOCCFG = 0x15,
        AGCCTRL2 = 0x17,
        AGCCTRL1 = 0x18,
        AGCCTRL0 = 0x19,
        FREND1 = 0x1a,
        FREND0 = 0x1b,
        FSCAL3 = 0x1c,
        FSCAL2 = 0x1d,
        FSCAL1 = 0x1e,
        FSCAL0 = 0x1f,
        TEST1 = 0x24,
        TEST0 = 0x25,
        PATABLE0 = 0x2e
    }

    public enum RileyLinkResponseType
    {
        Timeout = 0xaa,
        Interrupted = 0xbb,
        NoData = 0xcc,
        OK = 0xdd
    }

    public enum RileyLinkSoftwareEncoding
    {
        None = 0,
        Manchester = 1,
        FourBySix = 2
    }
}
