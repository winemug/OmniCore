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

    public interface PacketRadio
    {
        void connect(bool force_initialize = false);
        void disconnect(bool ignore_errors = true);
        void get_info();
        void init_radio(bool force_init = false);
        void tx_up();
		void tx_down();
		void set_tx_power(TxPower tx_power);
		void get_packet(double timeout = 5.0);
		byte[] send_and_receive_packet(byte[] packet, int repeat_count, int delay_ms, int timeout_ms, int retry_count, int preamble_ext_ms);
		void send_packet(byte[] packet, int repeat_count, int delay_ms, int preamble_extension_ms);
    }
}
