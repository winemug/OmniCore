using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IRadio : IServerResolvable, ILeaseable<IRadio>, IDisposable
    {
        IRadioPeripheral Peripheral { get; set; }
        IRadioEntity Entity { get; set; }
        IRadioConfiguration DefaultConfiguration { get; }
        Task SetConfiguration(IRadioConfiguration configuration, CancellationToken cancellationToken);
        IRadioConfiguration GetConfiguration();
        bool InUse { get; set; }
        RadioActivity Activity { get; set; }
        DateTimeOffset? ActivityStartDate { get; }

        Task Initialize(CancellationToken cancellationToken);
        Task Identify(CancellationToken cancellationToken);
        Task ExecuteRequest(IPodRequest request, CancellationToken cancellationToken);
        Task<(byte Rssi, byte[] Data)> DebugGetPacket(uint timeoutMilliseconds, CancellationToken cancellationToken);
        void Start(IRadioEntity radioEntity, IRadioPeripheral getPeripheral);
    }
}