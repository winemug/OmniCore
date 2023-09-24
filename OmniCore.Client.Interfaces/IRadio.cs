using OmniCore.Common.Pod;

namespace OmniCore.Common.Radio;

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