using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IRadio
    {
        IRadioEntity Entity { get; set; }
        Task<IRadioConfiguration> GetDefaultConfiguration();
        Task<IRadioConnection> GetConnection(IRadioConfiguration radioConfiguration, CancellationToken cancellationToken);
    }
}