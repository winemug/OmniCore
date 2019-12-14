using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IRadio
    {
        IRadioPeripheral Peripheral { get; set; }
        IRadioEntity Entity { get; set; }
        Task<IRadioConfiguration> GetDefaultConfiguration();
        Task<IRadioLease> Lease(CancellationToken cancellationToken);
    }
}