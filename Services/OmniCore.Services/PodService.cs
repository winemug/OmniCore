using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Services;

public class PodService
{
    private List<Pod> _pods = new();

    private RadioService _radioService;
    public PodService(RadioService radioService)
    {
        _radioService = radioService;
        _pods.Add(new Pod()
        {
            RadioAddress = 0x34c867a2,
            Lot = 72402,
            Serial = 3210594,
            NextMessageSequence = 2,
            NextPacketSequence = 18
        });
    }
    
    public async Task<PodConnection> GetConnectionAsync(
        uint radioAddress,
        CancellationToken cancellationToken = default)
    {
        var pod = _pods.Where(r => r.RadioAddress == radioAddress).FirstOrDefault();
        if (pod == null)
            return null;

        var radioConnection = await _radioService.GetConnectionAsync("ema");
        var podAllocationLockDisposable = await pod.LockAsync(cancellationToken);
        return new PodConnection(pod, radioConnection, podAllocationLockDisposable);
    }
}