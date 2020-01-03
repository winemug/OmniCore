using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioLease : IDisposable, IServerResolvable
    {
        IRadioPeripheralLease PeripheralLease { get; set; }
        IRadio Radio { get; set; }
        Task Initialize(CancellationToken cancellationToken);
        Task Identify(CancellationToken cancellationToken);
        Task ExecuteRequest(IPodRequest request, CancellationToken cancellationToken);
        Task<(byte Rssi, byte[] Data)> DebugGetPacket(uint timeoutMilliseconds, CancellationToken cancellationToken);
    }
}