using OmniCore.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioProvider
    {
        IObservable<Radio> ListRadios(CancellationToken cancellationToken);
        Task<IRadioConnection> GetConnection(Radio radioEntity, PodRequest request, CancellationToken cancellationToken);
    }
}
