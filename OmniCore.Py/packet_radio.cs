using OmniCore.Py;
using System.Threading.Tasks;

namespace Omni.Py
{
    public enum TxPower
    {
        Lowest = 0,
		Low = 1,
		Normal = 2,
		High = 3,
		Highest = 4
    }

    public interface IPacketRadio
    {
        Task reset();
        void tx_up();
        void tx_down();
        void set_tx_power(TxPower tx_power);
        Task<Bytes> get_packet(uint timeout = 5000);
        Task<Bytes> send_and_receive_packet(Bytes packet, byte repeat_count, ushort delay_ms, uint timeout_ms, byte retry_count, ushort preamble_ext_ms);
        Task send_packet(Bytes packet, byte repeat_count, ushort delay_ms, ushort preamble_ext_ms);
    }
}
