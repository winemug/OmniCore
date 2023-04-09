using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Services.Interfaces.Radio;

namespace OmniCore.Services;

public class RadioConnection : IRadioConnection
{
    private readonly Radio _radio;
    private readonly IDisposable _radioLockDisposable;

    public RadioConnection(Radio radio, IDisposable radioLockDisposable)
    {
        _radio = radio;
        _radioLockDisposable = radioLockDisposable;
    }

    public void Dispose()
    {
        _radioLockDisposable?.Dispose();
    }

    public async Task<BleExchangeResult> TryGetPacket(
        byte channel,
        uint timeoutMs,
        CancellationToken cancellationToken = default)
    {
        var cmdParams = new Bytes()
            .Append(channel)
            .Append(timeoutMs)
            .ToArray();
        return await _radio.ExecuteCommandAsync(
            RileyLinkCommand.GetPacket, cancellationToken,
            cmdParams);
    }

    public async Task<BleExchangeResult> SendPacket(
        byte channel,
        byte repeatCount,
        ushort delayMilliseconds,
        ushort preambleExtensionMs,
        IPodPacket packet,
        CancellationToken cancellationToken)
    {
        var cmdParamsWithData = new Bytes()
            .Append(channel)
            .Append(repeatCount)
            .Append(delayMilliseconds)
            .Append(preambleExtensionMs)
            .Append(packet.ToRadioData())
            .ToArray();
        return await _radio.ExecuteCommandAsync(
            RileyLinkCommand.SendPacket, cancellationToken,
            cmdParamsWithData);
    }

    public async Task<BleExchangeResult> SendAndTryGetPacket(
        byte sendChannel,
        byte sendRepeatCount,
        ushort sendRepeatDelayMs,
        ushort sendPreambleExtensionMs,
        byte listenChannel,
        uint listenTimeoutMs,
        byte listenRetryCount,
        IPodPacket packet,
        CancellationToken cancellationToken)
    {
        var cmdParamsWithData = new Bytes()
            .Append(sendChannel)
            .Append(sendRepeatCount)
            .Append(sendRepeatDelayMs)
            .Append(listenChannel)
            .Append(listenTimeoutMs)
            .Append(listenRetryCount)
            .Append(sendPreambleExtensionMs)
            .Append(packet.ToRadioData())
            .ToArray();
        return await _radio.ExecuteCommandAsync(
            RileyLinkCommand.SendAndListen, cancellationToken,
            cmdParamsWithData);
    }
}