using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRadioService
    {
        IRadioProvider[] Providers { get; }
    }
}
