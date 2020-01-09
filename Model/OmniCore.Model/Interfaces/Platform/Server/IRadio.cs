using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Common.Data.Entities;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IRadio : INotifyPropertyChanged, IServerResolvable, ILeaseable<IRadio>
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
    }
}