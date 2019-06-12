using OmniCore.Model;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using OmniCore.Model.Eros;
using OmniCore.Model.Eros.Data;

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

    public class RileyLinkStatistics : ErosMessageExchangeStatistics
    {
        private int started;
        private int startedME;
        private int endedME;

        List<RileyLinkPeStats> AllPeStats;
        RileyLinkPeStats currentPeStats = new RileyLinkPeStats();
        private int peStart;
        private int peEnd;

        int radioRssiCount = 0;
        int radioRssiTotal = 0;

        private TxPower currentTxPower = TxPower.A4_Normal;

        public RileyLinkStatistics()
        {
            started = Environment.TickCount;
            AllPeStats = new List<RileyLinkPeStats>();
        }

        public override void BeforeSave()
        {
            endedME = Environment.TickCount;
            QueueWaitDuration = startedME - started;
            ExchangeDuration = endedME - startedME;

            if (radioRssiCount > 0)
                RadioRssiAverage = radioRssiTotal / radioRssiCount;
            if (mobileRssiCount > 0)
                MobileDeviceRssiAverage = mobileRssiTotal / mobileRssiCount;

        }

        internal void StartMessageExchange()
        {
            startedME = Environment.TickCount;
        }

        internal void EndMessageExchange()
        {
        }

        internal void ExitPrematurely()
        {
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
                radioRssiCount++;
                radioRssiTotal += p.Rssi;
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
            GetRssi(p);
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

        int mobileRssiCount = 0;
        int mobileRssiTotal = 0;

        internal void MobileDeviceRssiReported(int rssi)
        {
            if (rssi != 0)
            {
                mobileRssiCount++;
                mobileRssiTotal += rssi;
            }
        }

        internal void RadioTxLevelChange(TxPower txPower)
        {
            currentTxPower = txPower;
            PowerAdjustmentCount++;
        }

        internal void RadioConnnected()
        {
        }

        internal void RadioDisconnected()
        {
        }

    }
}
