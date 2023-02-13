using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Services;

public class PodService
{
    private List<Pod> _pods = new();

    public PodService()
    {
        _pods.Add(new Pod()
        {
            RadioAddress = 0x34c867a2,
            Lot = 72402,
            Serial = 3210594,
            NextMessageSequence = 4,
            NextPacketSequence = 29
        });
    }
    
    public async Task<PodConnection> GetConnectionAsync(
        RadioConnection radioConnection,
        uint radioAddress,
        CancellationToken cancellationToken = default)
    {
        var pod = _pods.Where(r => r.RadioAddress == radioAddress).FirstOrDefault();
        if (pod == null)
            return null;
        var allocationLockDisposable = await pod.LockAsync(cancellationToken);
        return new PodConnection(pod, radioConnection, allocationLockDisposable);
    }
}