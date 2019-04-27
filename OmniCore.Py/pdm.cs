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

        public Pdm(Pod pod)
        {
            this.pod = pod;
            this.nonce = null;
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
                                  this.pod.radio_packet_sequence);

            return this.radio;
        }
    }
}
