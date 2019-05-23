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

        public uint Nonce { get; set; }

        public ErosRequest(PartType requestType, Bytes requestData, bool requiresNonce = false)
        {
            this.PartData = requestData;
            this.PartType = requestType;
            this.RequiresNonce = requiresNonce;
        }

        //public Request(RequestType cmd_type, Bytes cmd_body):base()
        //{
        //    this.add_part(cmd_type, cmd_body);
        //    this.message_str_prefix = "\n";
        //    this.type = PacketType.PDM;
        //}


        //public void set_nonce(uint nonce)
        //{
        //    var part = this.parts[0];
        //    this.parts[0] = new Tuple<byte, Bytes, uint?>(part.Item1, part.Item2, nonce);
        //}

        //public override string ToString()
        //{
        //    var sb = new StringBuilder();
        //    sb.Append(this.message_str_prefix);
        //    foreach (var p in this.parts)
        //    {
        //        if (p.Item3 == null)
        //            sb.Append($"{p.Item1:%02X} {p.Item2.ToHex()} ");
        //        else
        //            sb.Append($"{p.Item1:%02X} {p.Item3.Value:%08X} {p.Item2.ToHex()} ");
        //    }
        //    return sb.ToString();
        //}
    }
}

