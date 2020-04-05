using OmniCore.Model.Utilities;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkStatisticsResponse : RileyLinkDefaultResponse
    {
        public uint UptimeMilliseconds { get; private set; }
        public ushort RxOverflowCount { get; private set; }
        public ushort RxFifoOverflowCount { get; private set; }
        public ushort PacketCountRx { get; private set; }
        public ushort PacketCountTx { get; private set; }
        public ushort CrcFailureCount { get; private set; }
        public ushort SpiSyncFailureCount { get; private set; }

        protected override void ParseInternal(byte[] responseData)
        {
            if (responseData.Length == 20)
            {
                var b = new Bytes(responseData);

                UptimeMilliseconds = b.DWord(0);
                RxOverflowCount = b.Word(4);
                RxFifoOverflowCount = b.Word(6);
                PacketCountRx = b.Word(8);
                PacketCountTx = b.Word(10);
                CrcFailureCount = b.Word(12);
                SpiSyncFailureCount = b.Word(14);
            }
        }
    }
}