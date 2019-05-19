using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    public class MessageExchange
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

        public PdmMessage PdmMessage;
        public PodMessage PodMessage;
        public Exception Error;

        private IPacketRadio PacketRadio;
        private Pod Pod;

        public Packet last_received_packet;
        public int last_packet_timestamp = 0;

        public MessageExchange(PdmMessage pdmMessage, IPacketRadio packetRadio, Pod pod)
        {
            this.PdmMessage = pdmMessage;
            this.PacketRadio = packetRadio;
            this.Pod = pod;
        }

        private void reset_sequences()
        {
            this.Pod.radio_packet_sequence = 0;
        }

        public async Task<PodMessage> GetResponse(IMessageExchangeProgress messageExchangeProgress)
        {
            this.Started = DateTime.UtcNow;
            if (this.PdmMessage.TxLevel.HasValue)
            {
                this.PacketRadio.SetTxLevel(this.PdmMessage.TxLevel.Value);
            }

            if (!this.PdmMessage.address.HasValue)
                this.PdmMessage.address = this.Pod.radio_address;

            if (!this.PdmMessage.AckAddressOverride.HasValue)
                this.PdmMessage.AckAddressOverride = this.Pod.radio_address;

            if (!this.PdmMessage.sequence.HasValue)
                this.PdmMessage.sequence = this.Pod.radio_message_sequence;


            var packets = this.PdmMessage.get_radio_packets(this.Pod.radio_packet_sequence);

            Packet received = null;
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
                        received = await this.ExchangePackets(packet.with_sequence(this.Pod.radio_packet_sequence), expected_type, timeout);
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
                                await this.PacketRadio.Reset();
                                timeout = 15000;
                                continue;
                            }
                            else
                            {
                                Debug.WriteLine("Failed recovery");
                                this.reset_sequences();
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
                                await this.PacketRadio.Reset();
                                continue;
                            }
                            else if (repeat_count < 4)
                            {
                                await this.PacketRadio.Reset();
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
                                await this.PacketRadio.Reset();
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
                                await this.PacketRadio.Reset();
                                timeout = 10000;
                                Thread.Sleep(2000);
                                continue;
                            }
                            else
                            {
                                Debug.WriteLine("Failed recovery");
                                this.reset_sequences();
                                throw;
                            }
                        }
                    }
                    catch (ProtocolException pe)
                    {
                        if (pe.ReceivedPacket != null && expected_type == PacketType.POD && pe.ReceivedPacket.type == PacketType.ACK)
                        {
                            Debug.WriteLine("Trying to recover from protocol error");
                            //this.Pod.radio_packet_sequence++;
                            this.Pod.radio_message_sequence++;
                            this.PdmMessage.sequence = this.Pod.radio_message_sequence;

                            return await GetPodResponse();
                        }
                        else
                            throw pe;
                    }
                    //catch (ProtocolException pe)
                    //{
                    //    if (pe.ReceivedPacket != null && expected_type == RadioPacketType.POD && pe.ReceivedPacket.type == RadioPacketType.ACK)
                    //    {
                    //        Debug.WriteLine("Trying to recover from protocol error");
                    //        received = pe.ReceivedPacket;
                    //        while(true)
                    //        {
                    //            this.Pod.radio_packet_sequence = (received.sequence + 1) % 32;
                    //            var interimAck = this.interim_ack(this.PdmMessage.AckAddressOverride.Value, this.Pod.radio_packet_sequence);
                    //            try
                    //            {
                    //                received = await this.ExchangePackets(interimAck, RadioPacketType.POD, timeout);
                    //                break;
                    //            }
                    //            catch (ProtocolException)
                    //            {
                    //                this.Pod.radio_packet_sequence = (this.Pod.radio_packet_sequence + 1) % 32;
                    //                continue;
                    //            }
                    //            catch (OmnipyTimeoutException)
                    //            {
                    //                this.Pod.radio_packet_sequence = (this.Pod.radio_packet_sequence + 1) % 32;
                    //                throw new StatusUpdateRequiredException(pe);
                    //            }
                    //            catch(Exception)
                    //            {
                    //                throw;
                    //            }
                    //        }
                    //        continue;
                    //    }
                    //    if (pe.ReceivedPacket != null)
                    //    {
                    //        Debug.WriteLine("Trying to recover from protocol error");
                    //        this.Pod.radio_packet_sequence = (pe.ReceivedPacket.sequence + 1) % 32;
                    //        this.Pod.radio_message_sequence = (this.Pod.radio_message_sequence + 1) % 16;
                    //        if (pe.ReceivedPacket != null && expected_type == RadioPacketType.POD && pe.ReceivedPacket.type == RadioPacketType.ACK)
                    //        {
                    //            throw new StatusUpdateRequiredException(pe);
                    //        }
                    //    }
                    //    else
                    //        throw;
                    //}
                    catch (Exception) { throw; }
                }
                part++;
                this.Pod.radio_packet_sequence = (received.sequence + 1) % 32;
            }

            Debug.WriteLine($"SENT MSG {this.PdmMessage}");

            var part_count = 0;
            if (received.type == PacketType.POD)
            {
                part_count = 1;
                Debug.WriteLine($"Received POD message part {part_count}");
            }
            var pod_response = new PodMessage();
            while (!pod_response.add_radio_packet(received))
            {
                var ack_packet = this.interim_ack(this.PdmMessage.AckAddressOverride.Value, (received.sequence + 1) % 32);
                received = await this.ExchangePackets(ack_packet, PacketType.CON);
                part_count++;
                Debug.WriteLine($"Received POD message part {part_count}");
            }

            Debug.WriteLine($"RCVD MSG {pod_response}");
            Debug.WriteLine("Send and receive completed.");
            this.Pod.radio_message_sequence = (pod_response.sequence.Value + 1) % 16;
            this.Pod.radio_packet_sequence = (received.sequence + 1) % 32;

            return pod_response;

        }


        private async Task<Packet> ExchangePackets(Packet packet_to_send, PacketType expected_type, int timeout = 10000)
        {
            int start_time = 0;
            bool first = true;
            Bytes received = null;
            Debug.WriteLine($"SEND PKT {packet_to_send}");
            while (start_time == 0 || Environment.TickCount - start_time < timeout)
            {
                if (first)
                    first = false;
                else
                    this.repeated_sends += 1;

                if (this.last_packet_timestamp == 0 || (Environment.TickCount - this.last_packet_timestamp) > 2000)
                    received = await this.PacketRadio.SendAndGetPacket(packet_to_send.get_data(), 0, 0, 300, 1, 300);
                else
                    received = await this.PacketRadio.SendAndGetPacket(packet_to_send.get_data(), 0, 0, 120, 0, 40);
                if (start_time == 0)
                    start_time = Environment.TickCount;

                Debug.WriteLine($"SEND PKT {packet_to_send}");

                if (received == null)
                {
                    this.receive_timeouts++;
                    Debug.WriteLine("RECV PKT None");
                    this.PacketRadio.TxLevelUp();
                    continue;
                }

                var p = this.GetPacket(received);
                if (p == null)
                {
                    this.bad_packets++;
                    this.PacketRadio.TxLevelDown();
                    continue;
                }

                Debug.WriteLine($"RECV PKT {p}");
                if (p.address != this.Pod.radio_address)
                {
                    this.bad_packets++;
                    Debug.WriteLine("RECV PKT ADDR MISMATCH");
                    this.PacketRadio.TxLevelDown();
                    continue;
                }

                this.last_packet_timestamp = Environment.TickCount;

                if (this.last_received_packet != null && p.sequence == this.last_received_packet.sequence
                    && p.type == this.last_received_packet.type)
                {
                    this.repeated_receives++;
                    Debug.WriteLine("RECV PKT previous");
                    this.PacketRadio.TxLevelUp();
                    continue;
                }

                this.last_received_packet = p;
                this.Pod.radio_packet_sequence = (p.sequence + 1) % 32;

                if (p.type != expected_type)
                {
                    Debug.WriteLine("RECV PKT unexpected type");
                    this.protocol_errors++;
                    throw new ProtocolException("Unexpected packet type received", p);
                }

                if (p.sequence != (packet_to_send.sequence + 1) % 32)
                {
                    this.Pod.radio_packet_sequence = (p.sequence + 1) % 32;
                    Debug.WriteLine("RECV PKT unexpected sequence");
                    this.last_received_packet = p;
                    this.protocol_errors++;
                    throw new ProtocolException("Incorrect packet sequence received", p);
                }

                return p;

            }
            throw new OmniCoreTimeoutException("Exceeded timeout while send and receive");
        }

        private async Task SendPacket(Packet packet_to_send, int allow_premature_exit_after = -1, int timeout = 25000)
        {
            int start_time = 0;
            this.unique_packets++;
            Bytes received = null;
            while (start_time == 0 || Environment.TickCount - start_time < timeout)
            {
                try
                {
                    Debug.WriteLine($"SEND PKT {packet_to_send}");

                    received = await this.PacketRadio.SendAndGetPacket(packet_to_send.get_data(), 0, 0, 300, 0, 40);

                    if (start_time == 0)
                        start_time = Environment.TickCount;

                    //if (allow_premature_exit_after >= 0 && Environment.TickCount - start_time >= allow_premature_exit_after)
                    //{
                    //    if (this.request_arrived.WaitOne(0))
                    //    {
                    //        Debug.WriteLine("Prematurely exiting final phase to process next request");
                    //        this.packet_sequence = (this.packet_sequence + 1) % 32;
                    //        break;
                    //    }
                    //}

                    if (received == null)
                    {
                        received = await this.PacketRadio.GetPacket(600);
                        if (received == null)
                        {
                            Debug.WriteLine("Silence fell.");
                            this.Pod.radio_packet_sequence = (this.Pod.radio_packet_sequence + 1) % 32;
                            break;
                        }
                    }

                    var p = this.GetPacket(received);
                    if (p == null)
                    {
                        this.bad_packets++;
                        this.PacketRadio.TxLevelDown();
                        continue;
                    }

                    if (p.address != this.Pod.radio_address)
                    {
                        this.bad_packets++;
                        Debug.WriteLine("RECV PKT ADDR MISMATCH");
                        this.PacketRadio.TxLevelDown();
                        continue;
                    }

                    this.last_packet_timestamp = Environment.TickCount;
                    if (this.last_received_packet != null && p.type == this.last_received_packet.type
                        && p.sequence == this.last_received_packet.sequence)
                    {
                        this.repeated_receives++;
                        Debug.WriteLine("RECV PKT previous");
                        this.PacketRadio.TxLevelUp();
                        continue;
                    }

                    Debug.WriteLine($"RECV PKT {p}");
                    Debug.WriteLine($"RECEIVED unexpected packet");
                    this.protocol_errors++;
                    this.last_received_packet = p;
                    this.Pod.radio_packet_sequence = (p.sequence + 1) % 32;
                    packet_to_send.with_sequence(this.Pod.radio_packet_sequence);
                    start_time = Environment.TickCount;
                    continue;
                }
                catch (PacketRadioException pre)
                {
                    this.radio_errors++;
                    Debug.WriteLine($"Radio error during send, retrying {pre}");
                    await this.PacketRadio.Reset();
                    start_time = Environment.TickCount;
                }
                catch (OmniCoreTimeoutException)
                {
                }
                catch (Exception) { throw; }
            }
            Debug.WriteLine("Exceeded timeout while waiting for silence to fall");
        }

        private Packet GetPacket(Bytes data)
        {
            if (data != null && data.Length > 1)
            {
                byte rssi = data[0];
                try
                {
                    var rp = Packet.parse(data.Sub(2));
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

        private Packet _ack_data(uint address1, uint address2, int sequence)
        {
            return new Packet(address1, PacketType.ACK, sequence, new Bytes(address2));
        }

        private Packet interim_ack(uint ack_address_override, int sequence)
        {
            if (ack_address_override == this.Pod.radio_address)
                return _ack_data(this.Pod.radio_address, this.Pod.radio_address, sequence);
            else
                return _ack_data(this.Pod.radio_address, ack_address_override, sequence);
        }

        private Packet final_ack(uint ack_address_override, int sequence)
        {
            if (ack_address_override == this.Pod.radio_address)
                return _ack_data(this.Pod.radio_address, 0, sequence);
            else
                return _ack_data(this.Pod.radio_address, ack_address_override, sequence);
        }

    }
}