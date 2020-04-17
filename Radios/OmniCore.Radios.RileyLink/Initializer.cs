using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Radios.RileyLink
{
    public static class Initializer
    {
        public static IContainer<IServiceInstance> WithRileyLinkRadio
            (this IContainer<IServiceInstance> container)
        {
            return container
                .One<IErosRadioProvider, RileyLinkRadioProvider>(nameof(RileyLinkRadioProvider))
                .Many<RileyLinkRadio>();
        }
    }
}