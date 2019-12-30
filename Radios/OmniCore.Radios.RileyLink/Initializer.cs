using OmniCore.Model.Interfaces;
using OmniCore.Radios.RileyLink;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Radios.RileyLink
{
    public static class Initializer
    {
        public static ICoreContainer WithRileyLinkRadio(this ICoreContainer container)
        {
            return container
                .One<IRadioService, RileyLinkRadioService>()
                .Many<IRadio, RileyLinkRadio>()
                .Many<IRadioLease, RileyLinkRadioLease>();
        }
    }
}
