using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheral : IDisposable, INotifyPropertyChanged
    {
        Guid PeripheralUuid { get; }
        string PeripheralName { get; }
        Task<IRadioPeripheralLease> Lease(CancellationToken cancellationToken);
        TimeSpan? RssiUpdateTimeSpan { get; set; }
        int? Rssi { get; set; }
        DateTimeOffset? RssiDate { get; }
        DateTimeOffset? LastSeen { get; }
    }
}
