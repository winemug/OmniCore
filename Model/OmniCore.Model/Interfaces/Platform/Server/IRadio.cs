using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces;

namespace OmniCore.Model.Interfaces
{
    public interface IRadio : INotifyPropertyChanged, IServerResolvable
    {
        IRadioPeripheral Peripheral { get; set; }
        IRadioEntity Entity { get; set; }
        IRadioConfiguration DefaultConfiguration { get; }
        Task SetConfiguration(IRadioConfiguration configuration);
        IRadioConfiguration GetConfiguration();
        Task<IRadioLease> Lease(CancellationToken cancellationToken);
        bool InUse { get; set; }
        RadioActivity Activity { get; set; }
        DateTimeOffset? ActivityStartDate { get; }
    }
}