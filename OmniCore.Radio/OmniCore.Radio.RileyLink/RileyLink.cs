using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OmniCore.Model.Eros.Data;
using System.Reactive.Threading.Tasks;
using OmniCore.Mobile.Base.Interfaces;
using OmniCore.Mobile.Base;

namespace OmniCore.Radio.RileyLink
{
    public class RileyLink
    {

        // [0x0E, 0x1D, 0x34, 0x2C, 0x60, 0x84, 0xC8]

        private Dictionary<TxPower, byte> PaDictionary = new Dictionary<TxPower, byte>()
            {
                { TxPower.A0_Lowest, 0x0E },   
                { TxPower.A1_VeryLow, 0x1D },
                { TxPower.A2_Low, 0x34 },
                { TxPower.A3_BelowNormal, 0x2c },
                { TxPower.A4_Normal, 0x60 },
                { TxPower.A5_High, 0x84 },
                { TxPower.A6_VeryHigh, 0xC8 },
            };

        private Guid RileyLinkServiceUUID = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");
        private Guid RileyLinkDataCharacteristicUUID = Guid.Parse("c842e849-5028-42e2-867c-016adada9155");
        private Guid RileyLinkResponseCharacteristicUUID = Guid.Parse("6e6c7910-b89e-43a5-a0fe-50c5e2b81f4a");

        private IDevice Device;
        private IGattCharacteristic DataCharacteristic;
        private IGattCharacteristic ResponseCharacteristic;

        private TxPower TxAmplification;
        private RileyLinkMessageExchange Exchange;

        public RileyLink(RileyLink copyFrom, RileyLinkMessageExchange exchange)
        {
            Exchange = exchange;
            TxAmplification = copyFrom.TxAmplification;
            Device = copyFrom.Device;
            DataCharacteristic = copyFrom.DataCharacteristic;
            ResponseCharacteristic = copyFrom.ResponseCharacteristic;
        }

        public RileyLink(RileyLinkMessageExchange exchange)
        {
            Exchange = exchange;
            TxAmplification = TxPower.A4_Normal;
        }

        private async Task<ErosRadioPreferences> GetPreferences()
        {
            var repo = await ErosRepository.GetInstance();
            return await repo.GetRadioPreferences();
        }

        private async Task<IDevice> ScanForDevice()
        {
            IDevice found = null;
            
            Exchange.ActionText = "Searching for RileyLink";
            var scanResults = new List<IScanResult>();
            var config = new ScanConfig() { ScanType = BleScanType.Balanced, ServiceUuids = new List<Guid>() { RileyLinkServiceUUID } };

            var scanExtension = new TaskCompletionSource<int>();

            var prefs = await GetPreferences();
            CrossBleAdapter.Current.Scan(config)
                .Subscribe((sr) =>
                {
                    if (!scanResults.Any(r => r.Device.Uuid == sr.Device.Uuid))
                    {
                        scanResults.Add(sr);
                        if (prefs.PreferredRadios != null && prefs.PreferredRadios.Contains(sr.Device.Uuid))
                        {
                            scanExtension.TrySetResult(0);
                        }
                        else if (prefs.ConnectToAny)
                        {
                            scanExtension.TrySetResult(2500);
                        }
                    }
                });

            var tr = await Task.WhenAny(scanExtension.Task, Task.Delay(20000)).ConfigureAwait(true);
            if (tr == scanExtension.Task)
            {
                var additionalDelay = await scanExtension.Task;
                if (additionalDelay > 0)
                    await Task.Delay(additionalDelay);
            }

            CrossBleAdapter.Current.StopScan();

            foreach (var result in scanResults.OrderByDescending(x => x.Rssi))
            {
                if (prefs.ConnectToAny || prefs.PreferredRadios.Contains(result.Device.Uuid))
                {
                    found = result.Device;
                    break;
                }
            }
            return found;
        }

        private async Task<IDevice> CheckIfAlreadyConnected()
        {
            var devices = await CrossBleAdapter.Current.GetConnectedDevices(RileyLinkServiceUUID);
            var prefs = await GetPreferences();
            foreach (var device in devices)
            {
                if (prefs.ConnectToAny || prefs.PreferredRadios.Contains(device.Uuid))
                {
                    return device;
                }
            }
            return null;
        }

        private async Task ConnectToDevice()
        {
            Exchange.ActionText = "Connecting to RileyLink";

            Device.Connect(new ConnectionConfig() { AndroidConnectionPriority = ConnectionPriority.High, AutoConnect = true });

            var t1 = Device.WhenConnected().FirstAsync().ToTask();
            var t2 = Device.WhenConnectionFailed().FirstAsync().ToTask();
            Task t3;
            t3 = Task.Delay(20000, Exchange.Token);

            var finishedTask = await Task.WhenAny(t1, t2, t3);
            if (finishedTask == t1)
            {
                Exchange.RileyLinkStatistics.RadioConnnected();

                Device.WhenDisconnected().Subscribe((_) =>
                {
                    Device = null;
                });

                Debug.WriteLine($"MTU Size: {Device.MtuSize}");
                var response = await Device.RequestMtu(185);
                Debug.WriteLine($"MTU req response: {response}");

                Exchange.ActionText = "Configuring RileyLink";
                DataCharacteristic = await Device.GetKnownCharacteristics(RileyLinkServiceUUID, RileyLinkDataCharacteristicUUID).ToTask();
                ResponseCharacteristic = await Device.GetKnownCharacteristics(RileyLinkServiceUUID, RileyLinkResponseCharacteristicUUID).ToTask();

                await ResponseCharacteristic.EnableNotifications().ToTask();
                await ConfigureDeviceSpecifics();
            }
            else if (finishedTask == t2)
            {
                this.Device = null;
                var err = await t2;
                Exchange.RileyLinkStatistics.RadioErrorOccured(err);
                Exchange.RileyLinkStatistics.RadioDisconnected();
                OmniCoreServices.Logger.Warning("connection failed", err);
                throw new OmniCoreRadioException(FailureType.RadioNotReachable, "Failed to connect to RL", err);
            }
            else
            {
                Device.CancelConnection();
                this.Device = null;
                OmniCoreServices.Logger.Warning("connection timed out");
                Exchange.RileyLinkStatistics.RadioDisconnected();
                throw new OmniCoreRadioException(FailureType.RadioNotReachable, "Timed out connecting to RL");
            }
        }

        public async Task EnsureDevice()
        {
            try
            {

                if (this.Device == null || !this.Device.IsConnected())
                {
                    Exchange.RileyLinkStatistics.RadioOverheadStart();
                    try
                    {
                        this.Device = null;
                        Device = await CheckIfAlreadyConnected() ?? await ScanForDevice() ??
                                throw new OmniCoreRadioException(FailureType.RadioNotReachable, "Couldn't find RileyLink!");

                        await ConnectToDevice();
                        if (!this.Device.IsConnected())
                            throw new OmniCoreRadioException(FailureType.RadioNotReachable, "Cannot connect to Rileylink");
                    }
                    finally
                    {
                        Exchange.RileyLinkStatistics.RadioOverheadEnd();
                    }
                }

                if (!Exchange.RileyLinkStatistics.MobileDeviceRssiAverage.HasValue)
                {
                        this.Device.ReadRssi()
                            .Subscribe((rssiRead) =>
                            {
                                Exchange.RileyLinkStatistics.MobileDeviceRssiReported(rssiRead);
                            });
                }
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new OmniCoreRadioException(FailureType.RadioNotReachable, "Error while connecting to BLE device", e);
            }
            finally
            {
            }
        }

        public async Task<Bytes> GetPacket(uint timeout = 5000)
        {
            try
            {
                await EnsureDevice();
                var cmdParams = new Bytes((byte)0);
                cmdParams.Append(timeout);

                var result = await this.SendCommand(RileyLinkCommandType.SendAndListen, cmdParams, (int)timeout + 500);
                if (result != null)
                {
                    return result.Sub(0, 2).Append(ManchesterEncoding.Decode(result.Sub(2)));
                }
                else
                    return null;
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new OmniCoreRadioException(FailureType.RadioDisconnectPrematurely, "Error while receiving data with RL", e);
            }
        }

        public async Task SendPacket(
            Bytes packet, byte repeat_count, ushort delay_ms, ushort preamble_ext_ms)
        {
            try
            {
                await EnsureDevice();
                Debug.WriteLine($"SEND radio packet: {packet}");
                var data = ManchesterEncoding.Encode(packet);
                var cmdParams = new Bytes((byte)0).Append(repeat_count);
                cmdParams.Append(delay_ms);
                cmdParams.Append(preamble_ext_ms);
                cmdParams.Append(data);
                await this.SendCommand(RileyLinkCommandType.SendAndListen, cmdParams, 30000);
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new OmniCoreRadioException(FailureType.RadioDisconnectPrematurely, "Error while sending data with RL", e);
            }
        }

        public async Task<Bytes> SendAndGetPacket(
            Bytes packet, byte repeat_count, ushort delay_ms, uint timeout_ms, byte retry_count, ushort preamble_ext_ms)
        {
            try
            {
                await EnsureDevice();
                var data = ManchesterEncoding.Encode(packet);
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
                    var decoded = ManchesterEncoding.Decode(result.Sub(2));
                    return result.Sub(0, 2).Append(decoded);
                }
                else
                    return null;
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new OmniCoreRadioException(FailureType.RadioDisconnectPrematurely, "Error while sending and receiving data with RL", e);
            }
        }

        public async Task SetTxLevel(TxPower txPower)
        {
            await EnsureDevice();
            TxAmplification = txPower;
            Exchange.RileyLinkStatistics.RadioOverheadStart();
            await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.PATABLE0, PaDictionary[txPower] });
            Exchange.RileyLinkStatistics.RadioOverheadEnd();
            Exchange.RileyLinkStatistics.RadioTxLevelChange(txPower);
        }

        public async Task TxLevelDown()
        {
            await EnsureDevice();
            if (TxAmplification > TxPower.A0_Lowest)
            {
                TxAmplification--;
                await SetTxLevel(TxAmplification);
            }
        }

        public async Task TxLevelUp()
        {
            await EnsureDevice();
            if (TxAmplification < TxPower.A6_VeryHigh)
            {
                TxAmplification++;
                await SetTxLevel(TxAmplification);
            }
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

                var result = await WriteAndRead(data, timeout);

                if (result == null || result.Length == 0)
                    throw new OmniCoreRadioException(FailureType.RadioDisconnectPrematurely, "RL returned no result");

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
                else if (result[0] == (byte)RileyLinkResponseType.Timeout)
                    throw new OmniCoreTimeoutException(FailureType.RadioRecvTimeout);
                else
                    throw new OmniCoreRadioException(FailureType.RadioUnknownError, $"RL returned error code {result[0]}");
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new OmniCoreRadioException(FailureType.RadioDisconnectPrematurely, "Error while sending a command via BLE", e);
            }
        }

        private async Task<byte[]> WriteAndRead(byte[] dataToWrite, int timeout = 5000)
        {
            try
            {
                var tc = new TaskCompletionSource<CharacteristicGattResult>();
                ResponseCharacteristic.WhenNotificationReceived().Subscribe(result =>
                {
                    tc.TrySetResult(result);
                });

                try
                {
                    await DataCharacteristic.Write(dataToWrite);
                    await tc.Task;
                    var readResult = await DataCharacteristic.Read().Timeout(TimeSpan.FromMilliseconds(timeout));
                    if (readResult == null)
                    {
                        throw new OmniCoreRadioException(FailureType.RadioRecvTimeout);
                    }
                    return readResult.Data;
                }
                catch(TimeoutException)
                {
                    throw new OmniCoreRadioException(FailureType.RadioRecvTimeout);
                }
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new OmniCoreRadioException(FailureType.RadioDisconnectPrematurely, "Error while writing to and reading from RL", e);
            }
        }

        private async Task InitializeRadio()
        {
            try
            {
                //byte[] response;
                //if (WorkaroundRequired)
                //    response = await SendCommand(RileyLinkCommandType.ReadRegister, new byte[] { 0, (byte)RileyLinkRegister.PKTLEN });
                //else
                //    response = await SendCommand(RileyLinkCommandType.ReadRegister, new byte[] { (byte)RileyLinkRegister.PKTLEN });

                //if (response != null && response.Length > 0 && response[0] == 0x4e)
                //{
                //    Debug.WriteLine("Radio configuration verified");
                //}
                await SendCommand(RileyLinkCommandType.ResetRadioConfig);
                await SendCommand(RileyLinkCommandType.SetSwEncoding, new byte[] { (byte)RileyLinkSoftwareEncoding.None });
                await SendCommand(RileyLinkCommandType.SetPreamble, new byte[] { 0x66, 0x65 });

                var frequency = (int)(433910000 / (24000000 / Math.Pow(2, 16)));
                await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREQ0, (byte)(frequency & 0xff) });
                await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREQ1, (byte)((frequency >> 8) & 0xff) });
                await SendCommand(RileyLinkCommandType.UpdateRegister, new byte[] { (byte)RileyLinkRegister.FREQ2, (byte)((frequency >> 16) & 0xff) });
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
                Debug.WriteLine("Initialization completed.");

                var result = await SendCommand(RileyLinkCommandType.GetState);
                if (result.Length != 2 || result[0] != 'O' || result[1] != 'K')
                    throw new OmniCoreRadioException(FailureType.RadioStateError, "RL returned status not OK.");
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new OmniCoreRadioException(FailureType.RadioUnknownError, "Error while initializing radio", e);
            }
        }

        //private HashSet<Guid> ConfiguredDevices = new HashSet<Guid>();

        private async Task ConfigureDeviceSpecifics()
        {
            //if (!ConfiguredDevices.Contains(Device.Uuid))
            //{
                await DataCharacteristic.Write(new byte[] { 0 });
                while (true)
                {
                    try
                    {
                        await ResponseCharacteristic.WhenNotificationReceived().Timeout(TimeSpan.FromMilliseconds(150));
                        await DataCharacteristic.Read().Timeout(TimeSpan.FromMilliseconds(200));
                        await ResponseCharacteristic.Read().Timeout(TimeSpan.FromMilliseconds(200));
                    }
                    catch (TimeoutException)
                    {
                        break;
                    }
                }

                await VerifyVersion();
                await InitializeRadio();
            //    ConfiguredDevices.Add(Device.Uuid);
            //}
        }

        private async Task VerifyVersion()
        {
            try
            {
                var result = await SendCommand(RileyLinkCommandType.GetState);
                if (result == null || result.Length != 2 || result[0] != 'O' || result[1] != 'K')
                    throw new OmniCoreRadioException(FailureType.RadioStateError, "RL returned status not OK.");

                var versionData = await SendCommand(RileyLinkCommandType.GetVersion);
                if (versionData != null && versionData.Length > 0)
                {
                    var versionString = Encoding.ASCII.GetString(versionData);
                    Debug.WriteLine($"RL reports version string: {versionString}");
                    var m = Regex.Match(versionString, ".+([0-9]+)\\.([0-9]+)");
                    var v_major = int.Parse(m.Groups[1].ToString());
                    var v_minor = int.Parse(m.Groups[2].ToString());

                    if (v_major < 2)
                        throw new OmniCoreRadioException(FailureType.RadioStateError, "Firmware Version below 2, cannot be used for omnipod.");

                    //if (v_major == 2 && v_minor < 3)
                    //    this.WorkaroundRequired = true;
                }
                else
                    throw new OmniCoreRadioException(FailureType.RadioStateError, "Version info couldn't be obtained from RL");
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new OmniCoreRadioException(FailureType.RadioStateError, "Error verifying RL version", e);
            }
        }
    }
}
