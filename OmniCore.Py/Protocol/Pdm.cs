using Omni.Py;
using OmniCore.Py.Exceptions;
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
            catch(StatusUpdateRequiredException)
            {
                await this.internal_update_status();
                await this.UpdateStatus(update_type);
            }
            catch(OmnipyException) { throw; }
            catch(Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        public async Task AcknowledgeAlerts(byte alert_mask)
        {
            try
            {
                this.logger.Log($"Acknowledging alerts, bitmask: {alert_mask}");
                _assert_pod();
                await internal_update_status().ConfigureAwait(false);
                _assert_immediate_bolus_not_active();
                if (this.Pod.state_progress < PodProgress.PairingSuccess)
                    throw new PdmException("Pod not paired completely yet.");

                if (this.Pod.state_progress == PodProgress.ErrorShuttingDown)
                    throw new PdmException("Pod is shutting down, cannot acknowledge alerts.");

                if (this.Pod.state_progress == PodProgress.AlertExpiredShuttingDown)
                    throw new PdmException("Acknowledgement period expired, pod is shutting down");

                if (this.Pod.state_progress > PodProgress.AlertExpiredShuttingDown)
                    throw new PdmException("Pod is not active");

                if ((this.Pod.state_alert & alert_mask) != alert_mask)
                    throw new PdmException("Bitmask is invalid for current alert state");

                await send_request(ProtocolHelper.request_acknowledge_alerts(alert_mask));
            }
            catch (StatusUpdateRequiredException)
            {
                await this.internal_update_status();
                await this.AcknowledgeAlerts(alert_mask);
            }
            catch (OmnipyException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        public async Task Bolus(decimal bolusAmount)
        {
            try
            {
                this.logger.Log($"Bolusing {bolusAmount}U");
                _assert_pod();
                await internal_update_status();
                _assert_status_running();
                _assert_immediate_bolus_not_active();

                if (bolusAmount < 0.05m)
                    throw new PdmException("Cannot bolus less than 0.05U");

                if (bolusAmount % 0.05m != 0)
                    throw new PdmException("Bolus must be multiples of 0.05U");

                if (bolusAmount > 30m)
                    throw new PdmException("Cannot bolus more than 30U");

                await send_request(ProtocolHelper.request_bolus(bolusAmount), true);

                if (this.Pod.state_bolus != BolusState.Immediate)
                    throw new PdmException("Pod did not start bolusing");

                this.Pod.last_enacted_bolus_start = DateTime.UtcNow;
                this.Pod.last_enacted_bolus_amount = bolusAmount;
            }
            catch (StatusUpdateRequiredException)
            {
                await this.internal_update_status();
                await this.Bolus(bolusAmount);
            }
            catch (OmnipyException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        public async Task CancelBolus()
        {
            try
            {
                _assert_pod();
                await internal_update_status();
                _assert_status_running();

                if (this.Pod.state_bolus != BolusState.Immediate)
                    throw new PdmException("Immediate bolus is not running");

                await send_request(ProtocolHelper.request_cancel_bolus(), true);

                if (this.Pod.state_bolus == BolusState.Immediate)
                    throw new PdmException("Failed to cancel running bolus");

                this.Pod.last_enacted_bolus_amount = this.Pod.insulin_canceled;
            }
            catch (StatusUpdateRequiredException)
            {
                await this.internal_update_status();
                await this.CancelBolus();
            }
            catch (OmnipyException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        private void _assert_pod()
        {
            if (this.Pod == null)
                throw new PdmException("No pod registered");
        }

        private void _assert_immediate_bolus_not_active()
        {
            if (this.Pod.state_bolus == BolusState.Immediate)
                throw new PdmException("Bolus operation in progress");
        }

        private void _assert_status_running()
        {
            if (this.Pod.state_progress < PodProgress.Running)
                throw new PdmException("Pod is not yet running");

            if (this.Pod.state_progress > PodProgress.RunningLow)
                throw new PdmException("Pod is not running");
        }
    }
}
