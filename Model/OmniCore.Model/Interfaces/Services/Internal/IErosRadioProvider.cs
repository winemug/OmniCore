using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IErosRadioProvider: IDisposable
    {
        Guid ServiceUuid { get; }

        Task<IErosRadio> GetRadio(Guid uuid,
            CancellationToken cancellationToken);
    }
}