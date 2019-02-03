using OmniCore.Model.Protocol.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Protocol
{
    public class MessagePart : IMessagePart
    {
        public MessagePart(bool hasLengthField, bool nonceRequired, byte partType, byte[] content)
        {
            this.NonceRequired = nonceRequired;
            this.HasLengthField = hasLengthField;
            this.PartType = partType;
            this.Content = content;
            this.Data = BuildPart();
        }

        private byte[] BuildPart()
        {
            byte[] cmdBytes;
            if (this.HasLengthField)
            {
                var cmdLength = this.Content.Length + 2;
                cmdBytes = new byte[cmdLength];
                cmdBytes[0] = this.PartType;
                cmdBytes[1] = (byte)cmdLength;
                this.Content.CopyTo(cmdBytes, 2);
            }
            else
            {
                var cmdLength = this.Content.Length + 1;
                cmdBytes = new byte[cmdLength];
                cmdBytes[0] = this.PartType;
                this.Content.CopyTo(cmdBytes, 1);
            }
            return cmdBytes;
        }

        public void SetNonce(uint nonce)
        {
            this.Data = BuildPart();
            if (!NonceRequired)
                throw new Exception("Nonce not required for this request");
            int i = 1;
            if (HasLengthField)
                i = 2;
            Data[i]   = (byte)((nonce & 0xFF000000) >> 24);
            Data[i+1] = (byte)((nonce & 0x00FF0000) >> 16);
            Data[i+2] = (byte)((nonce & 0x0000FF00) >> 8);
            Data[i+3] = (byte)((nonce & 0x000000FF));
        }

        public bool HasLengthField { get; private set; }
        public byte PartType { get; private set; }
        public byte[] Content { get; private set; }
        public byte[] Data { get; private set; }
        public bool NonceRequired { get; private set; }
    }
}
