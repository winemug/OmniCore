using System;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IRadioEntity : IRadioAttributes, IEntity
    {
        string ConfigurationJson { get; set; }
    }
}
