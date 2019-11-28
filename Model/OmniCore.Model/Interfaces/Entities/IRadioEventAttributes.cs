using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IRadioEventAttributes
    {
        RadioEvent EventType { get; set; }
        bool Success { get; set; }
        byte[] Data { get; set; }
    }
}
