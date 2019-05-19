using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodVariables
    {
        int? fault_event { get; set; }
        int? fault_event_rel_time { get; set; }
        bool? fault_immediate_bolus_in_progress { get; set; }
        uint? fault_information_type2_last_word { get; set; }
        int? fault_insulin_state_table_corruption { get; set; }
        int? fault_internal_variables { get; set; }
        PodProgress? fault_progress_before { get; set; }
        PodProgress? fault_progress_before2 { get; set; }
        int? fault_table_access { get; set; }
        uint? id_lot { get; set; }
        uint? id_t { get; set; }
        string id_version_pi { get; set; }
        string id_version_pm { get; set; }
        byte[] id_version_unknown_7_bytes { get; set; }
        byte? id_version_unknown_byte { get; set; }
        decimal insulin_canceled { get; set; }
        decimal insulin_delivered { get; set; }
        decimal insulin_reservoir { get; set; }
        string last_command { get; set; }
        int? last_command_db_id { get; set; }
        decimal? last_enacted_bolus_amount { get; set; }
        DateTime? last_enacted_bolus_start { get; set; }
        decimal? last_enacted_temp_basal_amount { get; set; }
        TimeSpan? last_enacted_temp_basal_duration { get; set; }
        DateTime? last_enacted_temp_basal_start { get; set; }
        uint? nonce_last { get; set; }
        int nonce_ptr { get; set; }
        int nonce_runs { get; set; }
        uint nonce_seed { get; set; }
        uint? nonce_syncword { get; set; }
        uint radio_address { get; set; }
        int? radio_low_gain { get; set; }
        int radio_message_sequence { get; set; }
        int radio_packet_sequence { get; set; }
        int? radio_rssi { get; set; }
        uint state_active_minutes { get; set; }
        byte state_alert { get; set; }
        ushort? state_alert_w278 { get; set; }
        ushort[] state_alerts { get; set; }
        BasalState state_basal { get; set; }
        BolusState state_bolus { get; set; }
        bool state_faulted { get; set; }
        DateTime? state_last_updated { get; set; }
        PodProgress state_progress { get; set; }
        DateTime? var_activation_date { get; set; }
        decimal? var_alert_low_reservoir { get; set; }
        int? var_alert_replace_pod { get; set; }
        decimal[] var_basal_schedule { get; set; }
        DateTime? var_insertion_date { get; set; }
        int? var_utc_offset { get; set; }
    }
}
