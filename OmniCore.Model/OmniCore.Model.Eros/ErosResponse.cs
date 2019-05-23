using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosResponse : IMessagePart
    {
        public PartType PartType { get; set;  }

        public Bytes PartData { get; set; }

    }
}
