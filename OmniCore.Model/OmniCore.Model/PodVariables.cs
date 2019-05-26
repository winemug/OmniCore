using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    public abstract partial class Pod : IPodVariables
    {
        public uint? id_lot { get; set; }
        public uint? id_t { get; set; }
        public string id_version_pm { get; set; }
        public string id_version_pi { get; set; }
        public byte? id_version_unknown_byte { get; set; }
        public byte[] id_version_unknown_7_bytes { get; set; }

        public uint radio_address { get; set; }

        private int _packet_seq = 0;
        private int _message_seq = 0;

        public int radio_packet_sequence
        {
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

        public int? radio_low_gain { get; set; }
        public int? radio_rssi { get; set; }

        public uint? nonce_last { get; set; }
        public uint nonce_seed { get; set; }
        public uint? nonce_syncword { get; set; }
        public int nonce_ptr { get; set; }
        public int nonce_runs { get; set; }

        public DateTime? state_last_updated { get; set; }
        public PodProgress state_progress { get; set; }
        public BasalState state_basal { get; set; }
        public BolusState state_bolus { get; set; }
        public byte state_alert { get; set; }
        public ushort? state_alert_w278 { get; set; }
        public ushort[] state_alerts { get; set; }
        public uint state_active_minutes { get; set; }
        public bool state_faulted { get; set; }

        public decimal? var_alert_low_reservoir { get; set; }
        public int? var_alert_replace_pod { get; set; }

        public decimal[] var_basal_schedule { get; set; }
        public int? fault_event { get; set; }
        public int? fault_event_rel_time { get; set; }
        public int? fault_table_access { get; set; }
        public int? fault_insulin_state_table_corruption { get; set; }
        public int? fault_internal_variables { get; set; }
        public bool? fault_immediate_bolus_in_progress { get; set; }
        public PodProgress? fault_progress_before { get; set; }
        public PodProgress? fault_progress_before2 { get; set; }
        public uint? fault_information_type2_last_word { get; set; }

        public decimal insulin_reservoir { get; set; }
        public decimal insulin_delivered { get; set; }
        public decimal insulin_canceled { get; set; }

        public int? var_utc_offset { get; set; }
        public DateTime? var_activation_date { get; set; }
        public DateTime? var_insertion_date { get; set; }

        public string last_command { get; set; }

        public int? last_command_db_id { get; set; }
        public DateTime? last_enacted_temp_basal_start { get; set; }
        public TimeSpan? last_enacted_temp_basal_duration { get; set; }
        public decimal? last_enacted_temp_basal_amount { get; set; }
        public DateTime? last_enacted_bolus_start { get; set; }
        public decimal? last_enacted_bolus_amount { get; set; }

    }
}