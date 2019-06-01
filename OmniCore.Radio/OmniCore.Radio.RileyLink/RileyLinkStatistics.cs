using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Radio.RileyLink
{
    public class RileyLinkStatistics : IMessageExchangeStatistics
    {
        public int QueueWaitDuration { get; set; }

        public int ExchangeDuration { get; set; }

        public int PacketExchangeCount { get; set; }

        public int PacketExchangeDurationAverage { get; set; }

        public int PodRssiAverage { get; set; }

        public int RadioRssiAverage { get; set; }

        private int started;
        public RileyLinkStatistics()
        {
            started = Environment.TickCount;
        }

        private int startedME;
        private int endedME;
        internal void StartMessageExchange()
        {
            startedME = Environment.TickCount;
        }

        internal void EndMessageExchange()
        {
            endedME = Environment.TickCount;
            ExchangeDuration = endedME - startedME;
        }

        internal void ExitPrematurely()
        {
            endedME = Environment.TickCount;
            ExchangeDuration = endedME - startedME;
        }


        internal void StartPacketExchange()
        {
        }

        internal void EndPacketExchange()
        {
        }

        internal void RepeatPacketReceived(RadioPacket p)
        {
        }

        internal void BadPacketReceived(RadioPacket p)
        {
        }

        internal void UnexpectedPacketReceived(RadioPacket p)
        {
        }

        internal void NoPacketReceived()
        {
        }

        internal void PacketReceived(RadioPacket p)
        {
        }

        internal void TimeoutOccured(Exception e)
        {
        }

        internal void RadioOverheadStart()
        {
        }

        internal void RadioErrorOccured(Exception e)
        {
        }

        internal void ProtocolErrorOccured(Exception e)
        {
        }

        internal void UnknownErrorOccured(Exception e)
        {
        }

        internal void BadDataReceived(Bytes received)
        {
        }

        internal void RadioScanStarted()
        {
        }

        internal void RadioScanFinished()
        {
        }

        internal void RadioRssiReported(int rssi)
        {
        }

        internal void RadioOverheadEnd()
        {
        }

        internal void RadioTxLevelChange(TxPower txPower)
        {
        }

        internal void RadioConnnected()
        {
        }

        internal void RadioDisconnected()
        {
        }
    }
}
