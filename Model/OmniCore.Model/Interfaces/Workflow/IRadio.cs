using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IRadio
    {
        IRadioEntity Entity { get; }
        Task<string> GetDefaultConfiguration();
        Task<IRadioConnection> GetConnection();
    }
}