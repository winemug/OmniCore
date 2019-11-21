using OmniCore.Model.Interfaces.Operational;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRadioService
    {
        IRadioProvider RileyLinkProvider { get; }
        IRadioProvider RftpProvider { get; }
        IRadioProvider DashRadioProvider { get; }
    }
}
