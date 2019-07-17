using Microsoft.AppCenter.Crashes;
using OmniCore.Mobile.Base;
using OmniCore.Model;
using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Eros.Data;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Radio.RileyLink
{
    public class RileyLinkMessageExchange : IMessageExchange
    {
        public RileyLink RileyLink { get; set; }
        public RadioPacket LastReceivedPacket;

        public int LastPacketSent = 0;

        private Task FinalAckTask = null;

        public RileyLinkMessageExchange(IMessageExchangeParameters messageExchangeParameters, CancellationToken token, IPod pod)
        {
            Pod = pod;
            Token = token;
            Parameters = messageExchangeParameters;
            Statistics = new RileyLinkStatistics();
            Result = new ErosMessageExchangeResult();
        }

        public async Task InitializeExchange()
        {
            if (FinalAckTask != null)
            {
                ActionText = "Waiting for previous radio operation to complete";
                await FinalAckTask;
                FinalAckTask = null;
            }

            if (RileyLink == null)
            {
                RileyLink = new RileyLink(this);
            }

            await RileyLink.EnsureDevice();
        }

        public async Task FinalizeExchange()
        {
            if (FinalAckTask != null)
            {
                await FinalAckTask;
                FinalAckTask = null;
            }
        }

        public void SetException(Exception exception)
        {
            Result.Success = false;
            var oe = exception as OmniCoreException;
            Result.Failure = oe?.FailureType ?? FailureType.Unknown;
            Result.Exception = exception;
        }

        public string ActionText { get; set; }
        public bool CanBeCanceled { get; set; }
        public bool Waiting { get; set; }
        public bool Running { get; set; }
        public bool Finished { get; set; }
        public int Progress { get; set; }
        public CancellationToken Token { get; }

        public IMessageExchangeResult Result { get; set; }
        public IMessageExchangeStatistics Statistics { get; set; }
        public RileyLinkStatistics RileyLinkStatistics
        {
            get => (RileyLinkStatistics)Statistics;
        }

        public IMessageExchangeParameters Parameters { get; set; }
        public ErosMessageExchangeParameters ErosParameters
        {
            get => (ErosMessageExchangeParameters)Parameters;
        }

        public IPod Pod { get; }

        private ErosPod ErosPod
        {
            get => (ErosPod) Pod;
        }

        public async Task<IMessage> GetResponse(IMessage requestMessage)
        {
            var response = await GetResponseInternal(requestMessage);
            foreach (var mp in response.GetParts())
            {
                var erosResponse = mp as ErosResponse;
                erosResponse.Parse(Pod, this);
            }

            return response;
        }

        private async Task<IMessage> GetResponseInternal(IMessage requestMessage)
        {
            try
            {
                RileyLinkStatistics.StartMessageExchange();
                if (Parameters.TransmissionLevelOverride.HasValue)
                {
                    await RileyLink.SetTxLevel(Parameters.TransmissionLevelOverride.Value);
                }

                RadioPacket received = null;
                bool sendMessage = true;

                var exchangeTimeout1 = ErosParameters.FirstExchangeTimeout ?? 90000;
                var exchangeTimeout2 = ErosParameters.SubsequentExchangeTimeout ?? 180000;
                while (sendMessage)
                {
                    sendMessage = false;
                    var erosRequestMessage = requestMessage as ErosMessage;
                    var packets = GetRadioPackets(erosRequestMessage);
                    var packetCount = packets.Count;
                    try
                    {
                        for (int packetIndex = 0; packetIndex < packetCount; packetIndex++)
                        {
                            var packetToSend = packets[packetIndex];
                            RileyLinkStatistics.StartPacketExchange();
                            ActionText = $"Sending radio packet {packetIndex + 1} of {packetCount}";
                            if (packetIndex == 0)
                                received = await ExchangePacketWithRetries(packetToSend, packetIndex == packetCount - 1 ? PacketType.POD : PacketType.ACK, exchangeTimeout1);
                            else
                                received = await ExchangePacketWithRetries(packetToSend, packetIndex == packetCount - 1 ? PacketType.POD : PacketType.ACK, exchangeTimeout2);

                            RileyLinkStatistics.EndPacketExchange();
                            ErosPod.RuntimeVariables.PacketSequence = (received.Sequence + 1) % 32;
                        }
                    }
                    catch(Exception)
                    {
                        throw;
                    }
                }

                var responseBuilder = new ErosResponseBuilder();

                var messageAddress = ErosParameters.AddressOverride ?? Pod.RadioAddress;
                var ackAddress = ErosParameters.AckAddressOverride ?? Pod.RadioAddress;

                int podResponsePacketCount = 1;
                while (!responseBuilder.WithRadioPacket(received))
                {
                    podResponsePacketCount++;
                    ActionText = $"Waiting further response from pod (part #{podResponsePacketCount})";
                    RileyLinkStatistics.StartPacketExchange();
                    var ackPacket = CreateAckPacket(messageAddress, ackAddress, (received.Sequence + 1) % 32);
                    received = await ExchangePacketWithRetries(ackPacket, PacketType.CON, 30000);
                    RileyLinkStatistics.EndPacketExchange();
                }

                RileyLinkStatistics.EndMessageExchange();
                var podResponse = responseBuilder.Build();

                Debug.WriteLine($"RCVD MSG {podResponse}");
                Debug.WriteLine("Send and receive completed.");
                ErosPod.RuntimeVariables.PacketSequence = (received.Sequence + 1) % 32;

                ActionText = $"Ending conversation";
                RadioPacket finalAckPacket;
                if (messageAddress == ackAddress)
                    finalAckPacket = CreateAckPacket(messageAddress, 0, ErosPod.RuntimeVariables.PacketSequence);
                else
                    finalAckPacket = CreateAckPacket(messageAddress, ackAddress, ErosPod.RuntimeVariables.PacketSequence);

                FinalAckTask = Task.Run(() => AcknowledgeEndOfMessage(finalAckPacket));
                return podResponse;
            }
            catch (Exception)
            {
                RileyLinkStatistics.ExitPrematurely();
                throw;
            }
        }

        private async Task<RadioPacket> ExchangePacketWithRetries(RadioPacket packetToSend,
            PacketType expectedPacketType, int exchangeTimeout)
        {
            int timeoutCount = 0;
            int radioErrorCount = 0;
            int protocolErrorCount = 0;
            int exchangeStart = Environment.TickCount;
            while (exchangeStart + exchangeTimeout > Environment.TickCount)
            {
                packetToSend = packetToSend.WithSequence(this.ErosPod.RuntimeVariables.PacketSequence);
                try
                {
                    var receivedPacket = await this.ExchangePackets(packetToSend, expectedPacketType);
                    RileyLinkStatistics.PacketSent(packetToSend);
                    if (receivedPacket != null)
                        return receivedPacket;
                }
                catch (OmniCoreTimeoutException ote)
                {
                    timeoutCount++;
                    radioErrorCount = 0;
                    RileyLinkStatistics.TimeoutOccured(ote);
                    await HandleTimeoutException(ote, timeoutCount);
                }
                catch (OmniCoreRadioException pre)
                {
                    timeoutCount = 0;
                    radioErrorCount++;
                    RileyLinkStatistics.RadioErrorOccured(pre);
                    await HandleRadioException(pre, radioErrorCount);
                }
                catch (OmniCoreErosException pe)
                {
                    timeoutCount = 0;
                    radioErrorCount = 0;
                    protocolErrorCount++;
                    RileyLinkStatistics.ProtocolErrorOccured(pe);
                    await HandleProtocolException(pe, protocolErrorCount);
                }
                catch (Exception e)
                {
                    RileyLinkStatistics.UnknownErrorOccured(e);
                    Crashes.TrackError(e);
                    throw;
                }
            }
            throw new OmniCoreTimeoutException(FailureType.CommunicationInterrupted);
        }

        private async Task HandleTimeoutException(OmniCoreTimeoutException ote, int timeoutCount)
        {
            RileyLinkStatistics.NoPacketReceived();
            if (timeoutCount == 0)
                ActionText = $"Timed out waiting for pod response";
            else
                ActionText = $"Timed out waiting for pod response (retry: #{timeoutCount})";

            if (timeoutCount %3 == 0)
                await RileyLink.TxLevelUp();

            if (timeoutCount == 10)
                await Task.Delay(2000);

            if (timeoutCount == 15)
                await Task.Delay(2000);

            if (timeoutCount == 25)
                await Task.Delay(2000);

            if (timeoutCount == 30)
                ErosPod.RuntimeVariables.PacketSequence = 0;

            if (timeoutCount > 40)
                throw ote;
        }

        private async Task HandleRadioException(OmniCoreRadioException pre, int radioErrorCount)
        {
            if (radioErrorCount == 0)
                ActionText = $"Error communicating with RileyLink";
            else
                ActionText = $"Error communicating with RileyLink (retry: #{radioErrorCount})";

            if (radioErrorCount % 2 == 1)
                await Task.Delay(2000);

            if (radioErrorCount == 6)
                ErosPod.RuntimeVariables.PacketSequence = 0;

            if (radioErrorCount > 10)
                throw pre;
        }

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        private async Task HandleProtocolException(OmniCoreErosException pe, int protocolErrorCount)
        {
            throw new OmniCoreProtocolException(FailureType.PodResponseUnexpected);
        }

        private async Task AcknowledgeEndOfMessage(RadioPacket ackPacket)
        {
            try
            {
                using (var wakeLock = OmniCoreServices.Application.NewBluetoothWakeLock("OmniCore_FinalAck"))
                {
                    await wakeLock.Acquire(5000);
                    await SendPacket(ackPacket);
                }
            }
            catch(Exception)
            {
                Debug.WriteLine("Final ack failed, ignoring.");
            }
        }

        private async Task<RadioPacket> ExchangePackets(RadioPacket packet_to_send, PacketType expected_type)
        {
            Bytes receivedData = null;
            Debug.WriteLine($"SEND PKT {packet_to_send}");

            uint firstTimeout = ErosParameters.FirstPacketTimeout ?? 300;
            uint subsequentTimeout = ErosParameters.SubsequentPacketTimeout ?? 120;
            ushort firstSeed = ErosParameters.FirstPacketPreambleLength ?? 300;
            ushort subsequentSeed = ErosParameters.SubsequentPacketPreambleLength ?? 40;

            if (this.LastPacketSent == 0 || (Environment.TickCount - this.LastPacketSent) > 500)
                receivedData = await RileyLink.SendAndGetPacket(packet_to_send.GetPacketData(), 0, 0, firstTimeout, 0, firstSeed);
            else
                receivedData = await RileyLink.SendAndGetPacket(packet_to_send.GetPacketData(), 0, 0, subsequentTimeout, 0, subsequentSeed);

            var receivedPacket = this.GetPacket(receivedData);
            if (receivedPacket == null)
            {
                RileyLinkStatistics.BadDataReceived(receivedData);
                if (ErosParameters.AllowAutoLevelAdjustment)
                    await RileyLink.TxLevelDown();
                return null;
            }

            Debug.WriteLine($"RECV PKT {receivedPacket}");
            if (receivedPacket.Address != this.Pod.RadioAddress && receivedPacket.Address != 0xFFFFFFFF)
            {
                RileyLinkStatistics.BadPacketReceived(receivedPacket);
                Debug.WriteLine("RECV PKT ADDR MISMATCH");
                if (ErosParameters.AllowAutoLevelAdjustment)
                    await RileyLink.TxLevelDown();
                return null;
            }

            this.LastPacketSent = Environment.TickCount;

            if (this.LastReceivedPacket != null && receivedPacket.Sequence == this.LastReceivedPacket.Sequence
                && receivedPacket.Type == this.LastReceivedPacket.Type)
            {
                RileyLinkStatistics.RepeatPacketReceived(receivedPacket);
                Debug.WriteLine("RECV PKT previous");
                if (ErosParameters.AllowAutoLevelAdjustment)
                    await RileyLink.TxLevelUp();
                return null;
            }

            this.LastReceivedPacket = receivedPacket;
            this.ErosPod.RuntimeVariables.PacketSequence = (receivedPacket.Sequence + 1) % 32;

            if (receivedPacket.Type != expected_type)
            {
                RileyLinkStatistics.UnexpectedPacketReceived(receivedPacket);
                Debug.WriteLine("RECV PKT unexpected type");
                throw new OmniCoreErosException(FailureType.PodResponseUnexpected, "Unexpected packet type received", receivedPacket, expected_type);
            }

            if (receivedPacket.Sequence != (packet_to_send.Sequence + 1) % 32)
            {
                this.ErosPod.RuntimeVariables.PacketSequence = (receivedPacket.Sequence + 1) % 32;
                Debug.WriteLine("RECV PKT unexpected sequence");
                this.LastReceivedPacket = receivedPacket;
                RileyLinkStatistics.UnexpectedPacketReceived(receivedPacket);
                throw new OmniCoreErosException(FailureType.PodResponseUnexpected, "Incorrect packet sequence received", receivedPacket);
            }

            RileyLinkStatistics.PacketReceived(receivedPacket);
            return receivedPacket;
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
                        received = await RileyLink.SendAndGetPacket(packet_to_send.GetPacketData(), 0, 0, 300, 3, 300);
                    }
                    catch(OmniCoreTimeoutException)
                    {
                        Debug.WriteLine("Silence fell.");
                        this.ErosPod.RuntimeVariables.PacketSequence++;
                        return;
                    }

                    if (start_time == 0)
                        start_time = Environment.TickCount;

                    var p = this.GetPacket(received);
                    if (p == null)
                    {
                        if (ErosParameters.AllowAutoLevelAdjustment)
                            await RileyLink.TxLevelDown();
                        continue;
                    }

                    if (p.Address != this.Pod.RadioAddress && p.Address != 0xFFFFFFFF)
                    {
                        Debug.WriteLine("RECV PKT ADDR MISMATCH");
                        if (ErosParameters.AllowAutoLevelAdjustment)
                            await RileyLink.TxLevelDown();
                        continue;
                    }

                    this.LastPacketSent = Environment.TickCount;
                    if (this.LastReceivedPacket != null && p.Type == this.LastReceivedPacket.Type
                        && p.Sequence == this.LastReceivedPacket.Sequence)
                    {
                        Debug.WriteLine("RECV PKT previous");
                        if (ErosParameters.AllowAutoLevelAdjustment)
                            await RileyLink.TxLevelUp();
                        continue;
                    }

                    Debug.WriteLine($"RECV PKT {p}");
                    Debug.WriteLine($"RECEIVED unexpected packet");
                    this.LastReceivedPacket = p;
                    this.ErosPod.RuntimeVariables.PacketSequence = p.Sequence + 1;
                    packet_to_send.WithSequence(ErosPod.RuntimeVariables.PacketSequence);
                    start_time = Environment.TickCount;
                    continue;
                }
                catch (OmniCoreRadioException pre)
                {
                    Debug.WriteLine($"Radio error during send, retrying {pre}");
                    await Task.Delay(2000);
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
                try
                {
                    var rp = RadioPacket.Parse(data.Sub(2));
                    if (rp != null)
                        rp.Rssi = (sbyte)data[0];
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

        public List<RadioPacket> GetRadioPackets(ErosMessage message)
        {
            var message_body_len = 0;
            foreach (var p in message.GetParts())
            {
                var ep = p as ErosRequest;
                var cmd_body = ep.PartData;
                message_body_len += (int)cmd_body.Length + 2;
                if (ep.RequiresNonce)
                    message_body_len += 4;
            }

            byte b0 = 0;
            if (ErosParameters.CriticalWithFollowupRequired)
                b0 = 0x80;

            var msgSequence = Pod != null ? (Pod.MessageSequence + 1 % 16) : 0;
            if (ErosParameters.MessageSequenceOverride.HasValue)
                msgSequence = ErosParameters.MessageSequenceOverride.Value;

            b0 |= (byte)(msgSequence << 2);
            b0 |= (byte)((message_body_len >> 8) & 0x03);
            byte b1 = (byte)(message_body_len & 0xff);

            var msgAddress = Pod.RadioAddress;
            if (ErosParameters.AddressOverride.HasValue)
                msgAddress = ErosParameters.AddressOverride.Value;

            var message_body = new Bytes(msgAddress);
            message_body.Append(b0);
            message_body.Append(b1);

            foreach (var p in message.GetParts())
            {
                var ep = p as ErosRequest;
                var cmd_type = (byte) ep.PartType;
                var cmd_body = ep.PartData;

                if (ep.RequiresNonce)
                {
                    message_body.Append(cmd_type);
                    message_body.Append((byte)(cmd_body.Length + 4));

                    if (ErosPod.RuntimeVariables.NonceSync.HasValue)
                        ErosParameters.Nonce.Sync(msgSequence);

                    var nonce = ErosParameters.Nonce.GetNext();
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
                message_body.Append(cmd_body);
            }
            var crc_calculated = CrcUtil.Crc16(message_body.ToArray());
            message_body.Append(crc_calculated);

            int index = 0;
            bool first_packet = true;
            int sequence = ErosPod.RuntimeVariables.PacketSequence;
            var radio_packets = new List<RadioPacket>();

            while (index < message_body.Length)
            {
                var to_write = Math.Min(31, message_body.Length - index);
                var packet_body = message_body.Sub(index, index + to_write);
                radio_packets.Add(new RadioPacket(msgAddress,
                                                first_packet ? PacketType.PDM : PacketType.CON,
                                                sequence,
                                                packet_body));
                first_packet = false;
                sequence = (sequence + 2) % 32;
                index += to_write;
            }

            if (ErosParameters.RepeatFirstPacket)
            {
                var fp = radio_packets[0];
                radio_packets.Insert(0, fp);
            }
            return radio_packets;
        }
    }
}