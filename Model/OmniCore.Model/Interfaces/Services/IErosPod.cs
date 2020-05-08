using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services.Requests;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IErosPod : IPod
    {
        Task UpdateRadioList(IEnumerable<IErosRadio> radios, CancellationToken cancellationToken);
    }
}