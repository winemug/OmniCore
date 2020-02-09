using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Server;
using OmniCore.Model.Interfaces.Services.ServiceEntities;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IPodService : ICoreService
    {
        Task<IList<IPod>> ActivePods(CancellationToken cancellationToken);
        Task<IList<IPod>> ArchivedPods(CancellationToken cancellationToken);
        Task<IPodEros> NewErosPod(IUser user, IMedication medication, CancellationToken cancellationToken);
        IObservable<IErosRadio> ListErosRadios();
        Task<IPodDash> NewDashPod(IUser user, IMedication medication, CancellationToken cancellationToken);
    }
}