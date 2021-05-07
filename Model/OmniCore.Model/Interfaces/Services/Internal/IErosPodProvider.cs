using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IErosPodProvider: IDisposable
    {
        Task<IList<IErosPod>> ActivePods(CancellationToken cancellationToken);
        Task<IErosPod> NewPod(IUser user, IMedication medication, CancellationToken cancellationToken);
    }
}