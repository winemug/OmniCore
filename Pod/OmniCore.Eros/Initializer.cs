using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Facade;
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
                .Many<ErosPod>()
                .Many<IErosPodRequest, ErosPodRequest>()
                .Many<ErosRequestQueue>();
        }
    }
}