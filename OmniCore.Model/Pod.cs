using OmniCore.Model.Enums;
using System;
using System.IO;

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
    }
}