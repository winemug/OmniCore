using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Services.Interfaces;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Forms;
using Trace = System.Diagnostics.Trace;

namespace OmniCore.Services
{
    public class RadioConnection : IDisposable
    {
        private Radio _radio;
        private IDisposable _radioLockDisposable;

        public RadioConnection(Radio radio, IDisposable radioLockDisposable)
        {
            _radio = radio;
            _radioLockDisposable = radioLockDisposable;
        }

        public void Dispose()
        {
            _radioLockDisposable?.Dispose();
        }

        private async Task TrySendFirstPacket()
        {
            
        }
        
        //     var state = new MessageCommunication
        //     {
        //     };
        //     
        //     var sendPackets = messageToSend.GetPacketsForSending(nextPacketSequence);
        //     var receivedPackets = new List<RadioPacket>();
        //     int sendIndex = 0;
        //     while(sendIndex < sendPackets.Count)
        //     {
        //         var packetToSend = sendPackets[sendIndex];
        //         RadioPacket receivedPacket;
        //         if (state.PodAwake)
        //         {
        //             receivedPacket = await SendAndTryGetPacket(packetToSend, 5, 15, 145,
        //                 2, 5, cancellationToken);
        //         }
        //         else
        //         {
        //             receivedPacket = await SendAndTryGetPacket(packetToSend, 0, 0, 150,
        //                 0, 150, cancellationToken);
        //         }
        //
        //         if (receivedPacket != null && receivedPacket.Address == packetToSend.Address)
        //         {
        //             state.PodAwake = true;
        //             var expectedSequence = (nextPacketSequence + 1) % 32;
        //             if (receivedPacket.Sequence != expectedSequence)
        //             {
        //                 // wrn - sequence mismatch
        //                 Trace.WriteLine($"Incoming packet sequence mismatch, expected {expectedSequence} received: {receivedPacket.Sequence}");
        //                 nextPacketSequence = (receivedPacket.Sequence + 1) % 32;
        //             }
        //             
        //             if (receivedPacket.Type == RadioPacketType.Ack)
        //             {
        //                 if (sendIndex == sendPackets.Count - 1)
        //                 {
        //                     // err - ack received, pod expected
        //                     Trace.WriteLine($"Pod not in sync");
        //                 }
        //
        //                 if (receivedPacket.Data.Length != 4)
        //                 {
        //                     // err
        //                     Trace.WriteLine($"Received ack data unexpected, data len: {receivedPacket.Data.Length}");
        //                     // retry message send
        //                 }
        //                 var receivedAckAddress = new Bytes(receivedPacket.Data).DWord(0);
        //                 
        //                 if (receivedAckAddress == messageToSend.Address)
        //                 {
        //                     sendIndex++;
        //                     nextPacketSequence += 2;
        //                     nextPacketSequence %= 32;
        //                 }
        //                 else
        //                 {
        //                     // err - unknown ack address received
        //                 }
        //             }
        //             else if (receivedPacket.Type == RadioPacketType.Pod)
        //             {
        //                 if (sendIndex == sendPackets.Count - 1)
        //                 {
        //                     sendIndex++;
        //                     nextPacketSequence += 2;
        //                     nextPacketSequence %= 32;
        //                     receivedPackets.Add(receivedPacket);
        //                 }
        //                 else
        //                 {
        //                     // err pod type received while expecting ack response
        //                 }
        //             }
        //             else
        //             {
        //                 // err unexpected packet type (con or unknown)
        //             }
        //         }
        //         else
        //         {
        //             // err - no packet received
        //             await HandleNoResponseReceivedAsync(state, cancellationToken);
        //         }
        //     }
        //
        //     private async Task<bool> HandleNoResponseReceivedAsync(MessageCommunication state, CancellationToken cancellationToken)
        //     {
        //     }
        //
        //     
        //     var receivedMessage = RadioMessage.FromReceivedPackets(receivedPackets);
        //     while (receivedMessage == null)
        //     {
        //         var ackAddress = messageToSend.Address;
        //         if (messageToSend.AckAddressOverride.HasValue)
        //             ackAddress = messageToSend.AckAddressOverride.Value;
        //         
        //         var ackPacketToSend = new RadioPacket(messageToSend.Address, RadioPacketType.Ack,
        //             nextPacketSequence, new byte[]
        //             {
        //                 (byte)(ackAddress >> 24 & 0xFF),
        //                 (byte)(ackAddress >> 16 & 0xFF),
        //                 (byte)(ackAddress >> 8 & 0xFF),
        //                 (byte)(ackAddress & 0xFF),
        //             });
        //         var receivedPodDataPacket = await SendAndTryGetPacket(ackPacketToSend, 5, 15, 145,
        //             2, 5, cancellationToken);
        //         if (receivedPodDataPacket == null)
        //         {
        //             // err - no packet received
        //         }
        //         
        //         var podExpectedSequence = (nextPacketSequence + 1) % 32;
        //         
        //         if (receivedPodDataPacket.Sequence != podExpectedSequence)
        //         {
        //             // err - sequence mismatch
        //         }
        //         if (receivedPodDataPacket.Type != RadioPacketType.Con)
        //         {
        //             // err unexpected packet type, not con
        //         }
        //
        //         receivedPackets.Add(receivedPodDataPacket);
        //         nextPacketSequence += 2;
        //         nextPacketSequence %= 32;
        //     }
        //     
        //     uint finalAckAddress = 0;
        //     if (messageToSend.AckAddressOverride.HasValue)
        //         finalAckAddress = messageToSend.AckAddressOverride.Value;
        //         
        //     var finalPacketToSend = new RadioPacket(messageToSend.Address, RadioPacketType.Ack,
        //         nextPacketSequence, new byte[]
        //         {
        //             (byte)(finalAckAddress >> 24 & 0xFF),
        //             (byte)(finalAckAddress >> 16 & 0xFF),
        //             (byte)(finalAckAddress >> 8 & 0xFF),
        //             (byte)(finalAckAddress & 0xFF),
        //         });
        //
        //     var finalPacketSent = false;
        //     while (!finalPacketSent)
        //     {
        //         finalPacketSent = await SendPacket(finalPacketToSend, 5, 15, 5, cancellationToken);
        //     }
        //     
        //     nextMessageSequence += 2;
        //     nextMessageSequence %= 16;
        //
        //     return (receivedMessage, nextPacketSequence, nextMessageSequence);
        // }
        
        public async Task<RadioPacket> TryGetPacket(
            byte channel,
            uint timeoutMs,
            CancellationToken cancellationToken = default)
        {
            var cmdParams = new Bytes()
                .Append(channel)
                .Append(timeoutMs)
                .ToArray();
            var (code, result) = await _radio.ExecuteCommandAsync(
                RileyLinkCommand.GetPacket, cancellationToken,
                cmdParams);
            if (code != RileyLinkResponse.CommandSuccess)
                return null;
        
            if (result.Length < 3)
                return null;
        
            var rssi = (((sbyte)result[0]) - 127) / 2; // -128 to 127
            var sequence = (byte)result[1];
            // var seq = result[1];
            var data = new Bytes(result.Skip(2).ToArray());
            return RadioPacket.FromRadioData(data, rssi);
        }

        public async Task<bool> SendPacket(
            byte channel,
            byte repeatCount,
            ushort delayMilliseconds,
            ushort preambleExtensionMs,
            RadioPacket packet,
            CancellationToken cancellationToken)
        {
            var cmdParamsWithData = new Bytes()
                .Append(channel)
                .Append(repeatCount)
                .Append(delayMilliseconds)
                .Append(preambleExtensionMs)
                .Append(packet.ToRadioData())
                .ToArray();
            var (code, result) = await _radio.ExecuteCommandAsync(
                RileyLinkCommand.SendPacket, cancellationToken,
                cmdParamsWithData);
            return code == RileyLinkResponse.CommandSuccess;
        }
        
        public async Task<RadioPacket> SendAndTryGetPacket(
            byte sendChannel,
            byte sendRepeatCount,
            ushort sendRepeatDelayMs,
            ushort sendPreambleExtensionMs,
            byte listenChannel,
            uint listenTimeoutMs,
            byte listenRetryCount,
            RadioPacket packet,
            CancellationToken cancellationToken)
        {
            var cmdParamsWithData = new Bytes()
                .Append(sendChannel)
                .Append(sendRepeatCount)
                .Append(sendRepeatDelayMs)
                .Append(listenChannel)
                .Append(listenTimeoutMs)
                .Append(listenRetryCount)
                .Append(sendPreambleExtensionMs)
                .Append(packet.ToRadioData())
                .ToArray();
            var (code, result) = await _radio.ExecuteCommandAsync(
                RileyLinkCommand.SendAndListen, cancellationToken,
                cmdParamsWithData);
            if (code != RileyLinkResponse.CommandSuccess)
                return null;
            if (result.Length < 3)
                return null;
        
            var rssi = (((sbyte)result[0]) - 127) / 2; // -128 to 127
            var sequence = (byte)result[1];
            // var seq = result[1];
            var data = new Bytes(result.Skip(2).ToArray());
            return RadioPacket.FromRadioData(data, rssi);
        }
    }
}