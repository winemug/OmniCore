using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;

namespace OmniCore.Model.Eros
{
    public class ErosMessage : IMessage
    {
        public ErosMessage()
        {
            parts = new List<IMessagePart>();
        }

        public IList<IMessagePart> GetParts()
        {
            return parts;
        }

        public uint? address { get; set; }
        public int? sequence { get; set; }
        public bool expect_critical_followup { get; set; }
        public int body_length { get; set; }
        public Bytes body { get; set; }
        public Bytes body_prefix { get; set; }
        public List<IMessagePart> parts { get; set; }
        public string message_str_prefix { get; set; }

        public PacketType? type { get; set; }
        public IList<IMessagePart> Responses { get; private set; }


    }
}

