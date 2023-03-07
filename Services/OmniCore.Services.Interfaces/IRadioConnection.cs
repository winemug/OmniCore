using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces;

public interface IRadioConnection : IDisposable
{
    Task<IPodPacket> TryGetPacket(
        byte channel,
        uint timeoutMs,
        CancellationToken cancellationToken = default);

    Task<bool> SendPacket(
        byte channel,
        byte repeatCount,
        ushort delayMilliseconds,
        ushort preambleExtensionMs,
        IPodPacket packet,
        CancellationToken cancellationToken);

    Task<IPodPacket> SendAndTryGetPacket(
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