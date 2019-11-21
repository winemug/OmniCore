using OmniCore.Model.Interfaces.Attributes;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Operational
{
    public interface IRadio
    {
        IRadioEntity Entity { get; }
        Task<string> GetDefaultConfiguration();
        Task<IRadioConnection> GetConnection();
    }
}
