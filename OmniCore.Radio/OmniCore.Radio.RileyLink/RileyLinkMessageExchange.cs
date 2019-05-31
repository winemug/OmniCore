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
        private ErosPod Pod;
        private RileyLink RileyLink;
        public RadioPacket last_received_packet;
        public int last_packet_timestamp = 0;

        private ErosMessageExchangeParameters MessageExchangeParameters;
        private Task FinalAckTask = null;
        private SynchronizationContext UiContext;
        public IMessageExchangeStatistics Statistics { get => RlStatistics as IMessageExchangeStatistics; }

        private RileyLinkStatistics RlStatistics;
        internal RileyLinkMessageExchange(IMessageExchangeParameters messageExchangeParameters, IPod pod, RileyLink rileyLinkInstance, SynchronizationContext uiContext)
        {
            UiContext = uiContext;
            SetParameters(messageExchangeParameters, pod, rileyLinkInstance);
        }

        public void SetParameters(IMessageExchangeParameters messageExchangeParameters, IPod pod, RileyLink rileyLinkInstance)
        {
            RileyLink = rileyLinkInstance;
            Pod = pod as ErosPod;
            MessageExchangeParameters = messageExchangeParameters as ErosMessageExchangeParameters;
            RlStatistics = new RileyLinkStatistics();
            this.RileyLink.Statistics = RlStatistics;
        }

        private async Task RunInUiContext(Func<Task> a)
        {
            var current = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(UiContext);
            await a.Invoke();
            SynchronizationContext.SetSynchronizationContext(current);
        }

        private async Task<T> RunInUiContext<T>(Func<Task<T>> a)
        {
            var current = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(UiContext);
            var ret = await a.Invoke();
            SynchronizationContext.SetSynchronizationContext(current);
            return ret;
        }


        public async Task InitializeExchange(IMessageExchangeProgress messageProgress, CancellationToken ct)
        {
            if (FinalAckTask != null)
            {
                await FinalAckTask;
            }

            await RunInUiContext(async () => await RileyLink.EnsureDevice());
        }

        public async Task<IMessage> GetResponse(IMessage requestMessage, IMessageExchangeProgress messageExchangeProgress, CancellationToken ct)
        {
            try
            {
                return await GetResponseInternal(requestMessage, messageExchangeProgress, ct);
            }
            finally
            {
                RileyLink.Statistics = null;
            }
        }

        private async Task<IMessage> GetResponseInternal(IMessage requestMessage, IMessageExchangeProgress messageExchangeProgress, CancellationToken ct)
        {
            this.RlStatistics.StartMessageExchange();
            if (MessageExchangeParameters.TransmissionLevelOverride.HasValue)
            {
                await RunInUiContext(async () => await RileyLink.SetTxLevel(MessageExchangeParameters.TransmissionLevelOverride.Value));
            }

            var erosRequestMessage = requestMessage as ErosMessage;
            var packets = GetRadioPackets(erosRequestMessage);

            RadioPacket received = null;
            var packet_count = packets.Count;

            for (int part = 0; part < packet_count; part++)
            {
                var packet = packets[part];
                int repeat_count = -1;
                int timeout = 10000;
                this.RlStatistics.StartPacketExchange();
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
                        received = await this.ExchangePackets(packet.with_sequence(this.Pod.RuntimeVariables.PacketSequence), expected_type, timeout);
                        break;
                    }
                    catch (OmniCoreTimeoutException ote)
                    {
                        Debug.WriteLine("Trying to recover from timeout error");
                        this.RlStatistics.TimeoutOccured(ote);
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
                                await RunInUiContext(async () => await RileyLink.Reset());
                                timeout = 15000;
                                continue;
                            }
                            else
                            {
                                Debug.WriteLine("Failed recovery");
                                Pod.RuntimeVariables.PacketSequence = 0;
                                this.RlStatistics.ExitPrematurely();
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
                            {
                                this.RlStatistics.ExitPrematurely();
                                throw;
                            }
                        }
                        else
                        {
                            if (repeat_count < 10)
                            {
                                timeout = 20000;
                                continue;
                            }
                            else
                            {
                                this.RlStatistics.ExitPrematurely();
                                throw;
                            }
                        }
                    }
                    catch (OmniCoreRadioException pre)
                    {
                        this.RlStatistics.RadioErrorOccured(pre);
                        Debug.WriteLine("Trying to recover from radio error");
                        if (part == 0)
                        {
                            if (repeat_count < 2)
                            {
                                await RunInUiContext(async () => await RileyLink.Reset());
                                continue;
                            }
                            else if (repeat_count < 4)
                            {
                                await RunInUiContext(async () => await RileyLink.Reset());
                                timeout = 10000;
                                Thread.Sleep(2000);
                                continue;
                            }
                            else
                            {
                                Debug.WriteLine("Failed recovery");
                                this.RlStatistics.ExitPrematurely();
                                throw;
                            }
                        }
                        else if (part < packet_count - 1)
                        {
                            if (repeat_count < 6)
                            {
                                await RunInUiContext(async () => await RileyLink.Reset());
                                timeout = 10000;
                                Thread.Sleep(2000);
                                continue;
                            }
                            else
                            {
                                Debug.WriteLine("Failed recovery");
                                this.RlStatistics.ExitPrematurely();
                                throw;
                            }
                        }
                        else
                        {
                            if (repeat_count < 10)
                            {
                                await RunInUiContext(async () => await RileyLink.Reset());
                                timeout = 10000;
                                Thread.Sleep(2000);
                                continue;
                            }
                            else
                            {
                                Debug.WriteLine("Failed recovery");
                                Pod.RuntimeVariables.PacketSequence = 0;
                                this.RlStatistics.ExitPrematurely();
                                throw;
                            }
                        }
                    }
                    catch (OmniCoreErosException pe)
                    {
                        this.RlStatistics.ProtocolErrorOccured(pe);
                        if (pe.ReceivedPacket != null && expected_type == PacketType.POD && pe.ReceivedPacket.type == PacketType.ACK)
                        {
                            this.Pod.RuntimeVariables.PacketSequence++;
                        }
                        this.RlStatistics.ExitPrematurely();
                        throw pe;
                    }
                    catch (Exception e)
                    {
                        this.RlStatistics.UnknownErrorOccured(e);
                        this.RlStatistics.ExitPrematurely();
                        throw;
                    }
                }
                this.RlStatistics.EndPacketExchange();
                part++;
                this.Pod.RuntimeVariables.PacketSequence = (received.sequence + 1) % 32;
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
                this.RlStatistics.StartPacketExchange();
                var ackPacket = this.InterimAckPacket(ackAddress, (received.sequence + 1) % 32);
                received = await this.ExchangePackets(ackPacket, PacketType.CON);
                part_count++;
                Debug.WriteLine($"Received POD message part {part_count}");
                this.RlStatistics.EndPacketExchange();
            }

            this.RlStatistics.EndMessageExchange();
            var podResponse = responseBuilder.Build();

            Debug.WriteLine($"RCVD MSG {podResponse}");
            Debug.WriteLine("Send and receive completed.");
            this.Pod.RuntimeVariables.PacketSequence = (received.sequence + 1) % 32;

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
                this.Pod.RuntimeVariables.PacketSequence++;
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
                        received = await RunInUiContext(async () => await RileyLink.SendAndGetPacket(packet_to_send.get_data(), 0, 0, 300, 1, 300));
                    else
                        received = await RunInUiContext(async () => await RileyLink.SendAndGetPacket(packet_to_send.get_data(), 0, 0, 120, 0, 40));
                }
                catch (OmniCoreTimeoutException)
                {
                    received = null;
                }
                finally
                {
                    if (start_time == 0)
                        start_time = Environment.TickCount;
                }

                if (received == null)
                {
                    this.RlStatistics.NoPacketReceived();
                    Debug.WriteLine("RECV PKT None");
                    await RunInUiContext(async () => await RileyLink.TxLevelUp());
                    continue;
                }

                var p = this.GetPacket(received);
                if (p == null)
                {
                    this.RlStatistics.BadDataReceived(received);
                    await RunInUiContext(async () => await RileyLink.TxLevelDown());
                    continue;
                }

                Debug.WriteLine($"RECV PKT {p}");
                if (p.address != this.Pod.RadioAddress)
                {
                    this.RlStatistics.BadPacketReceived(p);
                    Debug.WriteLine("RECV PKT ADDR MISMATCH");
                    await RunInUiContext(async () => await RileyLink.TxLevelDown());
                    continue;
                }

                this.last_packet_timestamp = Environment.TickCount;

                if (this.last_received_packet != null && p.sequence == this.last_received_packet.sequence
                    && p.type == this.last_received_packet.type)
                {
                    this.RlStatistics.RepeatPacketReceived(p);
                    Debug.WriteLine("RECV PKT previous");
                    await RunInUiContext(async () => await RileyLink.TxLevelUp());
                    continue;
                }

                this.last_received_packet = p;
                this.Pod.RuntimeVariables.PacketSequence = (p.sequence + 1) % 32;

                if (p.type != expected_type)
                {
                    this.RlStatistics.UnexpectedPacketReceived(p);
                    Debug.WriteLine("RECV PKT unexpected type");
                    throw new OmniCoreErosException(FailureType.PodResponseUnexpected, "Unexpected packet type received", p);
                }

                if (p.sequence != (packet_to_send.sequence + 1) % 32)
                {
                    this.Pod.RuntimeVariables.PacketSequence = (p.sequence + 1) % 32;
                    Debug.WriteLine("RECV PKT unexpected sequence");
                    this.last_received_packet = p;
                    this.RlStatistics.UnexpectedPacketReceived(p);
                    throw new OmniCoreErosException(FailureType.PodResponseUnexpected, "Incorrect packet sequence received", p);
                }

                this.RlStatistics.PacketReceived(p);
                return p;

            }
            throw new OmniCoreTimeoutException(FailureType.PodUnreachable, "Exceeded timeout while send and receive");
        }

        private async Task SendPacket(RadioPacket packet_to_send, int timeout = 25000)
        {
            int start_time = 0;
            Bytes received = null;
            while (start_time == 0 || Environment.TickCount - start_time < timeout)
            {
                try
                {
                    Debug.WriteLine($"SEND PKT {packet_to_send}");

                    try
                    {
                        received = await RunInUiContext(async () => await RileyLink.SendAndGetPacket(packet_to_send.get_data(), 0, 0, 300, 3, 300));
                    }
                    catch(OmniCoreTimeoutException)
                    {
                        Debug.WriteLine("Silence fell.");
                        this.Pod.RuntimeVariables.PacketSequence++;
                        return;
                    }

                    if (start_time == 0)
                        start_time = Environment.TickCount;

                    var p = this.GetPacket(received);
                    if (p == null)
                    {
                        await RunInUiContext(async () => await RileyLink.TxLevelDown());
                        continue;
                    }

                    if (p.address != this.Pod.RadioAddress)
                    {
                        Debug.WriteLine("RECV PKT ADDR MISMATCH");
                        await RunInUiContext(async () => await RileyLink.TxLevelDown());
                        continue;
                    }

                    this.last_packet_timestamp = Environment.TickCount;
                    if (this.last_received_packet != null && p.type == this.last_received_packet.type
                        && p.sequence == this.last_received_packet.sequence)
                    {
                        Debug.WriteLine("RECV PKT previous");
                        await RunInUiContext(async () => await RileyLink.TxLevelUp());
                        continue;
                    }

                    Debug.WriteLine($"RECV PKT {p}");
                    Debug.WriteLine($"RECEIVED unexpected packet");
                    this.last_received_packet = p;
                    this.Pod.RuntimeVariables.PacketSequence = p.sequence + 1;
                    packet_to_send.with_sequence(this.Pod.RuntimeVariables.PacketSequence);
                    start_time = Environment.TickCount;
                    continue;
                }
                catch (OmniCoreRadioException pre)
                {
                    Debug.WriteLine($"Radio error during send, retrying {pre}");
                    await RunInUiContext(async () => await RileyLink.Reset());
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

            var msgSequence = Pod.Status != null ? (Pod.Status.MessageSequence + 1 % 16) : 0;
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
            int sequence = Pod.RuntimeVariables.PacketSequence;
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

        public IMessageExchangeResult ParseResponse(IMessage response, IPod pod)
        {
            try
            {
                foreach (var mp in response.GetParts())
                {
                    var er = mp as ErosResponse;
                    er.Parse(pod);
                }
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }

            return new MessageExchangeResult();
        }
    }
}