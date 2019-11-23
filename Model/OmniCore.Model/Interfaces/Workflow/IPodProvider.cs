using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IPodProvider
    {
        Task<IList<IPod>> ListActive();
        Task<IList<IPod>> ListArchived();
        Task<IPod> New(IUserEntity user, IMedicationEntity medication, IList<IRadioEntity> radios);
        Task<IPod> Register(IPod pod, IUserEntity user, IList<IRadioEntity> radios);
    }
}