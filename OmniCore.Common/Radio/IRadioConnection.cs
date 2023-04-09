using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services.Interfaces.Radio;

public interface IRadioConnection : IDisposable
{
    Task<BleExchangeResult> TryGetPacket(
        byte channel,
        uint timeoutMs,
        CancellationToken cancellationToken = default);

    Task<BleExchangeResult> SendPacket(
        byte channel,
        byte repeatCount,
        ushort delayMilliseconds,
        ushort preambleExtensionMs,
        IPodPacket packet,
        CancellationToken cancellationToken);

    Task<BleExchangeResult> SendAndTryGetPacket(
        byte sendChannel,
        byte sendRepeatCount,
        ushort sendRepeatDelayMs,
        ushort sendPreambleExtensionMs,
        byte listenChannel,
        uint listenTimeoutMs,
        byte listenRetryCount,
        IPodPacket packet,
        CancellationToken cancellationToken);
}