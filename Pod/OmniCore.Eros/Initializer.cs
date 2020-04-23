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
                .Many<IErosPodRequest, ErosPodRequest>()
                .Many<IErosPodResponse, ErosPodResponse>()
                .Many<IErosPod, ErosPod>()
                .Many<IPodTask, ErosPodTask>();
            //.Many<ErosRequestQueue>();
        }
    }
}