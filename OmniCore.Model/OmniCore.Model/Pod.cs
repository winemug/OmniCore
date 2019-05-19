using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    public class Pod
    {
        public uint? id_lot = null;
        public uint? id_t = null;
        public string id_version_pm = null;
        public string id_version_pi = null;
        public byte? id_version_unknown_byte = null;
        public byte[] id_version_unknown_7_bytes = null;
        public uint radio_address;


        private int _packet_seq = 0;
        private int _message_seq = 0;

        public int radio_packet_sequence {
            get => _packet_seq;
            set
            {
                _packet_seq = value % 32;
            }
        }

        public int radio_message_sequence
        {
            get => _message_seq;
            set
            {
                _message_seq = value % 16;
            }
        }

        public int? radio_low_gain = null;
        public int? radio_rssi = null;

        public uint? nonce_last = null;
        public uint nonce_seed = 0;
        public uint? nonce_syncword = null;
        public int nonce_ptr = 0;
        public int nonce_runs = 0;

        public DateTime? state_last_updated = null;
        public PodProgress state_progress = PodProgress.InitialState;
        public BasalState state_basal = BasalState.Suspended;
        public BolusState state_bolus = BolusState.Inactive;
        public byte state_alert = 0;
        public ushort? state_alert_w278 = null;
        public ushort[] state_alerts = null;
        public uint state_active_minutes = 0;
        public bool state_faulted = false;

        public decimal? var_alert_low_reservoir = null;
        public int? var_alert_replace_pod = null;

        public decimal[] var_basal_schedule = null;
        public int? fault_event = null;
        public int? fault_event_rel_time = null;
        public int? fault_table_access = null;
        public int? fault_insulin_state_table_corruption = null;
        public int? fault_internal_variables = null;
        public bool? fault_immediate_bolus_in_progress = null;
        public PodProgress? fault_progress_before = null;
        public PodProgress? fault_progress_before2 = null;
        public uint? fault_information_type2_last_word = null;

        public decimal insulin_reservoir = 0;
        public decimal insulin_delivered = 0;
        public decimal insulin_canceled = 0;

        public int? var_utc_offset = null;
        public DateTime? var_activation_date = null;
        public DateTime? var_insertion_date = null;

        public string last_command = null;

        public int? last_command_db_id = null;
        public DateTime? last_enacted_temp_basal_start = null;
        public TimeSpan? last_enacted_temp_basal_duration = null;
        public decimal? last_enacted_temp_basal_amount = null;
        public DateTime? last_enacted_bolus_start = null;
        public decimal? last_enacted_bolus_amount = null;

        private readonly ProtocolHandler ProtocolHandler;
        public Pod(IMessageExchangeProvider messageExchangeProvider)
        {
            ProtocolHandler = new ProtocolHandler(messageExchangeProvider);
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

        private async Task send_request(RequestMessage request, bool with_nonce = false)
        {
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
        }

        private async Task internal_update_status(byte update_type = 0)
        {
            await send_request(ProtocolHelper.request_status(update_type));
        }

        public async Task UpdateStatus(byte update_type = 0)
        {
            try
            {
                Debug.WriteLine($"Updating pod status, request type {update_type}");
                await this.internal_update_status(update_type);
            }
            catch (StatusUpdateRequiredException)
            {
                await this.internal_update_status();
                await this.UpdateStatus(update_type);
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        public async Task AcknowledgeAlerts(byte alert_mask)
        {
            try
            {
                Debug.WriteLine($"Acknowledging alerts, bitmask: {alert_mask}");
                await internal_update_status().ConfigureAwait(false);
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

                await send_request(ProtocolHelper.request_acknowledge_alerts(alert_mask));
            }
            catch (StatusUpdateRequiredException)
            {
                await this.internal_update_status();
                await this.AcknowledgeAlerts(alert_mask);
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        public async Task Bolus(decimal bolusAmount)
        {
            try
            {
                Debug.WriteLine($"Bolusing {bolusAmount}U");
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

                if (state_bolus != BolusState.Immediate)
                    throw new PdmException("Pod did not start bolusing");

                last_enacted_bolus_start = DateTime.UtcNow;
                last_enacted_bolus_amount = bolusAmount;
            }
            catch (StatusUpdateRequiredException)
            {
                await this.internal_update_status();
                await this.Bolus(bolusAmount);
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        public async Task CancelBolus()
        {
            try
            {
                await internal_update_status();
                _assert_status_running();

                if (state_bolus != BolusState.Immediate)
                    throw new PdmException("Immediate bolus is not running");

                await send_request(ProtocolHelper.request_cancel_bolus(), true);

                if (state_bolus == BolusState.Immediate)
                    throw new PdmException("Failed to cancel running bolus");

                last_enacted_bolus_amount = insulin_canceled;
            }
            catch (StatusUpdateRequiredException)
            {
                await this.internal_update_status();
                await this.CancelBolus();
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