using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosRequest : IMessagePart
    {
        public PartType PartType { get; }

        public Bytes PartData { get; }

        public bool RequiresNonce { get; }

        // public uint Nonce { get; set; }

        public ErosRequest(PartType requestType, Bytes requestData, bool requiresNonce = false)
        {
            this.PartData = requestData;
            this.PartType = requestType;
            this.RequiresNonce = requiresNonce;
        }
    }
}

