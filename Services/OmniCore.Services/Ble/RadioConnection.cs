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

    public async Task<IPodPacket> TryGetPacket(
        byte channel,
        uint timeoutMs,
        CancellationToken cancellationToken = default)
    {
        var cmdParams = new Bytes()
            .Append(channel)
            .Append(timeoutMs)
            .ToArray();
        var (code, result) = await _radio.ExecuteCommandAsync(
            RileyLinkCommand.GetPacket, cancellationToken,
            cmdParams);
        if (code != RileyLinkResponse.CommandSuccess)
            return null;

        if (result.Length < 3)
            return null;

        var rssi = ((sbyte)result[0] - 127) / 2; // -128 to 127
        var sequence = result[1];
        // var seq = result[1];
        var data = new Bytes(result.Skip(2).ToArray());
        return PodPacket.FromRadioData(data, rssi);
    }

    public async Task<bool> SendPacket(
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
        var (code, result) = await _radio.ExecuteCommandAsync(
            RileyLinkCommand.SendPacket, cancellationToken,
            cmdParamsWithData);
        return code == RileyLinkResponse.CommandSuccess;
    }

    public async Task<IPodPacket> SendAndTryGetPacket(
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
        var (code, result) = await _radio.ExecuteCommandAsync(
            RileyLinkCommand.SendAndListen, cancellationToken,
            cmdParamsWithData);
        if (code != RileyLinkResponse.CommandSuccess)
            return null;
        if (result.Length < 3)
            return null;

        var rssi = ((sbyte)result[0] - 127) / 2; // -128 to 127
        var sequence = result[1];

        var data = new Bytes(result.Skip(2).ToArray());
        return PodPacket.FromRadioData(data, rssi);
    }
}