using Omni.Py;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Py
{
    public class Pdm
    {

        private IPacketRadio _packetRadio;

        private Pod _pod;
        public Pod Pod
        {
            get => this._pod;
            set
            {
                if (this._pod != value)
                {
                    this._pod = value;
                    if (value == null)
                    {
                        this.Radio = null;
                        this.Nonce = null;
                    }
                    else
                    {
                        if (this.Radio == null)
                            this.Radio = new Radio(_packetRadio, value);
                        else
                            this.Radio.Pod = value;

                        this.Nonce = new Nonce(value);
                    }
                }
            }
        }

        private Radio Radio { get; set; }

        private Nonce Nonce { get; set; }

        private logger logger;

        public Pdm(IPacketRadio packetRadio)
        {
            this._packetRadio = packetRadio;
            this.logger = definitions.getLogger();
        }

        private async Task send_request(PdmMessage request, bool with_nonce = false)
        {
            if (with_nonce)
            {
                var nonce_val = this.Nonce.GetNext();
                request.set_nonce(nonce_val);
                this.Pod.nonce_syncword = null;
            }

            var response = await this.Radio.SendAndGet(request);
            protocol.response_parse(response, this.Pod);

            if (with_nonce && this.Pod.nonce_syncword != null)
            {
                this.logger.log("Nonce resync requested");
                this.Nonce.Sync(request.sequence.Value);
                var nonce_val = this.Nonce.GetNext();
                request.set_nonce(nonce_val);
                this.Pod.nonce_syncword = null;
                this.Pod.radio_message_sequence = request.sequence.Value;
                response = await this.Radio.SendAndGet(request);
                protocol.response_parse(response, this.Pod);
                if (this.Pod.nonce_syncword != null)
                {
                    this.Nonce.Reset();
                    throw new PdmError("Nonce sync failed");
                }
            }
        }

        private async Task internal_update_status(int update_type = 0)
        {
            _assert_pod();
            await send_request(protocol.request_status(update_type));
        }

        public async Task UpdateStatus(int update_type = 0)
        {
            try
            {
                this.logger.log($"Updating pod status, request type {update_type}");
                await this.internal_update_status(update_type);
            }
            catch(OmnipyError)
            {
                throw;
            }
            catch(Exception e)
            {
                throw new PdmError("Unexpected error", e);
            }
        }

        private void _assert_pod()
        {
            if (this.Pod == null)
                throw new PdmError("No pod registered");
        }
    }
}
