using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IRadio : INotifyPropertyChanged
    {
        IRadioPeripheral Peripheral { get; set; }
        IRadioEntity Entity { get; set; }
        IRadioConfiguration DefaultConfiguration { get; }
        Task SetConfiguration(IRadioConfiguration configuration);
        IRadioConfiguration GetConfiguration();
        Task<IRadioLease> Lease(CancellationToken cancellationToken);
        bool IsBusy { get; set; }
    }
}