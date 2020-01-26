using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IPodService : ICoreService
    {
        IRadioService[] RadioProviders { get; }
        string Description { get; }
        IList<IPod> ActivePods(CancellationToken cancellationToken);
        IList<IPod> ArchivedPods(CancellationToken cancellationToken);
        Task<IPod> New(UserEntity user, MedicationEntity medication, RadioEntity radio);
        Task<IPod> Register(PodEntity pod, UserEntity user, RadioEntity radio);
    }
}