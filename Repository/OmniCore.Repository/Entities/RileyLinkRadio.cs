using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class RileyLinkRadio : UpdateableEntity
    {
        public uint CenterFrequency { get; set; } = (uint)(433923000m / 366.2109375m);

    }
}
