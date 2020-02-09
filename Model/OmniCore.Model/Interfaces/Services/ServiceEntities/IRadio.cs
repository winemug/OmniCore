using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IRadio
    {
        RadioType Type { get; }
        string Address { get; }
        string Description { get; }
        IObservable<string> Name { get; }
        IObservable<PeripheralState> State { get; }
        IObservable<PeripheralConnectionState> ConnectionState { get; }
        IObservable<int> Rssi{ get; }

        Task SetDescription(string description, CancellationToken cancellationToken);
        
        Task Identify(CancellationToken cancellationToken);

        bool InUse { get; }
        RadioActivity Activity { get;  }
        DateTimeOffset? ActivityStartDate { get; }
    }
}