using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IPodProvider
    {
        IRadioProvider[] RadioProviders { get; }
        string Description { get; }
        string Code { get; }
        Task<IList<IPod>> ActivePods();
        Task<IList<IPodEntity>> ArchivedPods();
        Task<IPod> New(IUserEntity user, IMedicationEntity medication, IList<IRadioEntity> radios);
        Task<IPod> Register(IPodEntity pod, IUserEntity user, IList<IRadioEntity> radios);
    }
}