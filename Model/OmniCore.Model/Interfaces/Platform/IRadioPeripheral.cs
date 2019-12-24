using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheral : IDisposable, INotifyPropertyChanged
    {
        Guid Uuid { get; }
        string Name { get; }
        Task<IRadioPeripheralLease> Lease(CancellationToken cancellationToken);
        TimeSpan? RssiUpdateTimeSpan { get; set; }
        int? Rssi { get; set; }
        DateTimeOffset? RssiDate { get; }
        PeripheralConnectionState ConnectionState { get; }
        DateTimeOffset? ConnectionStateDate { get; }
        DateTimeOffset? DisconnectDate { get; }
    }
}
