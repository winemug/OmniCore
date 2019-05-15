using OmniCore.Py;
using System.Threading.Tasks;

namespace Omni.Py
{
    public interface IPacketRadio
    {
        Task Reset();
        void TxLevelUp();
        void TxLevelDown();
        void SetTxLevel(TxPower tx_power);
        Task<Bytes> GetPacket(uint timeout = 5000);
        Task<Bytes> SendAndGetPacket(Bytes packet, byte repeat_count, ushort delay_ms, uint timeout_ms, byte retry_count, ushort preamble_ext_ms);
        Task SendPacket(Bytes packet, byte repeat_count, ushort delay_ms, ushort preamble_ext_ms);
    }
}
