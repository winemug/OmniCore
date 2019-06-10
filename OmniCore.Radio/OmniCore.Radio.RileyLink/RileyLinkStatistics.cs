using OmniCore.Model;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OmniCore.Radio.RileyLink
{
    public struct RileyLinkPeStats
    {
        public int ExchangeDuration { get; set; }

        public int SendCount { get; set; }

        public int BadPackets { get; set; }

        public int RepeatPackets { get; set; }

        public int ReceiveTimeout { get; set; }

        public int RadioErrors { get; set; }

        public int AverageRssi { get => RssiTotal / RssiCount; }

        public int RssiTotal { get; set; }

        public int RssiCount { get; set; }

    }

    public class RileyLinkStatistics : MessageExchangeStatistics
    {
        private int started;
        public RileyLinkStatistics()
        {
            started = Environment.TickCount;
            AllPeStats = new List<RileyLinkPeStats>();
        }

        private int startedME;
        private int endedME;
        internal void StartMessageExchange()
        {
            startedME = Environment.TickCount;
            QueueWaitDuration = startedME - started;
        }

        internal void EndMessageExchange()
        {
            endedME = Environment.TickCount;
            ExchangeDuration = endedME - startedME;
            RadioRssiAverage = radioRssiTotal / radioRssiCount;
        }

        internal void ExitPrematurely()
        {
            endedME = Environment.TickCount;
            ExchangeDuration = endedME - startedME;
            RadioRssiAverage = radioRssiTotal / radioRssiCount;
        }

        private int radioOverheadStart;
        internal void RadioOverheadStart()
        {
            radioOverheadStart = Environment.TickCount;
        }

        internal void RadioOverheadEnd()
        {
            TotalRadioOverhead += Environment.TickCount - radioOverheadStart;
        }

        List<RileyLinkPeStats> AllPeStats;

        RileyLinkPeStats currentPeStats = new RileyLinkPeStats();
        private int peStart;
        private int peEnd;
        internal void StartPacketExchange()
        {
            currentPeStats = new RileyLinkPeStats();
            peStart = Environment.TickCount;
        }

        internal void EndPacketExchange()
        {
            peEnd = Environment.TickCount;
            currentPeStats.ExchangeDuration = peEnd - peStart;
            AllPeStats.Add(currentPeStats);
        }

        internal void PacketSent(RadioPacket packetToSend)
        {
            currentPeStats.SendCount++;
        }

        void GetRssi(RadioPacket p)
        {
            if (p.Rssi != 0)
            {
                currentPeStats.RssiCount++;
                currentPeStats.RssiTotal = (int)p.Rssi;
            }
        }

        internal void RepeatPacketReceived(RadioPacket p)
        {
            currentPeStats.RepeatPackets++;
            GetRssi(p);
        }

        internal void BadPacketReceived(RadioPacket p)
        {
            currentPeStats.BadPackets++;
        }

        internal void UnexpectedPacketReceived(RadioPacket p)
        {
        }

        internal void NoPacketReceived()
        {
            currentPeStats.ReceiveTimeout++;
        }

        internal void BadDataReceived(Bytes received)
        {
            currentPeStats.BadPackets++;
        }

        internal void PacketReceived(RadioPacket p)
        {
            GetRssi(p);
        }

        internal void TimeoutOccured(Exception e)
        {
            currentPeStats.RadioErrors++;
        }

        internal void RadioErrorOccured(Exception e)
        {
            currentPeStats.RadioErrors++;
        }

        internal void ProtocolErrorOccured(Exception e)
        {
        }

        internal void UnknownErrorOccured(Exception e)
        {
        }

        internal void RadioScanStarted()
        {
        }

        internal void RadioScanFinished()
        {
        }

        int radioRssiCount = 0;
        int radioRssiTotal = 0;
        internal void RadioRssiReported(int rssi)
        {
            if (rssi != 0)
            {
                radioRssiCount++;
                radioRssiTotal += rssi;
            }
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
