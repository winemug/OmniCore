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
    public string? RequestId { get; set; }
    public uint? TransferActiveSerial { get; set; }
    public uint? TransferActiveLot { get; set; }
    public uint? TransferActiveAddress { get; set; }
    public string? PodId { get; set; }
    public int? NextRecordIndex { get; set; }
    public bool Beep { get; set; }
    public bool UpdateStatus { get; set; }
    public int? BolusTicks { get; set; }
    public int? TempBasalTicks { get; set; }
    public int? TempBasalHalfHours { get; set; }
    public bool CancelTemp { get; set; }
    public bool CancelBolus { get; set; }
    public bool Deactivate { get; set; }
    public bool Remove { get; set; }
    public bool TestBolus { get; set; }
}