using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Services.Interfaces.Radio;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using Polly;
using Polly.Timeout;
using Trace = System.Diagnostics.Trace;

namespace OmniCore.Services;

public class Radio : IRadio
{
    private readonly AsyncLock _allocationLock = new();
    private readonly Task _connectionLoopTask;
    private readonly AsyncAutoResetEvent _connectionLostEvent = new(false);

    private CancellationTokenSource? _connectionTaskCancellation = new();
    private readonly AsyncManualResetEvent _radioReadyEvent = new(false);

    private readonly AsyncManualResetEvent _responseCountUpdatedEvent = new(false);
    private readonly AsyncAutoResetEvent _rssiRequestedEvent = new(false);

    private IDevice _device;
    private IService _mainService;
    private ICharacteristic _responseCountCharacteristic;
    private ICharacteristic _dataCharacteristic;
    private TaskCompletionSource<int?> _rssiSource;

    public Radio(Guid id, string name)
    {
        Id = id;
        Name = name;
        Rssi = null;
        _connectionLoopTask = Task.Run(() =>
            ConnectionLoop(_connectionTaskCancellation.Token));
    }

    public Guid Id { get; }
    public string Name { get; }
    public int? Rssi { get; private set; }

    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken)
    {
        return await _allocationLock.LockAsync(cancellationToken);
    }

    public void Dispose()
    {
        _connectionTaskCancellation?.Cancel();
        try
        {
            _connectionLoopTask.GetAwaiter().GetResult();
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error disposing radio: {ex}");
        }
        finally
        {
            _connectionTaskCancellation?.Dispose();
            _connectionTaskCancellation = null;
        }
    }

    public async Task UpdateRssiAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Rssi = null;
            _rssiSource = new TaskCompletionSource<int?>();
            cancellationToken.Register(() => _rssiSource.TrySetCanceled());
            _rssiRequestedEvent.Set();
            Rssi = await _rssiSource.Task;
        }
        finally
        {
            _rssiSource = null;
        }
    }

    private async Task ConnectionTask(CancellationToken cancellationToken)
    {
        var adapter = CrossBluetoothLE.Current.Adapter;
    }
    private async Task<IDevice> TryConnectAsync(CancellationToken cancellationToken)
    {
        var retryPolicy = Policy
            .Handle<Exception>(e => !(e is TaskCanceledException))
            .WaitAndRetryForeverAsync(i =>
            {
                if (i < 10)
                    return TimeSpan.FromSeconds(i * 30);
                return TimeSpan.FromSeconds(10 * 30);
            });
        
        var timeoutPolicy = Policy.TimeoutAsync(30, TimeoutStrategy.Optimistic);
        var policy = retryPolicy.WrapAsync(timeoutPolicy);

        return await policy.ExecuteAsync(ct => CrossBluetoothLE.Current.Adapter
            .ConnectToKnownDeviceAsync(Id,
                new ConnectParameters(false, true),
                ct), cancellationToken);
    }

    private async Task<IDevice> TryConnectForeverAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var adapter = CrossBluetoothLE.Current.Adapter;
            IDevice device = null;
            try
            {
                if (CrossBluetoothLE.Current.State == BluetoothState.On)
                {
                    using var ctt = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    var timeoutToken = ctt.Token;
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken);
                    Debug.WriteLine($"{Name} connecting");
                    device = await adapter.ConnectToKnownDeviceAsync(Id,
                        new ConnectParameters(false, true),
                        cts.Token);
                    Debug.WriteLine($"{Name} connected");
                    return device;
                }
                else
                {
                    Debug.WriteLine($"{Name} waiting for bluetooth");
                    await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                device?.Dispose();
                throw;
            }
            catch (Exception e)
            {
                device?.Dispose();
                Debug.WriteLine($"{Name} connection failed, retrying. {e}");
                await Task.Delay(TimeSpan.FromSeconds(15));
            }
        }
    }

    private async Task ConnectionLoop(CancellationToken cancellationToken)
    {
        var adapter = CrossBluetoothLE.Current.Adapter;
        adapter.DeviceConnectionLost += AdapterOnDeviceConnectionLost;
        try
        {
            while (true)
            {
                _radioReadyEvent.Reset();
                _device = null;
                _mainService = null;
                _dataCharacteristic = null;
                _responseCountCharacteristic = null;
                try
                {
                    _device = await TryConnectForeverAsync(cancellationToken);
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    _mainService = await _device.GetServiceAsync(RileyLinkGatt.ServiceMain, cancellationToken);
                    _dataCharacteristic =
                        await _mainService.GetCharacteristicAsync(RileyLinkGatt.ServiceMainCharData);
                    _responseCountCharacteristic =
                        await _mainService.GetCharacteristicAsync(RileyLinkGatt.ServiceMainCharResponseCount);
                    Debug.WriteLine($"{Name} got chars");

                    //var ledCharacteristic = await _mainService.GetCharacteristicAsync(RileyLinkGatt.ServiceMainCharLedMode);
                    //await ledCharacteristic.WriteAsync(new byte[] { 0x01, 0x01 }, cancellationToken);
                    //Debug.WriteLine($"led write complete");
                    
                    _responseCountCharacteristic.ValueUpdated += ResponseCountCharacteristicOnValueUpdated;
                    await _responseCountCharacteristic.StartUpdatesAsync(cancellationToken);

                    var mtu = await _device.RequestMtuAsync(128);
                    Debug.WriteLine($"{Name} got mtu: {mtu}");

                    // for (int i = 0; i < 4; i++)
                    // {
                    //     var write_data = new byte[] { 1, 1 };
                    //     Debug.WriteLine($"{Name} radio write {write_data.ToHexString()}");
                    //     await TryWriteToCharacteristic(_dataCharacteristic,
                    //         write_data,
                    //         cancellationToken);
                    //     await Task.Delay(150);
                    //     var read_data = await TryReadFromCharacteristic(_dataCharacteristic,
                    //         cancellationToken);
                    //     Debug.WriteLine($"{Name} radio data {read_data.ToHexString()}");
                    // }

                    Debug.WriteLine($"{Name} reset radio");

                    await TryWriteToCharacteristic(_dataCharacteristic,
                        new byte[] { 1, (byte)RileyLinkCommand.Reset, 0 }, cancellationToken);

                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                    Debug.WriteLine($"{Name} radio reset");

                    _responseCountUpdatedEvent.Reset();
                    Debug.WriteLine($"{Name} start initializing");
                    await InitializeRadioParametersAsync(cancellationToken);
                    Debug.WriteLine($"{Name} radio initialized.");

                    _radioReadyEvent.Set();
                    var connLostTask = _connectionLostEvent.WaitAsync(cancellationToken);
                    var rssiRequestTask = _rssiRequestedEvent.WaitAsync(cancellationToken);

                    while (true)
                    {
                        var waitResult = Task.WhenAny(connLostTask, rssiRequestTask);
                        if (waitResult == rssiRequestTask)
                        {
                            await rssiRequestTask;
                            if (await _device.UpdateRssiAsync())
                                _rssiSource.TrySetResult(_device.Rssi);
                            else
                                _rssiSource.TrySetResult(null);
                        }
                        else
                        {
                            await connLostTask;
                            break;
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Trace.WriteLine($"Error in radio loop: {e}");
                }
                finally
                {
                    _radioReadyEvent.Reset();
                    if (_responseCountCharacteristic != null)
                        _responseCountCharacteristic.ValueUpdated -= ResponseCountCharacteristicOnValueUpdated;
                    _device?.Dispose();
                    _device = null;
                    _responseCountCharacteristic = null;
                    _dataCharacteristic = null;
                    _mainService = null;
                }

                Trace.WriteLine("Restarting radio connection in 5 seconds..");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        finally
        {
            adapter.DeviceConnectionLost -= AdapterOnDeviceConnectionLost;
        }
    }

    private void AdapterOnDeviceConnectionLost(object sender, DeviceErrorEventArgs e)
    {
        if (e.Device.Id == Id)
        {
            Debug.WriteLine($"Radio {Name} connection lost! {e.ErrorMessage}");
            _connectionLostEvent.Set();
        }
    }

    private void ResponseCountCharacteristicOnValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
    {
        //Debug.WriteLine($"Response count value updated: {e.Characteristic.Value.ToHexString()}");
        _responseCountUpdatedEvent.Set();
    }

    private void AssertRadioReturnResult(BleExchangeResult result)
    {
        if (result.CommunicationResult != BleCommunicationResult.OK ||
            result.ResponseCode != RileyLinkResponse.CommandSuccess)
            throw new ApplicationException($"BLE comm error");
    }

    private async Task InitializeRadioParametersAsync(CancellationToken cancellationToken = default)
    {
        AssertRadioReturnResult(await ExecuteCommandInternalAsync(
            RileyLinkCommand.RadioResetConfig, cancellationToken));
        AssertRadioReturnResult(await ExecuteCommandInternalAsync(
            RileyLinkCommand.SetSwEncoding, cancellationToken, 0));
        AssertRadioReturnResult(await ExecuteCommandInternalAsync(
            RileyLinkCommand.SetPreamble,
            cancellationToken, 0x66, 0x65));

        var commonRegisters = new Dictionary<RileyLinkRadioRegister, byte>
        {
            { RileyLinkRadioRegister.SYNC1, 0xA5 },
            { RileyLinkRadioRegister.SYNC0, 0x5A },
            { RileyLinkRadioRegister.PKTLEN, 0x50 },
            { RileyLinkRadioRegister.PKTCTRL1, 0x20 },
            { RileyLinkRadioRegister.PKTCTRL0, 0x00 },
            { RileyLinkRadioRegister.ADDR, 0x00 },
            { RileyLinkRadioRegister.CHANNR, 0x00 },

            { RileyLinkRadioRegister.FSCTRL1, 0x0F },
            { RileyLinkRadioRegister.FSCTRL0, 0x00 },

            { RileyLinkRadioRegister.MDMCFG4, 0xBA },
            { RileyLinkRadioRegister.MDMCFG3, 0xB9 },
            { RileyLinkRadioRegister.MDMCFG2, 0x12 },
            { RileyLinkRadioRegister.MDMCFG1, 0x43 },
            { RileyLinkRadioRegister.MDMCFG0, 0x11 },

            { RileyLinkRadioRegister.MCSM2, 0x07 },
            { RileyLinkRadioRegister.MCSM1, 0x30 },
            { RileyLinkRadioRegister.MCSM0, 0x19 },

            { RileyLinkRadioRegister.FOCCFG, 0x17 },
            { RileyLinkRadioRegister.BSCFG, 0x6C },
            { RileyLinkRadioRegister.AGCCTRL2, 0x43 },
            { RileyLinkRadioRegister.AGCCTRL1, 0x40 },
            { RileyLinkRadioRegister.AGCCTRL0, 0x91 },
            { RileyLinkRadioRegister.FREND1, 0x56 },
            { RileyLinkRadioRegister.FREND0, 0x10 },

            { RileyLinkRadioRegister.FSCAL3, 0xE9 },
            { RileyLinkRadioRegister.FSCAL2, 0x2A },
            { RileyLinkRadioRegister.FSCAL1, 0x00 },
            { RileyLinkRadioRegister.FSCAL0, 0x1F },
            { RileyLinkRadioRegister.TEST1, 0x31 },
            { RileyLinkRadioRegister.TEST0, 0x09 },

// 0xC0 +10
// 0xC8 +7
// 0x84 +5
// 0x60 0
// 0x62 -1
// 0x2C -5
// 0x34 -10
// 0x1D -15
// 0x0E -20
// 0x12 -30        
            { RileyLinkRadioRegister.PA_TABLE0, 0x60 }
        };

        var rxRegisters = new Dictionary<RileyLinkRadioRegister, byte>
        {
            { RileyLinkRadioRegister.DEVIATN, 0x46 },
            { RileyLinkRadioRegister.FREQ2, 0x12 },
            { RileyLinkRadioRegister.FREQ1, 0x14 },
            { RileyLinkRadioRegister.FREQ0, 0x77 }
        };

        var txRegisters = new Dictionary<RileyLinkRadioRegister, byte>
        {
            { RileyLinkRadioRegister.DEVIATN, 0x50 },
            { RileyLinkRadioRegister.FREQ2, 0x12 },
            { RileyLinkRadioRegister.FREQ1, 0x14 },
            { RileyLinkRadioRegister.FREQ0, 0x56 }
        };

        foreach (var rkv in commonRegisters)
            AssertRadioReturnResult(await ExecuteCommandInternalAsync(
                RileyLinkCommand.UpdateRegister, cancellationToken,
                (byte)rkv.Key, rkv.Value));

        var txParams = new byte[txRegisters.Count * 2 + 1];
        txParams[0] = 0x01;
        var idx = 1;
        foreach (var rkv in txRegisters)
        {
            txParams[idx] = (byte)rkv.Key;
            txParams[idx + 1] = rkv.Value;
            idx += 2;
        }

        AssertRadioReturnResult(await ExecuteCommandInternalAsync(
            RileyLinkCommand.SetModeRegisters, cancellationToken, txParams));

        var rxParams = new byte[rxRegisters.Count * 2 + 1];
        rxParams[0] = 0x02;
        idx = 1;
        foreach (var rkv in rxRegisters)
        {
            rxParams[idx] = (byte)rkv.Key;
            rxParams[idx + 1] = rkv.Value;
            idx += 2;
        }

        AssertRadioReturnResult(await ExecuteCommandInternalAsync(
            RileyLinkCommand.SetModeRegisters, cancellationToken, rxParams));
    }

    public async Task<BleExchangeResult> ExecuteCommandAsync(
        RileyLinkCommand command,
        CancellationToken cancellationToken,
        params byte[] data)
    {
        while (true)
        {
            try
            {
                await _radioReadyEvent.WaitAsync(cancellationToken);
                return await ExecuteCommandInternalAsync(command, cancellationToken, data);
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while bt comm: {e}");
                _radioReadyEvent.Reset();
                _connectionLostEvent.Set();
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
    
    private async Task<BleExchangeResult> ExecuteCommandInternalAsync(
        RileyLinkCommand command,
        CancellationToken cancellationToken,
        params byte[] data)
    {
        var commandData = new byte[data.Length + 2];
        commandData[0] = (byte)(data.Length + 1);
        commandData[1] = (byte)command;
        if (data.Length > 0)
            data.CopyTo(commandData, 2);
        _responseCountUpdatedEvent.Reset();

        var result = new BleExchangeResult { CommunicationResult = BleCommunicationResult.WriteFailed };
        try
        {
            await TryWriteToCharacteristic(_dataCharacteristic, commandData, cancellationToken);
            result.BleWriteCompleted = DateTimeOffset.UtcNow;
            result.CommunicationResult = BleCommunicationResult.IndicateTimedOut;

            await Policy.TimeoutAsync(TimeSpan.FromSeconds(15), TimeoutStrategy.Optimistic)
                .ExecuteAsync(token => _responseCountUpdatedEvent.WaitAsync(token), cancellationToken);
            result.BleReadIndicated = DateTimeOffset.UtcNow;
            result.CommunicationResult = BleCommunicationResult.ReadFailed;
            var response = new Bytes(await TryReadFromCharacteristic(_dataCharacteristic, cancellationToken));
            result.CommunicationResult = BleCommunicationResult.OK;

            if (response.Length > 0)
                result.ResponseCode = (RileyLinkResponse)response[0];
            result.ResponseData = response.Sub(1);
            return result;
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            result.Exception = e;
            return result;
        }
    }


    private async Task<byte[]> TryReadFromCharacteristic(ICharacteristic characteristic,
        CancellationToken cancellationToken = default)
    {
        var retryPolicy = Policy.Handle<Exception>(e => !(e is TaskCanceledException))
            .WaitAndRetryAsync(4, attempt => TimeSpan.FromMilliseconds(50*attempt));
        var timeoutPolicy = Policy.TimeoutAsync(10, TimeoutStrategy.Optimistic);
        var policy = timeoutPolicy.WrapAsync(retryPolicy);

        return await policy.ExecuteAsync(token => characteristic.ReadAsync(token), cancellationToken);
    }

    private async Task TryWriteToCharacteristic(ICharacteristic characteristic, byte[] data,
        CancellationToken cancellationToken = default)
    {
        var retryPolicy = Policy.Handle<Exception>(e => !(e is TaskCanceledException))
            .WaitAndRetryAsync(4, attempt => TimeSpan.FromMilliseconds(50*attempt));
        var resultPolicy = Policy.HandleResult<bool>(r => !r)
            .WaitAndRetryAsync(4, attempt => TimeSpan.FromMilliseconds(50*attempt));;
        var timeoutPolicy = Policy.TimeoutAsync(10, TimeoutStrategy.Optimistic);
        var policy = timeoutPolicy.WrapAsync(resultPolicy).WrapAsync(retryPolicy);
        var writeResult = await policy.ExecuteAsync(token => characteristic.WriteAsync(data, cancellationToken), cancellationToken);
        if (!writeResult)
            throw new ApplicationException("BLE write returned false");
    }
}