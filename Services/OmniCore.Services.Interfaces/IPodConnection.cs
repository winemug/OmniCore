using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces;

public interface IPodConnection : IDisposable
{
    Task<PodResponse> Pair(CancellationToken cancellationToken = default);
    Task<PodResponse> Activate(CancellationToken cancellationToken = default);

    Task<PodResponse> Start(
        BasalRateEntry[] basalRateEntries,
        CancellationToken cancellationToken = default);

    Task<PodResponse> UpdateStatus(CancellationToken cancellationToken = default);
    Task<PodResponse> Beep(BeepType type, CancellationToken cancellationToken = default);
    Task<PodResponse> CancelTempBasal(CancellationToken cancellationToken = default);

    Task<PodResponse> SetTempBasal(
        int hourlyRateMilliunits,
        int halfHourCount,
        CancellationToken cancellationToken = default);

    Task<PodResponse> Bolus(
        int bolusMilliunits,
        int pulseIntervalSeconds,
        CancellationToken cancellationToken = default);

    Task<PodResponse> CancelBasal(CancellationToken cancellationToken = default);
    Task<PodResponse> CancelBolus(CancellationToken cancellationToken = default);
    Task<PodResponse> Suspend(CancellationToken cancellationToken = default);
    Task<PodResponse> Deactivate(CancellationToken cancellationToken = default);
}