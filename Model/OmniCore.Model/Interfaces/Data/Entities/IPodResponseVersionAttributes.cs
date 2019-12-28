using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Data.Entities
{
    public interface IPodResponseVersionAttributes
    {
        string VersionPm { get; set; }
        string VersionPi { get; set; }
        byte[] VersionUnk2b { get; set; }
        uint Lot { get; set; }
        uint Serial { get; set; }
        uint RadioAddress { get; set; }

    }
}
