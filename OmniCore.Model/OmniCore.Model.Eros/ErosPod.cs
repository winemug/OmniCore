using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Eros
{
    public class ErosPod : Pod
    {
        IMessageExchange MessageExchange;
        MessageHandler MessageHandler;

        private readonly ErosMessageExchangeParameters StandardParameters;

        public ErosPod(IMessageExchangeProvider messageExchangeProvider)
        {
            MessageHandler = new MessageHandler(this, messageExchangeProvider);
            StandardParameters = new ErosMessageExchangeParameters() { };
        }

        //private static Pod Load(uint lot, uint tid)
        //{
        //    //var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"pod_{lot}_{tid}.json");
        //    //if (string.IsNullOrEmpty(path))
        //    //    throw new ArgumentException();

        //    //if (!File.Exists(path)
        //    //{
        //    //    SavePod();
        //    //}
        //    //else
        //    //{
        //    //    Pod pod = null;
        //    //    var js = new JsonSerializer();
        //    //    using (var sr = new StreamReader(this.PodPath))
        //    //    {
        //    //        using (var jr = new JsonTextReader(sr))
        //    //        {
        //    //            pod = js.Deserialize<Pod>(jr);
        //    //        }
        //    //    }
        //    //    this.Pdm.Pod = pod;
        //    //}
        //    throw new NotImplementedException();
        //}

        public void Save()
        {
            //var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"pod_{lot}_{tid}.json");
            //if (string.IsNullOrEmpty(this.PodPath))
            //    throw new ArgumentException();

            //var js = new JsonSerializer();
            //using (var sw = new StreamWriter(this.PodPath, false, Encoding.UTF8))
            //{
            //    using (var jw = new JsonTextWriter(sw))
            //    {
            //        js.Serialize(jw, this.Pdm.Pod);
            //    }
            //}
        }

        //private async Task send_request(IRequest request, bool with_nonce = false)
        //{
            //if (with_nonce)
            //{
            //    var nonce_val = this.Nonce.GetNext();
            //    request.set_nonce(nonce_val);
            //    nonce_syncword = null;
            //}

            //var me = new MessageExchange(request, this.packetRadio, this.Pod);

            //var response = await me.GetPodResponse();
            //ProtocolHelper.response_parse(response, this.Pod);

            //if (with_nonce && nonce_syncword != null)
            //{
            //    Debug.WriteLine("Nonce resync requested");
            //    this.Nonce.Sync(request.sequence.Value);
            //    var nonce_val = this.Nonce.GetNext();
            //    request.set_nonce(nonce_val);
            //    nonce_syncword = null;
            //    radio_message_sequence = request.sequence.Value;
            //    response = await me.GetPodResponse();
            //    ProtocolHelper.response_parse(response, this.Pod);
            //    if (nonce_syncword != null)
            //    {
            //        this.Nonce.Reset();
            //        throw new PdmException("Nonce sync failed");
            //    }
            //}
        //}

        private async Task<PodCommandResult> internal_update_status(IMessageProgress progress, CancellationToken ct,
            StatusRequestType update_type = StatusRequestType.Standard)
        {
            var request = new ErosMessageBuilder().WithStatus(update_type).Build();
            return await MessageHandler.PerformExchange(request, StandardParameters, progress, ct);
        }

        public override async Task UpdateStatus(IMessageProgress progress, CancellationToken ct,
            StatusRequestType update_type = StatusRequestType.Standard)
        {
            try
            {
                Debug.WriteLine($"Updating pod status, request type {update_type}");
                await this.internal_update_status(progress, ct, update_type);
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        public override async Task AcknowledgeAlerts(IMessageProgress progress, CancellationToken ct, byte alert_mask)
        {
            try
            {
                Debug.WriteLine($"Acknowledging alerts, bitmask: {alert_mask}");
                await internal_update_status(progress, ct);
                _assert_immediate_bolus_not_active();
                if (state_progress < PodProgress.PairingSuccess)
                    throw new PdmException("Pod not paired completely yet.");

                if (state_progress == PodProgress.ErrorShuttingDown)
                    throw new PdmException("Pod is shutting down, cannot acknowledge alerts.");

                if (state_progress == PodProgress.AlertExpiredShuttingDown)
                    throw new PdmException("Acknowledgement period expired, pod is shutting down");

                if (state_progress > PodProgress.AlertExpiredShuttingDown)
                    throw new PdmException("Pod is not active");

                if ((state_alert & alert_mask) != alert_mask)
                    throw new PdmException("Bitmask is invalid for current alert state");

                // await send_request(ProtocolHelper.request_acknowledge_alerts(alert_mask));
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        public override async Task Bolus(IMessageProgress progress, CancellationToken ct, decimal bolusAmount)
        {
            try
            {
                Debug.WriteLine($"Bolusing {bolusAmount}U");
                await internal_update_status(progress, ct);
                _assert_status_running();
                _assert_immediate_bolus_not_active();

                if (bolusAmount < 0.05m)
                    throw new PdmException("Cannot bolus less than 0.05U");

                if (bolusAmount % 0.05m != 0)
                    throw new PdmException("Bolus must be multiples of 0.05U");

                if (bolusAmount > 30m)
                    throw new PdmException("Cannot bolus more than 30U");

                // await send_request(ProtocolHelper.request_bolus(bolusAmount), true);

                if (state_bolus != BolusState.Immediate)
                    throw new PdmException("Pod did not start bolusing");

                last_enacted_bolus_start = DateTime.UtcNow;
                last_enacted_bolus_amount = bolusAmount;
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        public override async Task CancelBolus(IMessageProgress progress, CancellationToken ct)
        {
            try
            {
                await internal_update_status(progress, ct);
                _assert_status_running();

                if (state_bolus != BolusState.Immediate)
                    throw new PdmException("Immediate bolus is not running");

                // await send_request(ProtocolHelper.request_cancel_bolus(), true);

                if (state_bolus == BolusState.Immediate)
                    throw new PdmException("Failed to cancel running bolus");

                last_enacted_bolus_amount = insulin_canceled;
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        private void _assert_immediate_bolus_not_active()
        {
            if (state_bolus == BolusState.Immediate)
                throw new PdmException("Bolus operation in progress");
        }

        private void _assert_status_running()
        {
            if (state_progress < PodProgress.Running)
                throw new PdmException("Pod is not yet running");

            if (state_progress > PodProgress.RunningLow)
                throw new PdmException("Pod is not running");
        }

    }
}