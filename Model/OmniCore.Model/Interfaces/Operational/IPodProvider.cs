using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Operational
{
    public interface IPodProvider
    {
        Task<IList<IPod>> ListActive();
        Task<IList<IPod>> ListArchived();
        Task<IPod> New(IUserEntity user, IMedicationEntity medication, IList<IRadioEntity> radios);
        Task<IPod> Register(IPod pod, IUserEntity user, IList<IRadioEntity> radios);
    }
}
