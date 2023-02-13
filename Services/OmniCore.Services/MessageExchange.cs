using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces;
using Plugin.BLE.Abstractions;
using Trace = System.Diagnostics.Trace;

namespace OmniCore.Services
{
    public class CommunicationResult
    {
        public bool IsSuccessful { get; set; }
        public int NextMessageSequence { get; set; }
        public int NextPacketSequence { get; set; }
        public RadioMessage Response { get; set; }
    }
    
    public class MessageExchange
    {
        private uint PacketAddressOut { get; set; }
        private uint PacketAddressIn { get; set; }
        private uint AssignedAddress { get; set; }
        private int NextPacketSequence { get; set; }
        private int NextMessageSequence { get; set; }
        private int _startingPacketSequence;
        private int _startingMessageSequence;

        private RadioConnection RadioConnection;
        private RadioMessage MessageToSend;
        private DateTimeOffset? LastPacketReceived;
        
        public MessageExchange(
            RadioMessage messageToSend,
            RadioConnection radioConnection,
            int startPacketSequence)
        {
            PacketAddressOut = messageToSend.Address;
            PacketAddressIn = messageToSend.Address;
            _startingMessageSequence = messageToSend.Sequence;
            _startingPacketSequence = startPacketSequence;
            MessageToSend = messageToSend;
            RadioConnection = radioConnection;
        }

        public async Task<CommunicationResult> RunExchangeAsync(CancellationToken cancellationToken = default, bool doFinalAck = true)
        {
            var messageBody = MessageToSend.GetBody();
            var sendPacketCount = messageBody.Length / 31 + 1;
            int sendPacketIndex = 0;
            RadioPacket receivedPacket = null;
            var exchangeStarted = DateTimeOffset.Now;
            NextPacketSequence = _startingPacketSequence;
            NextMessageSequence = _startingMessageSequence;

            while (sendPacketIndex < sendPacketCount)
            {
                int byteStart = sendPacketIndex * 31;
                var byteEnd = byteStart + 31;
                if (messageBody.Length < byteEnd)
                    byteEnd = messageBody.Length;

                var packetToSend = new RadioPacket(
                    PacketAddressOut,
                    sendPacketIndex == 0 ? RadioPacketType.Pdm : RadioPacketType.Con,
                    NextPacketSequence,
                    messageBody.Sub(byteStart, byteEnd));

                receivedPacket = await TryExchangePackets(packetToSend, sendPacketIndex == 0, cancellationToken);

                if (receivedPacket == null || receivedPacket.Address != PacketAddressIn)
                {
                    Trace.WriteLine($"No response");

                    if (LastPacketReceived.HasValue && LastPacketReceived < DateTimeOffset.Now - TimeSpan.FromSeconds(25))
                    {
                        // partial comm timed out
                        return new CommunicationResult
                        {
                            IsSuccessful = false,
                            NextMessageSequence = _startingMessageSequence,
                            NextPacketSequence = _startingPacketSequence,
                            Response = null
                        };
                    }
                    continue;
                }

                LastPacketReceived = DateTimeOffset.Now;

                if (receivedPacket.Sequence != (NextPacketSequence + 1) % 32)
                {
                    Trace.WriteLine($"Received unexpected packet sequence {receivedPacket.Address.ToString("x8")}");
                    continue;
                }
                
                NextPacketSequence = (receivedPacket.Sequence + 1) % 32;

                if (sendPacketIndex == sendPacketCount - 1)
                {
                    // last send packet
                    if (receivedPacket.Type != RadioPacketType.Pod)
                    {
                        Trace.WriteLine($"Expected Pod response, received: {receivedPacket}");
                        continue;
                    }
                }
                else
                {
                    // interim send packet
                    if (receivedPacket.Type != RadioPacketType.Ack)
                    {
                        Trace.WriteLine($"Expected Ack to continue sending message, received: {receivedPacket}");
                        continue;
                    }
                }
                sendPacketIndex++;
            }

            if (receivedPacket == null)
            {
                return new CommunicationResult
                {
                    IsSuccessful = false,
                    NextMessageSequence = NextMessageSequence,
                    NextPacketSequence = NextPacketSequence,
                    Response = null
                };
            }
            
            var b0 = receivedPacket.Data[4];
            var b1 = receivedPacket.Data[5];

            int receivedMessageSequence = (b0  & 0b00111100) >> 2;
            int responseMessageLength = ((b0 & 0x03) << 8 | b1) + 4 + 2 + 2;
            int receivedMessageLength = receivedPacket.Data.Length;

            var podResponsePackets = new List<RadioPacket>();
            podResponsePackets.Add(receivedPacket);
            
            while (receivedMessageLength < responseMessageLength)
            {
                RadioPacket interimAck = new RadioPacket(PacketAddressOut, RadioPacketType.Ack,
                    NextPacketSequence, new Bytes(PacketAddressOut)
                );

                receivedPacket = await TryExchangePackets(interimAck, false, cancellationToken);
                
                if (receivedPacket == null || receivedPacket.Address != PacketAddressIn)
                {
                    Trace.WriteLine($"No response");
                    if (LastPacketReceived.HasValue && LastPacketReceived < DateTimeOffset.Now - TimeSpan.FromSeconds(25))
                    {
                        // partial comm timed out
                        return new CommunicationResult
                        {
                            IsSuccessful = false,
                            NextMessageSequence = _startingMessageSequence,
                            NextPacketSequence = _startingPacketSequence,
                            Response = null
                        };
                    }
                    continue;
                }
                
                LastPacketReceived = DateTimeOffset.Now;
                if (receivedPacket.Sequence != (NextPacketSequence + 1) % 32)
                {
                    Trace.WriteLine($"Received unexpected packet sequence {receivedPacket.Address.ToString("x8")}");
                    continue;
                }
                
                if (receivedPacket.Type != RadioPacketType.Con)
                {
                    Trace.WriteLine($"Expected type Con, received: {receivedPacket}");
                    continue;
                }
                if (receivedPacket.Data.Length + receivedMessageLength > responseMessageLength)
                {
                    Trace.WriteLine($"Received message exceeds expected data length! last received: {receivedPacket}");
                    continue;
                }
                podResponsePackets.Add(receivedPacket);
                NextPacketSequence = (receivedPacket.Sequence + 1) % 32;
                receivedMessageLength += receivedPacket.Data.Length;
            }
          
            if (doFinalAck)
            {
                RadioPacket finalAck = new RadioPacket(PacketAddressOut, RadioPacketType.Ack,
                    NextPacketSequence, new Bytes(new byte[] { 0, 0, 0, 0 }));

                var nrCount = 0;
                while (nrCount < 3)
                {
                    Debug.WriteLine($"Final ack, nrc: {nrCount}");
                    var received = await TryExchangePackets(finalAck, false, cancellationToken);
                    if (received == null)
                        nrCount++;
                    else
                        nrCount = 0;
                }

                Debug.WriteLine($"Final send complete");
                NextPacketSequence = (NextPacketSequence + 1) % 32;
            }

            var podMessageReceived = RadioMessage.FromReceivedPackets(podResponsePackets);
            NextMessageSequence = (receivedMessageSequence + 1) % 16;
            return new CommunicationResult
            {
                IsSuccessful = true,
                NextMessageSequence = NextMessageSequence,
                NextPacketSequence = NextPacketSequence,
                Response = podMessageReceived
            };
        }

        private async Task<RadioPacket> TryExchangePackets(
            RadioPacket packetToSend,
            bool firstPacket,
            CancellationToken cancellationToken)
        {
            RadioPacket received = null;
            Debug.WriteLine($"SEND: {packetToSend}");
            if (firstPacket)
            {
                received = await RadioConnection.SendAndTryGetPacket(
                    0, 0, 0, 150,
                    0, 250, 0, packetToSend, cancellationToken);
            }
            else
            {
                received =  await RadioConnection.SendAndTryGetPacket(
                    0, 3, 25, 0,
                    0, 250, 0, packetToSend, cancellationToken);
            }

            if (received != null)
                Debug.WriteLine($"RCVD: {received}");
            else
                Debug.WriteLine($"RCVD: --------");
            return received;
        }
    }
}