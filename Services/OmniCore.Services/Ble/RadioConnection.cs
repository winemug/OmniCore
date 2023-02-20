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
        
        public async Task<PodPacket> TryGetPacket(
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
            return PodPacket.FromRadioData(data, rssi);
        }

        public async Task<bool> SendPacket(
            byte channel,
            byte repeatCount,
            ushort delayMilliseconds,
            ushort preambleExtensionMs,
            PodPacket packet,
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
        
        public async Task<PodPacket> SendAndTryGetPacket(
            byte sendChannel,
            byte sendRepeatCount,
            ushort sendRepeatDelayMs,
            ushort sendPreambleExtensionMs,
            byte listenChannel,
            uint listenTimeoutMs,
            byte listenRetryCount,
            PodPacket packet,
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

            var data = new Bytes(result.Skip(2).ToArray());
            return PodPacket.FromRadioData(data, rssi);
        }
    }
}