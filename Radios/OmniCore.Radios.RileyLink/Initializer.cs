using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Radios.RileyLink;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Constants;

namespace OmniCore.Radios.RileyLink
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithRileyLinkRadio
            (this ICoreContainer<IServerResolvable> container)
        {
            return container
                .One<IRadioService, RileyLinkRadioService>()
                .Many<IRadio, RileyLinkRadio>();
        }
    }
}
