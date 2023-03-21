using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Tests;

public class PodConnectionProxy : IPodConnection
{
    private IPodConnection _podConnection;

    public PodConnectionProxy(IPodConnection podConnection)
    {
        _podConnection = podConnection;
    }

    public void Dispose()
    {
        _podConnection.Dispose();
    }

    public async Task<PodResponse> Pair(CancellationToken cancellationToken = default)
    {
        return await _podConnection.Pair(cancellationToken);
    }

    public async Task<PodResponse> Activate(CancellationToken cancellationToken = default)
    {
        return await _podConnection.Activate(cancellationToken);
    }

    public async Task<PodResponse> Start(BasalRateEntry[] basalRateEntries, CancellationToken cancellationToken = default)
    {
        return await _podConnection.Start(basalRateEntries, cancellationToken);
    }

    public async Task<PodResponse> UpdateStatus(CancellationToken cancellationToken = default)
    {
        return await _podConnection.UpdateStatus(cancellationToken);
    }

    public async Task<PodResponse> Beep(BeepType type, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PodResponse> CancelTempBasal(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PodResponse> SetTempBasal(int hourlyRateMilliunits, int halfHourCount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PodResponse> Bolus(int bolusPulses, int pulseIntervalSeconds, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PodResponse> CancelBasal(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PodResponse> CancelBolus(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PodResponse> Suspend(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PodResponse> Deactivate(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}