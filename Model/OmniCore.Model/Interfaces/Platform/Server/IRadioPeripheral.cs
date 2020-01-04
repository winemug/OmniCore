using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioPeripheral : IDisposable, INotifyPropertyChanged, IServerResolvable
    {
        Guid Uuid { get; }

        Guid ServiceUuid { get; }
        string Name { get; set; }
        Task<IRadioPeripheralLease> Lease(CancellationToken cancellationToken);
        TimeSpan? RssiUpdateTimeSpan { get; set; }
        int? Rssi { get; set; }
        DateTimeOffset? RssiDate { get; }
        PeripheralState State { get; }
        DateTimeOffset? ConnectionStateDate { get; }
        DateTimeOffset? DisconnectDate { get; }
    }
}
