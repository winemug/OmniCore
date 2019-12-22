using System;

namespace OmniCore.Model.Interfaces.Data.Entities
{
    public interface IRadioEntity : IRadioAttributes, IEntity
    {
        bool KeepConnected { get; set; }
        TimeSpan ResponseTimeout { get; set; }
        TimeSpan ConnectTimeout { get; set; }
        string ConfigurationJson { get; set; }
    }
}
