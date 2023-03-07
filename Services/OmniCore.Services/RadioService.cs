using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RadioService : IRadioService
{
    private List<Radio> _radios;

    public void Start()
    {
        Debug.WriteLine("starting radios");
        _radios = new List<Radio>
        {
            new(Guid.Parse("00000000-0000-0000-0000-bc33acb95371"), "ema")
            //new Radio(Guid.Parse("00000000-0000-0000-0000-886b0ff897cf"), "mod"),
            //new Radio(Guid.Parse("00000000-0000-0000-0000-c2c42b149fe4"), "ora"),
        };
    }

    public void Stop()
    {
        Debug.WriteLine("stopping radios");
        foreach (var radio in _radios) radio.Dispose();
        _radios = null;
    }

    public async Task<IRadioConnection> GetIdealConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_radios.Count == 0)
            return null;
        if (_radios.Count == 1)
            return await GetConnectionAsync(_radios[0], cancellationToken);

        await Task.WhenAll(_radios.Select(r => r.UpdateRssiAsync(cancellationToken)));

        var radio = _radios.Where(r => r.Rssi.HasValue)
            .OrderByDescending(r => r.Rssi).FirstOrDefault();

        if (radio == null)
            return null;

        return await GetConnectionAsync(radio, cancellationToken);
    }

    public async Task<IRadioConnection> GetConnectionByNameAsync(string name,
        CancellationToken cancellationToken = default)
    {
        var radio = _radios.Where(r => r.Name == name).FirstOrDefault();
        if (radio == null)
            return null;
        return await GetConnectionAsync(radio, cancellationToken);
    }

    public async Task<IRadioConnection> GetConnectionByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var radio = _radios.Where(r => r.Id == id).FirstOrDefault();
        if (radio == null)
            return null;
        return await GetConnectionAsync(radio, cancellationToken);
    }

    private async Task<IRadioConnection> GetConnectionAsync(Radio radio, CancellationToken cancellationToken)
    {
        var allocationLockDisposable = await radio.LockAsync(cancellationToken);
        return new RadioConnection(radio, allocationLockDisposable);
    }
}