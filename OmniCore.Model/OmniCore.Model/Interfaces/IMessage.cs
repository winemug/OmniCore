using System;
using System.Collections.Generic;
using OmniCore.Model.Enums;
using OmniCore.Model.Utilities;

namespace OmniCore.Model.Interfaces
{
    public interface IMessage
    {
        //uint? AckAddressOverride { get; set; }
        //uint? address { get; set; }
        //Bytes body { get; set; }
        //int body_length { get; set; }
        //Bytes body_prefix { get; set; }
        //bool DoubleTake { get; set; }
        //bool expect_critical_followup { get; set; }
        //string message_str_prefix { get; set; }
        //List<Tuple<byte, Bytes, uint?>> parts { get; set; }
        //int? sequence { get; set; }
        //TxPower? TxLevel { get; set; }

        //void add_part(PdmRequest cmd_type, Bytes cmd_body);

        // void AddRequest(IRequest request);
        // IList<IResponse> Responses { get; }

        IList<IMessagePart> GetParts();

        //bool add_radio_packet(Packet radio_packet);
        //List<Packet> get_radio_packets(int first_packet_sequence);
    }
}