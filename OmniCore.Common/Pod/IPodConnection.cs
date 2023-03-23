using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces.Pod;

public interface IPodConnection : IDisposable
{
    Task<PodResponse> PrimePodAsync(DateOnly podDate, TimeOnly podTime, bool relaxDeliveryCrosschecks, CancellationToken cancellationToken);
    Task<PodResponse> StartPodAsync(TimeOnly podTime, BasalRateEntry[] basalRateEntries, CancellationToken cancellationToken = default);
    Task<PodResponse> ConfigureAlerts(
        AlertConfiguration[] alertConfigurations,
        CancellationToken cancellationToken);
    Task<PodResponse> UpdateStatus(CancellationToken cancellationToken = default);
    Task<PodResponse> Beep(BeepType type, CancellationToken cancellationToken = default);
    Task<PodResponse> SetBasalSchedule(
        TimeOnly podTime,
        BasalRateEntry[] basalRateEntries,
        CancellationToken cancellationToken = default);
    Task<PodResponse> SetTempBasal(
        int hourlyRateMilliunits,
        int halfHourCount,
        CancellationToken cancellationToken = default);
    Task<PodResponse> CancelTempBasal(CancellationToken cancellationToken = default);
    Task<PodResponse> Bolus(
        int bolusPulses,
        int pulseIntervalSeconds,
        CancellationToken cancellationToken = default);
    Task<PodResponse> CancelBolus(CancellationToken cancellationToken = default);
    Task<PodResponse> Deactivate(CancellationToken cancellationToken = default);
}
