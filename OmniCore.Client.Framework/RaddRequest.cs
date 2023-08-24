namespace OmniCore.Framework;

public class RaddRequest
{
    // public string? pod_id { get; set; }
    // public string? profile_id { get; set; }
    // public int? radio_address { get; set; }
    // public int? record_index { get; set; }
    // public int? units_per_ml { get; set; }
    // public int? medication { get; set; }
    // public int? valid_from { get; set; }
    // public int? valid_to { get; set; }
    public string? request_id { get; set; }
    public uint? transfer_active_serial { get; set; }
    public uint? transfer_active_lot { get; set; }
    public uint? transfer_active_address { get; set; }
    public string? pod_id { get; set; }
    public int? next_record_index { get; set; }
    public bool beep { get; set; }
    public bool update_status { get; set; }
    public int? bolus_ticks { get; set; }
    public int? temp_basal_ticks { get; set; }
    public int? temp_basal_half_hours { get; set; }
    public bool cancel_temp { get; set; }
    public bool cancel_bolus { get; set; }
    public bool deactivate { get; set; }
    public bool remove { get; set; }
    public bool test_bolus { get; set; }
}