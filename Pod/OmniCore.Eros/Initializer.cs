using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Eros
{
    public static class Initializer
    {
        public static IContainer WithOmnipodEros
            (this IContainer container)
        {
            return container
                .One<IErosPodProvider, ErosPodProvider>()
                .Many<IErosPodRequestMessage, ErosPodRequestMessage>()
                .Many<IErosPodResponseMessage, ErosPodResponseMessage>()
                .Many<IErosPod, ErosPod>()
                .Many<IPodRequest, ErosPodRequest>();
            //.Many<ErosRequestQueue>();
        }
    }
}