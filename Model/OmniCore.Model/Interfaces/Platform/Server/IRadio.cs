using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IRadio : IServerResolvable, ILeaseable<IRadio>, IDisposable
    {
        IRadioPeripheral Peripheral { get; set; }
        RadioEntity Entity { get; set; }
        bool InUse { get; set; }
        RadioActivity Activity { get; set; }
        DateTimeOffset? ActivityStartDate { get; }

        void StartMonitoring();
        Task Initialize(CancellationToken cancellationToken);
        Task Identify(CancellationToken cancellationToken);
        Task ExecuteRequest(IPodRequest request, CancellationToken cancellationToken);
        Task<(byte Rssi, byte[] Data)> DebugGetPacket(uint timeoutMilliseconds, CancellationToken cancellationToken);
    }
}