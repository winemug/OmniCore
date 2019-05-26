using OmniCore.Model;
using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Radio.RileyLink
{
    public class RileyLinkMessageExchange : IMessageExchange
    {
        public int unique_packets = 0;
        public int repeated_sends = 0;
        public int receive_timeouts = 0;
        public int repeated_receives = 0;
        public int protocol_errors = 0;
        public int bad_packets = 0;
        public int radio_errors = 0;
        public bool successful = false;
        public DateTime Started;
        public DateTime Ended;

        private RileyLink RileyLink;

        private IPod Pod;
        public RadioPacket last_received_packet;
        public int last_packet_timestamp = 0;

        private ErosMessageExchangeParameters MessageExchangeParameters;
        private Task FinalAckTask = null;

        internal RileyLinkMessageExchange(IMessageExchangeParameters messageExchangeParameters, IPod pod, RileyLink rileyLinkInstance)
        {
            RileyLink = rileyLinkInstance;
            Pod = pod;
            MessageExchangeParameters = messageExchangeParameters as ErosMessageExchangeParameters;
        }

        public void UpdateParameters(IMessageExchangeParameters messageExchangeParameters, IPod pod, RileyLink rileyLinkInstance)
        {
            RileyLink = rileyLinkInstance;
            Pod = pod;
            MessageExchangeParameters = messageExchangeParameters as ErosMessageExchangeParameters;
        }

        public async Task InitializeExchange(IMessageProgress messageProgress, CancellationToken ct)
        {
            if (FinalAckTask != null)
            {
                await FinalAckTask;
            }

            await RileyLink.Connect();
        }

        public async Task<IMessage> GetResponse(IMessage requestMessage, IMessageProgress messageExchangeProgress, CancellationToken ct)
        {
            this.Started = DateTime.UtcNow;
            if (MessageExchangeParameters.TransmissionLevelOverride.HasValue)
            {
                RileyLink.SetTxLevel(MessageExchangeParameters.TransmissionLevelOverride.Value);
            }

            var erosRequestMessage = requestMessage as ErosMessage;
            var packets = GetRadioPackets(erosRequestMessage);

            RadioPacket received = null;
            var packet_count = packets.Count;

            this.unique_packets = packet_count * 2;

            for (int part = 0; part < packet_count; part++)
            {
                var packet = packets[part];
                int repeat_count = -1;
                int timeout = 10000;
                while (true)
                {
                    repeat_count++;
                    if (repeat_count == 0)
                        Debug.WriteLine($"Sending PDM message part {part + 1}/{packet_count}");
                    else
                        Debug.WriteLine($"Sending PDM message part {part + 1}/{packet_count} (Repeat: {repeat_count})");

                    PacketType expected_type;
                    if (part == packet_count - 1)
                        expected_type = PacketType.POD;
                    else
                        expected_type = PacketType.ACK;

                    try
                    {
                        received = await this.ExchangePackets(packet.with_sequence(this.Pod.PacketSequence), expected_type, timeout);
                        break;
                    }
                    catch (OmniCoreTimeoutException)
                    {
                        Debug.WriteLine("Trying to recover from timeout error");
                        if (part == 0)
                        {
                            if (repeat_count == 0)
                            {
                                timeout = 15000;
                                continue;
                            }
                            else if (repeat_count == 1)
                            {
                                timeout = 10000;
                                Thread.Sleep(2000);
                                continue;
                            }
                            else if (repeat_count == 2)
                            {
                                await RileyLink.Reset();
                                timeout = 15000;
                                continue;
                            }
                            else
                            {
                                Debug.WriteLine("Failed recovery");
                                Pod.PacketSequence = 0;
                                throw;
                            }
                        }
                        else if (part < packet_count - 1)
                        {
                            if (repeat_count < 2)
                            {
                                timeout = 20000;
                                continue;
                            }
                            else
                                throw;
                        }
                        else
                        {
                            if (repeat_count < 10)
                            {
                                timeout = 20000;
                                continue;
                            }
                            else
                                throw;
                        }
                    }
                    catch (PacketRadioException)
                    {
                        Debug.WriteLine("Trying to recover from radio error");
                        this.radio_errors++;
                        if (part == 0)
                        {
                            if (repeat_count < 2)
                            {
                                await RileyLink.Reset();
                                continue;
                            }
                            else if (repeat_count < 4)
                            {
                                await RileyLink.Reset();
                                timeout = 10000;
                                Thread.Sleep(2000);
                                continue;
                            }
                            else
                            {
                                Debug.WriteLine("Failed recovery");
                                throw;
                            }
                        }
                        else if (part < packet_count - 1)
                        {
                            if (repeat_count < 6)
                            {
                                await RileyLink.Reset();
                                timeout = 10000;
                                Thread.Sleep(2000);
                                continue;
                            }
                            else
                            {
                                Debug.WriteLine("Failed recovery");
                                throw;
                            }
                        }
                        else
                        {
                            if (repeat_count < 10)
                            {
                                await RileyLink.Reset();
                                timeout = 10000;
                                Thread.Sleep(2000);
                                continue;
                            }
                            else
                            {
                                Debug.WriteLine("Failed recovery");
                                Pod.PacketSequence = 0;
                                throw;
                            }
                        }
                    }
                    catch (ErosProtocolException pe)
                    {
                        if (pe.ReceivedPacket != null && expected_type == PacketType.POD && pe.ReceivedPacket.type == PacketType.ACK)
                        {
                            Debug.WriteLine("Trying to recover from protocol error");
                            this.Pod.PacketSequence++;

                            return await GetResponse(requestMessage, messageExchangeProgress, ct);
                        }
                        else
                            throw pe;
                    }
                    catch (Exception) { throw; }
                }
                part++;
                this.Pod.PacketSequence = (received.sequence + 1) % 32;
            }

            Debug.WriteLine($"SENT MSG {requestMessage}");

            var part_count = 0;
            if (received.type == PacketType.POD)
            {
                part_count = 1;
                Debug.WriteLine($"Received POD message part {part_count}");
            }
            var responseBuilder = new ErosResponseBuilder();

            var radioAddress = Pod.RadioAddress;
            if (MessageExchangeParameters.AddressOverride.HasValue)
                radioAddress = MessageExchangeParameters.AddressOverride.Value;

            var ackAddress = radioAddress;
            if (MessageExchangeParameters.AckAddressOverride.HasValue)
                ackAddress = MessageExchangeParameters.AckAddressOverride.Value;

            while (!responseBuilder.WithRadioPacket(received))
            {
                var ackPacket = this.InterimAckPacket(ackAddress, (received.sequence + 1) % 32);
                received = await this.ExchangePackets(ackPacket, PacketType.CON);
                part_count++;
                Debug.WriteLine($"Received POD message part {part_count}");
            }

            var podResponse = responseBuilder.Build();

            Debug.WriteLine($"RCVD MSG {podResponse}");
            Debug.WriteLine("Send and receive completed.");
            this.Pod.MessageSequence = (podResponse.sequence.Value + 1) % 16;
            this.Pod.PacketSequence = (received.sequence + 1) % 32;

            var finalAckPacket = this.FinalAckPacket(ackAddress, (received.sequence + 1) % 32);

            FinalAckTask = Task.Run(() => AcknowledgeEndOfMessage(finalAckPacket));
            return podResponse;
        }

        private async Task AcknowledgeEndOfMessage(RadioPacket ackPacket)
        {
            try
            {
                Debug.WriteLine("Sending final ack");
                await SendPacket(ackPacket);
                this.Pod.PacketSequence++;
                Debug.WriteLine("Message exchange finalized");
            }
            catch(Exception)
            {
                Debug.WriteLine("Final ack failed, ignoring.");
            }
        }


        private async Task<RadioPacket> ExchangePackets(RadioPacket packet_to_send, PacketType expected_type, int timeout = 10000)
        {
            int start_time = 0;
            Bytes received = null;
            Debug.WriteLine($"SEND PKT {packet_to_send}");
            while (start_time == 0 || Environment.TickCount - start_time < timeout)
            {
                try
                {
                    if (this.last_packet_timestamp == 0 || (Environment.TickCount - this.last_packet_timestamp) > 2000)
                        received = await RileyLink.SendAndGetPacket(packet_to_send.get_data(), 0, 0, 300, 1, 300);
                    else
                        received = await RileyLink.SendAndGetPacket(packet_to_send.get_data(), 0, 0, 120, 0, 40);
                }
                catch (OmniCoreTimeoutException)
                {
                    received = null;
                }
                finally
                {
                    if (start_time == 0)
                        start_time = Environment.TickCount;
                    else
                        this.repeated_sends += 1;
                }

                if (received == null)
                {
                    this.receive_timeouts++;
                    Debug.WriteLine("RECV PKT None");
                    RileyLink.TxLevelUp();
                    continue;
                }

                var p = this.GetPacket(received);
                if (p == null)
                {
                    this.bad_packets++;
                    RileyLink.TxLevelDown();
                    continue;
                }

                Debug.WriteLine($"RECV PKT {p}");
                if (p.address != this.Pod.RadioAddress)
                {
                    this.bad_packets++;
                    Debug.WriteLine("RECV PKT ADDR MISMATCH");
                    RileyLink.TxLevelDown();
                    continue;
                }

                this.last_packet_timestamp = Environment.TickCount;

                if (this.last_received_packet != null && p.sequence == this.last_received_packet.sequence
                    && p.type == this.last_received_packet.type)
                {
                    this.repeated_receives++;
                    Debug.WriteLine("RECV PKT previous");
                    RileyLink.TxLevelUp();
                    continue;
                }

                this.last_received_packet = p;
                this.Pod.PacketSequence = (p.sequence + 1) % 32;

                if (p.type != expected_type)
                {
                    Debug.WriteLine("RECV PKT unexpected type");
                    this.protocol_errors++;
                    throw new ErosProtocolException("Unexpected packet type received", p);
                }

                if (p.sequence != (packet_to_send.sequence + 1) % 32)
                {
                    this.Pod.PacketSequence = (p.sequence + 1) % 32;
                    Debug.WriteLine("RECV PKT unexpected sequence");
                    this.last_received_packet = p;
                    this.protocol_errors++;
                    throw new ErosProtocolException("Incorrect packet sequence received", p);
                }

                return p;

            }
            throw new OmniCoreTimeoutException("Exceeded timeout while send and receive");
        }

        private async Task SendPacket(RadioPacket packet_to_send, int timeout = 25000)
        {
            int start_time = 0;
            this.unique_packets++;
            Bytes received = null;
            while (start_time == 0 || Environment.TickCount - start_time < timeout)
            {
                try
                {
                    Debug.WriteLine($"SEND PKT {packet_to_send}");

                    try
                    {
                        received = await RileyLink.SendAndGetPacket(packet_to_send.get_data(), 0, 0, 300, 3, 300);
                    }
                    catch(OmniCoreTimeoutException)
                    {
                        Debug.WriteLine("Silence fell.");
                        this.Pod.PacketSequence = (this.Pod.PacketSequence + 1) % 32;
                        return;
                    }

                    if (start_time == 0)
                        start_time = Environment.TickCount;

                    var p = this.GetPacket(received);
                    if (p == null)
                    {
                        this.bad_packets++;
                        RileyLink.TxLevelDown();
                        continue;
                    }

                    if (p.address != this.Pod.RadioAddress)
                    {
                        this.bad_packets++;
                        Debug.WriteLine("RECV PKT ADDR MISMATCH");
                        RileyLink.TxLevelDown();
                        continue;
                    }

                    this.last_packet_timestamp = Environment.TickCount;
                    if (this.last_received_packet != null && p.type == this.last_received_packet.type
                        && p.sequence == this.last_received_packet.sequence)
                    {
                        this.repeated_receives++;
                        Debug.WriteLine("RECV PKT previous");
                        RileyLink.TxLevelUp();
                        continue;
                    }

                    Debug.WriteLine($"RECV PKT {p}");
                    Debug.WriteLine($"RECEIVED unexpected packet");
                    this.protocol_errors++;
                    this.last_received_packet = p;
                    this.Pod.PacketSequence = (p.sequence + 1) % 32;
                    packet_to_send.with_sequence(this.Pod.PacketSequence);
                    start_time = Environment.TickCount;
                    continue;
                }
                catch (PacketRadioException pre)
                {
                    this.radio_errors++;
                    Debug.WriteLine($"Radio error during send, retrying {pre}");
                    await RileyLink.Reset();
                    start_time = Environment.TickCount;
                }
                catch (Exception) { throw; }
            }
            Debug.WriteLine("Exceeded timeout while waiting for silence to fall");
        }

        private RadioPacket GetPacket(Bytes data)
        {
            if (data != null && data.Length > 1)
            {
                byte rssi = data[0];
                try
                {
                    var rp = RadioPacket.parse(data.Sub(2));
                    if (rp != null)
                        rp.rssi = rssi;
                    return rp;
                }
                catch
                {
                    Debug.WriteLine($"RECV INVALID DATA {data}");
                }
            }
            return null;
        }

        private RadioPacket CreateAckPacket(uint address1, uint address2, int sequence)
        {
            return new RadioPacket(address1, PacketType.ACK, sequence, new Bytes(address2));
        }

        private RadioPacket InterimAckPacket(uint ack_address_override, int sequence)
        {
            if (ack_address_override == this.Pod.RadioAddress)
                return CreateAckPacket(this.Pod.RadioAddress, this.Pod.RadioAddress, sequence);
            else
                return CreateAckPacket(this.Pod.RadioAddress, ack_address_override, sequence);
        }

        private RadioPacket FinalAckPacket(uint ack_address_override, int sequence)
        {
            if (ack_address_override == this.Pod.RadioAddress)
                return CreateAckPacket(this.Pod.RadioAddress, 0, sequence);
            else
                return CreateAckPacket(this.Pod.RadioAddress, ack_address_override, sequence);
        }

        public List<RadioPacket> GetRadioPackets(ErosMessage message)
        {
            // this.message_str_prefix = $"{this.address:%08X} {this.sequence:%02X} {this.expect_critical_followup} ";

            var message_body_len = 0;
            foreach (var p in message.GetParts())
            {
                var ep = p as ErosRequest;
                var cmd_body = ep.PartData;
                var nonce = ep.Nonce;
                message_body_len += (int)cmd_body.Length + 2;
                if (ep.RequiresNonce)
                    message_body_len += 4;
            }

            byte b0 = 0;
            if (MessageExchangeParameters.CriticalWithFollowupRequired)
                b0 = 0x80;

            var msgSequence = Pod.MessageSequence;
            if (MessageExchangeParameters.MessageSequenceOverride.HasValue)
                msgSequence = MessageExchangeParameters.MessageSequenceOverride.Value;

            b0 |= (byte)(msgSequence << 2);
            b0 |= (byte)((message_body_len >> 8) & 0x03);
            byte b1 = (byte)(message_body_len & 0xff);

            var msgAddress = Pod.RadioAddress;
            if (MessageExchangeParameters.AddressOverride.HasValue)
                msgAddress = MessageExchangeParameters.AddressOverride.Value;

            var message_body = new Bytes(msgAddress);
            message_body.Append(b0);
            message_body.Append(b1);

            foreach (var p in message.GetParts())
            {
                var ep = p as ErosRequest;
                var cmd_type = (byte) ep.PartType;
                var cmd_body = ep.PartData;
                var nonce = ep.Nonce;

                if (ep.RequiresNonce)
                {
                    message_body.Append(cmd_type);
                    message_body.Append((byte)(cmd_body.Length + 4));
                    message_body.Append(nonce);
                }
                else
                {
                    if (cmd_type == (byte)PartType.ResponseStatus)
                        message_body.Append(cmd_type);
                    else
                    {
                        message_body.Append(cmd_type);
                        message_body.Append((byte)cmd_body.Length);
                    }
                }
                message_body.Append(cmd_body[0]);
            }
            var crc_calculated = CrcUtil.Crc16(message_body.ToArray());
            message_body.Append(crc_calculated);

            int index = 0;
            bool first_packet = true;
            int sequence = Pod.PacketSequence;
            int total_body_len = (int)message_body.Length;
            var radio_packets = new List<RadioPacket>();
            var ackAddress = msgAddress;
            if (MessageExchangeParameters.AckAddressOverride.HasValue)
                ackAddress = MessageExchangeParameters.AckAddressOverride.Value;

            while (index < message_body.Length)
            {
                var to_write = Math.Min(31, message_body.Length - index);
                var packet_body = message_body.Sub(index, index + to_write);
                radio_packets.Add(new RadioPacket(ackAddress,
                                                first_packet ? PacketType.PDM : PacketType.CON,
                                                sequence,
                                                packet_body));
                first_packet = false;
                sequence = (sequence + 2) % 32;
                index += to_write;
            }

            if (MessageExchangeParameters.RepeatFirstPacket)
            {
                var fp = radio_packets[0];
                radio_packets.Insert(0, fp);
            }
            return radio_packets;
        }

        public PodCommandResult ParseResponse(IMessage response, IPod pod)
        {
            foreach(var mp in response.GetParts())
            {
                var er = mp as ErosResponse;
                er.Parse(pod);
            }

            return new PodCommandResult()
            {
            };
        }
    }
}