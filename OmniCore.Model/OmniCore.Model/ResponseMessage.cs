using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System.Text;

namespace OmniCore.Model
{
    public class ResponseMessage : Message, IResponse
    {
        public ResponseMessage():base()
        {
            base.type = PacketType.POD;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{this.address:%08X} {this.sequence:%02X} {this.expect_critical_followup} ");
            foreach(var p in this.parts)
            {
                sb.Append($"{p.Item1:%02X} {p.Item2.ToHex()} ");
            }
            return sb.ToString();
        }
    }
}

