using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IRadioEntity : IRadioAttributes, IEntity
    {
        bool KeepConnected { get; set; }
        TimeSpan ResponseTimeout { get; set; }
        TimeSpan ConnectTimeout { get; set; }
        string ConfigurationJson { get; set; }
    }
}
