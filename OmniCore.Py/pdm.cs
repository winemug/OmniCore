using Omni.Py;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Py
{
    public class Pdm
    {
        private Pod pod;
        private Nonce nonce;
        private Radio radio;
        private logger logger;
        private PacketRadio packetRadio;

        public Pdm(Pod pod, PacketRadio packetRadio)
        {
            this.packetRadio = packetRadio;
            this.pod = pod;
            this.nonce = null;
            this.logger = definitions.getLogger();
        }

        public void stop_radio()
        {
            if (this.radio != null)
                this.radio.stop();
            this.radio = null;
        }

        public Radio start_radio()
        {
            return this.get_radio(true);
        }

        private Nonce get_nonce()
        {
            if (this.nonce == null)
            {
                if (this.pod.id_lot == null || this.pod.id_t == null)
                    return null;
                if (this.pod.nonce_last == null)
                    this.nonce = new Nonce(this.pod.id_lot.Value, this.pod.id_t.Value);
                else
                    this.nonce = new Nonce(this.pod.id_lot.Value,
                        this.pod.id_t.Value, this.pod.nonce_last.Value, this.pod.nonce_seed);
            }
            return this.nonce;
        }

        private Radio get_radio(bool getNew = false)
        {
            if (this.radio != null && getNew)
            {
                this.radio.stop();
                this.radio = null;
            }

            this.radio = new Radio(this.pod.radio_address.Value,
                                  this.pod.radio_message_sequence,
                                  this.pod.radio_packet_sequence,
                                  this.packetRadio);

            return this.radio;
        }

        private void send_request(PdmMessage request, bool with_nonce = false, bool double_take = false,
                    bool expect_critical_follow_up = false,
                    TxPower tx_power = TxPower.Normal)
        {
            var nonce_obj = this.get_nonce();
            if (with_nonce)
            {
                var nonce_val = nonce_obj.getNext();
                request.set_nonce(nonce_val);
                this.pod.nonce_syncword = null;
            }

            var response = this.get_radio().SendAndGet(request, null, null, tx_power, double_take, expect_critical_follow_up);
            protocol.response_parse(response, this.pod);

            if (with_nonce && this.pod.nonce_syncword != null)
            {
                this.logger.log("Nonce resync requested");
                nonce_obj.sync(this.pod.nonce_syncword.Value, request.sequence.Value);
                var nonce_val = nonce_obj.getNext();
                request.set_nonce(nonce_val);
                this.pod.nonce_syncword = null;
                this.get_radio().message_sequence = request.sequence.Value;
                response = this.get_radio().SendAndGet(request, null, null, tx_power, double_take, expect_critical_follow_up);
                protocol.response_parse(response, this.pod);
                if (this.pod.nonce_syncword != null)
                {
                    this.get_nonce().reset();
                    throw new PdmError("Nonce sync failed");
                }
            }
        }

        private void internal_update_status(int update_type = 0)
        {
            _assert_pod_address_assigned();
            send_request(protocol.request_status(update_type));
        }

        public void UpdateStatus(int update_type = 0)
        {
            try
            {
                this.logger.log($"Updating pod status, request type {update_type}");
                this.internal_update_status(update_type);
            }
            catch(OmnipyError)
            {
                throw;
            }
            catch(Exception e)
            {
                throw new PdmError("Unexpected error", e);
            }
            finally
            {
                SavePod();
            }
        }

        private void SavePod()
        {
            try
            {
                radio = this.get_radio();
                if (radio != null)
                {
                    this.pod.radio_message_sequence = radio.message_sequence;
                    this.pod.radio_packet_sequence = radio.packet_sequence;
                }

                nonce = this.get_nonce();
                if (nonce != null)
                {
                    this.pod.nonce_last = nonce.lastNonce;
                    this.pod.nonce_seed = nonce.seed;
                }

                this.pod.Save();
            }
            catch(Exception e)
            {
                throw new PdmError("Pod status was not saved", e);
            }
        }

        private void _assert_pod_address_assigned()
        {
            if (this.pod.radio_address == null)
                throw new PdmError("Pod address not set");
        }
    }
}
