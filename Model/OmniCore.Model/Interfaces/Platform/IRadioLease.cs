using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRadioLease : IDisposable
    {
        IRadioPeripheralLease PeripheralLease { get; set; }
        IRadio Radio { get; set; }
        Task Initialize(CancellationToken cancellationToken);
        Task Identify(CancellationToken cancellationToken);
        Task ExecuteRequest(IPodRequest request, CancellationToken cancellationToken);
        Task<(byte Rssi, byte[] Data)> DebugGetPacket(uint timeoutMilliseconds, CancellationToken cancellationToken);
    }
}