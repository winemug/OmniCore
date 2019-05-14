using Omni.Py;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Py
{
    public class Pdm
    {

        private IPacketRadio packetRadio;

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
                        this.Nonce = null;
                    }
                    else
                    {
                        this.Nonce = new Nonce(value);
                    }
                }
            }
        }

        private Nonce Nonce { get; set; }

        private PyLogger logger = new PyLogger();

        public Pdm(IPacketRadio packetRadio)
        {
            this.packetRadio = packetRadio;
            this.packetRadio.reset();
        }

        private async Task send_request(PdmMessage request, bool with_nonce = false)
        {
            if (with_nonce)
            {
                var nonce_val = this.Nonce.GetNext();
                request.set_nonce(nonce_val);
                this.Pod.nonce_syncword = null;
            }

            var me = new MessageExchange(request, this.packetRadio, this.Pod);

            var response = await me.GetPodResponse();
            ProtocolHelper.response_parse(response, this.Pod);

            if (with_nonce && this.Pod.nonce_syncword != null)
            {
                this.logger.Log("Nonce resync requested");
                this.Nonce.Sync(request.sequence.Value);
                var nonce_val = this.Nonce.GetNext();
                request.set_nonce(nonce_val);
                this.Pod.nonce_syncword = null;
                this.Pod.radio_message_sequence = request.sequence.Value;
                response = await me.GetPodResponse();
                ProtocolHelper.response_parse(response, this.Pod);
                if (this.Pod.nonce_syncword != null)
                {
                    this.Nonce.Reset();
                    throw new PdmException("Nonce sync failed");
                }
            }
        }

        private async Task internal_update_status(byte update_type = 0)
        {
            _assert_pod();
            await send_request(ProtocolHelper.request_status(update_type));
        }

        public async Task UpdateStatus(byte update_type = 0)
        {
            try
            {
                this.logger.Log($"Updating pod status, request type {update_type}");
                await this.internal_update_status(update_type);
            }
            catch(OmnipyException)
            {
                throw;
            }
            catch(Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        private void _assert_pod()
        {
            if (this.Pod == null)
                throw new PdmException("No pod registered");
        }
    }
}
