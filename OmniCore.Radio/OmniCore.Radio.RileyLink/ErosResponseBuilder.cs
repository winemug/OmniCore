using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Radio.RileyLink
{
    public class ErosResponseBuilder
    {
        private ErosMessage ResponseMessage;
        public ErosResponseBuilder()
        {
            ResponseMessage = new ErosMessage();
        }

        public ErosMessage Build()
        {
            return ResponseMessage;
        }

        public bool WithRadioPacket(RadioPacket radio_packet)
        {
            if (radio_packet.type == PacketType.POD || radio_packet.type == PacketType.PDM)
            {
                ResponseMessage.type = radio_packet.type;
                ResponseMessage.address = radio_packet.body.DWord(0);
                var r4 = radio_packet.body.Byte(4);
                ResponseMessage.sequence = (r4 >> 2) & 0x0f;
                ResponseMessage.expect_critical_followup = (r4 & 0x80) > 0;
                ResponseMessage.body_length = ((r4 & 0x03) << 8) | radio_packet.body.Byte(5);
                ResponseMessage.body_prefix = radio_packet.body.Sub(0, 6);
                ResponseMessage.body = radio_packet.body.Sub(6);
            }
            else
            {
                if (radio_packet.type == PacketType.CON)
                    ResponseMessage.body.Append(radio_packet.body);
                else
                    throw new ErosProtocolException("Packet type invalid");
            }

            if (ResponseMessage.body_length == ResponseMessage.body.Length - 2)
            {
                var bodyWithoutCrc = ResponseMessage.body.Sub(0, ResponseMessage.body.Length - 2);
                var crc = ResponseMessage.body.Word(ResponseMessage.body.Length - 2);
                var crc_calculated = CrcUtil.Crc16(new Bytes(ResponseMessage.body_prefix, bodyWithoutCrc).ToArray());

                if (crc == crc_calculated)
                {
                    ResponseMessage.body = bodyWithoutCrc;
                    var bi = 0;
                    while (bi < ResponseMessage.body.Length)
                    {
                        var response_type = (PartType) ResponseMessage.body[bi];
                        Bytes response_body;
                        if (response_type == PartType.ResponseStatus)
                        {
                            response_body = ResponseMessage.body.Sub(bi + 1);
                            bi = ResponseMessage.body.Length;
                        }
                        else
                        {
                            var response_len = ResponseMessage.body[bi + 1];
                            response_body = ResponseMessage.body.Sub(bi + 2, bi + 2 + response_len);
                            bi += response_len + 2;
                        }
                        ResponseMessage.parts.Add(new ErosResponse() { PartType = response_type, PartData = response_body });
                    }
                    return true;
                }
                else
                {
                    throw new ErosProtocolException("Message crc error");
                }
            }
            else
            {
                return false;
            }
        }
    }
}
