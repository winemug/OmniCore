using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services.Interfaces.Radio;

public interface IRadio : IDisposable
{
    Guid Id { get; }
    string Name { get; }
    int? Rssi { get; }
    Task<IDisposable> LockAsync(CancellationToken cancellationToken);
    void Dispose();
    Task UpdateRssiAsync(CancellationToken cancellationToken = default);

    Task<BleExchangeResult> ExecuteCommandAsync(
        RileyLinkCommand command,
        CancellationToken cancellationToken,
        params byte[] data);
}