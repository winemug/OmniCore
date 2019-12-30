using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Data.Entities;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRadio : INotifyPropertyChanged
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