using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Eros
{
    public class ErosPodRadioSelector
    {
        private readonly ILogger Logger;

        private List<IErosRadio> Radios;
        public ErosPodRadioSelector(ILogger logger)
        {
            Logger = logger;
        }

        public async Task Initialize(List<IErosRadio> radios)
        {
            Radios = radios;
        }

        public async Task<IErosRadio> Select()
        {
            if (Radios == null || Radios.Count == 0)
                throw new OmniCoreWorkflowException(FailureType.Internal, "No radios available");

            //TODO: algo
            return Radios[0];
        }
    }
}