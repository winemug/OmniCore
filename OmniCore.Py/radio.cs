using Omni.Py;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace OmniCore.Py
{

    public class MessageExchange
    {
        public int unique_packets = 0;
        public int repeated_sends = 0;
        public int receive_timeouts = 0;
        public int repeated_receives = 0;
        public int protocol_errors = 0;
        public int bad_packets = 0;
        public int radio_errors = 0;
        public bool successful = false;
        public DateTime started;
        public DateTime ended;
    }

    public class Radio
    {
        public uint radio_address;
        public int message_sequence;
        public int packet_sequence;
        public RadioPacket last_received_packet;
        public PacketRadio packet_radio;
        public int last_packet_timestamp = 0;

        private AutoResetEvent request_arrived = new AutoResetEvent(false);
        private AutoResetEvent response_received = new AutoResetEvent(false);
        private AutoResetEvent request_shutdown = new AutoResetEvent(false);

        private PdmMessage pdm_message = null;
        private bool double_take = false;
        private TxPower? tx_power = null;
        private bool expect_critical_follow_up = false;
        private PodMessage pod_message = null;
        private Exception response_exception = null;
        private Thread radio_thread = null;

        private uint pdm_message_address;
        private uint? ack_address_override = null;

        private MessageExchange current_exchange = null;

        private logger logger = definitions.getLogger();
        private logger packet_logger = definitions.get_packet_logger();

        public Radio(uint radio_address, int msg_sequence, int pkt_sequence)
        {
            start();
        }

        public void start()
        {
            lock (this)
            {
                if (this.radio_thread == null)
                {
                    this.radio_thread = new Thread(radio_loop);
                    this.radio_init();
                    this.radio_thread.Start();
                }
            }
        }

        public void stop()
        {
            lock (this)
            {
                this.request_shutdown.Set();
                this.request_arrived.Set();
                this.radio_thread.Join();
                this.radio_thread = null;
            }
        }

        public void disconnect()
        {
            lock (this)
            {
                this._disconnect();
            }
        }

        private void _disconnect()
        {
            try
            {
                this.packet_radio.disconnect(true);
            }
            catch (Exception e)
            {
                this.logger.exception("Error while disconnecting", e);
            }
        }

        public bool radio_init(int retries = 1)
        {
            int retry = 0;
            while (retry < retries)
            {
                try
                {
                    this.packet_radio.disconnect();
                    this.packet_radio.connect(true);
                    return true;
                }
                catch (Exception e)
                {
                    this.logger.exception("Error during radio initialization", e);
                    Thread.Sleep(2000);
                    retry += 1;
                }
            }
            return false;
        }

        public PodMessage SendAndGet(PdmMessage message, uint? message_address = null, uint? ack_address_override = null,
                                    TxPower? tx_power = null, bool double_take = false, bool expect_critical_follow_up = false)
        {
            lock (this)
            {
                if (this.radio_thread == null)
                    throw new PacketRadioError("Radio is stopped");

                this.pdm_message = message;
                if (message_address == null)
                    this.pdm_message_address = this.radio_address;
                else
                    this.pdm_message_address = message_address.Value;

                this.ack_address_override = ack_address_override;
                this.pod_message = null;
                this.double_take = double_take;
                this.tx_power = tx_power;
                this.expect_critical_follow_up = expect_critical_follow_up;

                this.request_arrived.Set();

                this.response_received.WaitOne();

                if (this.pod_message == null)
                {
                    this.current_exchange.successful = false;
                    throw this.response_exception;
                }

                this.current_exchange.successful = true;
                return this.pod_message;
            }
        }

        private RadioPacket _ack_data(uint address1, uint address2, int sequence)
        {
            return new RadioPacket(address1, RadioPacketType.ACK, sequence, address2.ToBytes());
        }

        private RadioPacket _interim_ack(uint? ack_address_override, int sequence)
        {
            if (ack_address_override == null)
                return _ack_data(this.radio_address, this.radio_address, sequence);
            else
                return _ack_data(this.radio_address, ack_address_override.Value, sequence);
        }

        private RadioPacket _final_ack(uint? ack_address_override, int sequence)
        {
            if (ack_address_override == null)
                return _ack_data(this.radio_address, 0, sequence);
            else
                return _ack_data(this.radio_address, ack_address_override.Value, sequence);
        }

        private void radio_loop()
        {
            while (true)
            {
                if (!this.request_arrived.WaitOne(5000))
                    this._disconnect();
                this.request_arrived.WaitOne();

                if (this.request_shutdown.WaitOne(0))
                {
                    this._disconnect();
                    break;
                }

                this.current_exchange = new MessageExchange();
                this.current_exchange.started = DateTime.UtcNow;

                try
                {
                    this.pod_message = this._send_and_get(this.pdm_message, this.pdm_message_address,
                        this.ack_address_override, this.tx_power, this.double_take, this.expect_critical_follow_up);
                    this.response_exception = null;
                }
                catch (Exception e)
                {
                    this.pod_message = null;
                    this.response_exception = e;
                }

                if (this.response_exception == null)
                {
                    var ack_packet = this._final_ack(this.ack_address_override, this.packet_sequence);
                    this.current_exchange.ended = DateTime.UtcNow;
                    this.response_received.Set();
                    try
                    {
                        this._send_packet(ack_packet, 3500);
                    }
                    catch (Exception e)
                    {
                        this.logger.exception("Error during ending conversation, ignored.", e);
                    }
                }
                else
                {
                    this.current_exchange.ended = DateTime.UtcNow;
                    this.response_received.Set();
                }
            }
        }

        private void reset_sequences()
        {
            this.packet_sequence = 0;
            this.message_sequence = 0;
        }

        private PodMessage _send_and_get(PdmMessage pdm_message, uint pdm_message_address, uint? ack_address_override = null,
                  TxPower? tx_power = null, bool double_take = false, bool expect_critical_follow_up = false)
        {
            var packets = pdm_message.get_radio_packets(pdm_message_address,
                                        this.message_sequence,
                                        this.radio_address,
                                        this.packet_sequence,
                                        double_take,
                                        expect_critical_follow_up);

            try
            {
                if (tx_power != null)
                    this.packet_radio.set_tx_power(tx_power.Value);
            }
            catch (PacketRadioError pre)
            {
                if (!this.radio_init(3))
                    throw pre;
            }

            RadioPacket received = null;
            var packet_count = packets.Count;

            this.current_exchange.unique_packets = packet_count * 2;

            for (int part = 0; part < packet_count; part++)
            {
                var packet = packets[part];
                int repeat_count = -1;
                int timeout = 10000;
                while (true)
                {
                    repeat_count++;
                    if (repeat_count == 0)
                        this.logger.log($"Sending PDM message part {part + 1}/{packet_count}");
                    else
                        this.logger.log($"Sending PDM message part {part + 1}/{packet_count} (Repeat: {repeat_count})");

                    RadioPacketType expected_type;
                    if (part == packet_count - 1)
                        expected_type = RadioPacketType.POD;
                    else
                        expected_type = RadioPacketType.ACK;

                    try
                    {
                        received = this._exchange_packets(packet.with_sequence(this.packet_sequence), expected_type, timeout);
                        break;
                    }
                    catch (OmnipyTimeoutError)
                    {
                        this.logger.log("Trying to recover from timeout error");
                        if (part == 0)
                        {
                            if (repeat_count == 0)
                            {
                                timeout = 15000;
                                continue;
                            }
                            else if (repeat_count == 1)
                            {
                                this.reset_sequences();
                                timeout = 10000;
                                Thread.Sleep(2000);
                                continue;
                            }
                            else if (repeat_count == 2)
                            {
                                this.reset_sequences();
                                this.radio_init();
                                timeout = 15000;
                                continue;
                            }
                            else
                            {
                                this.logger.log("Failed recovery");
                                if (packet_count == 1)
                                {
                                    this.logger.log("Calming pod down in case it's still broadcasting");
                                    var ack_packet = this._final_ack(this.ack_address_override, 2);
                                    try
                                    {
                                        this.packet_radio.set_tx_power(TxPower.Highest);
                                        this._send_packet(ack_packet);
                                    }
                                    catch(Exception e)
                                    {
                                        this.logger.exception("Ignored.", e);
                                    }
                                }
                                this.reset_sequences();
                                throw;
                            }
                        }
                        else if (part < packet_count - 1)
                        {
                            if (repeat_count < 2)
                            {
                                timeout = 20000;
                                continue;
                            }
                            else
                                throw;
                        }
                        else
                        {
                            if (repeat_count < 10)
                            {
                                timeout = 20000;
                                continue;
                            }
                            else
                                throw;
                        }
                    }
                    catch(PacketRadioError)
                    {
                        this.logger.log("Trying to recover from radio error");
                        this.current_exchange.radio_errors++;
                        if (part == 0)
                        {
                            if (repeat_count < 2)
                            {
                                this.radio_init();
                                continue;
                            }
                            else if (repeat_count < 4)
                            {
                                this._disconnect();
                                timeout = 10000;
                                Thread.Sleep(2000);
                                continue;
                            }
                            else
                            {
                                this.logger.log("Failed recovery");
                                this.reset_sequences();
                                throw;
                            }
                        }
                        else if (part < packet_count - 1)
                        {
                            if (repeat_count < 6)
                            {
                                this._disconnect();
                                timeout = 10000;
                                Thread.Sleep(2000);
                                continue;
                            }
                            else
                            {
                                this.logger.log("Failed recovery");
                                this.reset_sequences();
                                throw;
                            }
                        }
                        else
                        {
                            if (repeat_count < 10)
                            {
                                this._disconnect();
                                timeout = 10000;
                                Thread.Sleep(2000);
                                continue;
                            }
                            else
                            {
                                this.logger.log("Failed recovery");
                                this.reset_sequences();
                                throw;
                            }
                        }
                    }
                    catch(ProtocolError pe)
                    {
                        if (pe.ReceivedPacket != null && expected_type == RadioPacketType.POD && pe.ReceivedPacket.type == RadioPacketType.ACK)
                        {
                            this.logger.log("Trying to recover from protocol error");
                            this.packet_sequence = (pe.ReceivedPacket.sequence + 1) % 32;
                            packet = this._interim_ack(this.ack_address_override, this.packet_sequence);
                            continue;
                        }
                        else
                            throw;
                    }
                }
                part++;
                this.packet_sequence = (received.sequence + 1) % 32;
            }

            this.packet_logger.log($"SENT MSG {pdm_message}");

            var part_count = 0;
            if (received.type == RadioPacketType.POD)
            {
                part_count = 1;
                this.logger.log($"Received POD message part {part_count}");
            }
            var pod_response = new PodMessage();
            while (!pod_response.add_radio_packet(received))
            {
                var ack_packet = this._interim_ack(ack_address_override, (received.sequence + 1) % 32);
                received = this._exchange_packets(ack_packet, RadioPacketType.CON);
                part_count++;
                this.logger.log($"Received POD message part {part_count}");
            }

            this.packet_logger.log($"RCVD MSG {pod_response}");
            this.logger.log("Send and receive completed.");
            this.message_sequence = (pod_response.sequence.Value + 1) % 16;
            this.packet_sequence = (received.sequence + 1) % 32;

            return pod_response;
        }

        private RadioPacket _exchange_packets(RadioPacket packet_to_send, RadioPacketType expected_type, int timeout = 10000)
        {
            int start_time = 0;
            bool first = true;
            byte[] received = null;
            while (start_time == 0 || Environment.TickCount - start_time < timeout)
            {
                if (first)
                    first = false;
                else
                    this.current_exchange.repeated_sends += 1;

                if (this.last_packet_timestamp == 0 || (Environment.TickCount - this.last_packet_timestamp) > 4000)
                    received = this.packet_radio.send_and_receive_packet(packet_to_send.get_data(), 0, 0, 300, 1, 300);
                else
                    received = this.packet_radio.send_and_receive_packet(packet_to_send.get_data(), 0, 0, 120, 0, 40);
                if (start_time == 0)
                    start_time = Environment.TickCount;

                this.packet_logger.log($"SEND PKT {packet_to_send}");

                if (received == null)
                {
                    this.current_exchange.receive_timeouts++;
                    this.packet_logger.log("RECV PKT None");
                    this.packet_radio.tx_up();
                    continue;
                }

                var p = this._get_packet(received);
                if (p == null)
                {
                    this.current_exchange.bad_packets++;
                    this.packet_radio.tx_down();
                    continue;
                }

                this.packet_logger.log($"RECV PKT {p}");
                if (p.address != this.radio_address)
                {
                    this.current_exchange.bad_packets++;
                    this.packet_logger.log("RECV PKT ADDR MISMATCH");
                    this.packet_radio.tx_down();
                    continue;
                }

                this.last_packet_timestamp = Environment.TickCount;

                if (this.last_received_packet != null && p.sequence == this.last_received_packet.sequence
                    && p.type == this.last_received_packet.type)
                {
                    this.current_exchange.repeated_receives++;
                    this.packet_logger.log("RECV PKT previous");
                    this.packet_radio.tx_up();
                    continue;
                }

                this.last_received_packet = p;
                this.packet_sequence = (p.sequence + 1) % 32;

                if (p.type != expected_type)
                {
                    this.packet_logger.log("RECV PKT unexpected type");
                    this.current_exchange.protocol_errors++;
                    throw new ProtocolError("Unexpected packet type received");
                }

                if (p.sequence != (packet_to_send.sequence+1) %32)
                {
                    this.packet_sequence = (p.sequence + 1) % 32;
                    this.packet_logger.log("RECV PKT unexpected sequence");
                    this.last_received_packet = p;
                    this.current_exchange.protocol_errors++;
                    throw new ProtocolError("Incorrect packet sequence received");
                }

                return p;

            }
            throw new OmnipyTimeoutError("Exceeded timeout while send and receive");
        }

        private void _send_packet(RadioPacket packet_to_send, int allow_premature_exit_after = -1, int timeout=25000)
        {
            int start_time = 0;
            this.current_exchange.unique_packets++;
            byte[] received = null;
            while (start_time == 0 || Environment.TickCount - start_time < timeout)
            {
                try
                {
                    this.packet_logger.log($"SEND PKT {packet_to_send}");

                    received = this.packet_radio.send_and_receive_packet(packet_to_send.get_data(), 0, 0, 300, 0, 40);

                    if (start_time == 0)
                        start_time = Environment.TickCount;

                    if (allow_premature_exit_after >= 0 && Environment.TickCount - start_time >= allow_premature_exit_after)
                    {
                        if (this.request_arrived.WaitOne(0))
                        {
                            this.logger.log("Prematurely exiting final phase to process next request");
                            this.packet_sequence = (this.packet_sequence + 1) % 32;
                            break;
                        }
                    }

                    if (received == null)
                    {
                        received = this.packet_radio.get_packet(600);
                        if (received == null)
                        {
                            this.packet_logger.log("Silence fell.");
                            this.packet_sequence = (this.packet_sequence + 1) % 32;
                            break;
                        }
                    }

                    var p = this._get_packet(received);
                    if (p == null)
                    {
                        this.current_exchange.bad_packets++;
                        this.packet_radio.tx_down();
                        continue;
                    }

                    if (p.address != this.radio_address)
                    {
                        this.current_exchange.bad_packets++;
                        this.packet_logger.log("RECV PKT ADDR MISMATCH");
                        this.packet_radio.tx_down();
                        continue;
                    }

                    this.last_packet_timestamp = Environment.TickCount;
                    if (this.last_received_packet != null && p.type == this.last_received_packet.type
                        && p.sequence == this.last_received_packet.sequence)
                    {
                        this.current_exchange.repeated_receives++;
                        this.packet_logger.log("RECV PKT previous");
                        this.packet_radio.tx_up();
                        continue;
                    }

                    this.packet_logger.log($"RECV PKT {p}");
                    this.packet_logger.log($"RECEIVED unexpected packet");
                    this.current_exchange.protocol_errors++;
                    this.last_received_packet = p;
                    this.packet_sequence = (p.sequence + 1) % 32;
                    packet_to_send.with_sequence(this.packet_sequence);
                    start_time = Environment.TickCount;
                    continue;
                }
                catch(PacketRadioError pre)
                {
                    this.current_exchange.radio_errors++;
                    this.logger.exception("Radio error during send, retrying", pre);
                    if (!this.radio_init(3))
                        throw pre;
                    start_time = Environment.TickCount;
                }
            }
            this.logger.log("Exceeded timeout while waiting for silence to fall");
        }

        private RadioPacket _get_packet(byte[] data)
        {
            if (data != null && data.Length > 2)
            {
                byte rssi = data[0];
                try
                {
                    var rp = RadioPacket.parse(data.Sub(2));
                    rp.rssi = rssi;
                    return rp;
                }
                catch (Exception e)
                {
                    this.packet_logger.log($"RECV INVALID DATA {data.Sub(2)}");
                }
            }
            return null;
        }
    }
}