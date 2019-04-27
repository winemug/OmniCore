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
        //void connect(bool force_initialize = false);
        //void disconnect(bool ignore_errors = true);
        //void init_radio(bool force_init = false);
        void tx_up();
        void tx_down();
        void set_tx_power(TxPower tx_power);
        Task<byte[]> get_packet(int timeout = 5000);
		Task<byte[]> send_and_receive_packet(byte[] packet, int repeat_count, int delay_ms, int timeout_ms, int retry_count, int preamble_ext_ms);
		//void send_packet(byte[] packet, int repeat_count, int delay_ms, int preamble_extension_ms);
    }
}
