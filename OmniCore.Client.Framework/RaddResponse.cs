namespace OmniCore.Framework;

public class RaddResponse
{
    //public string? pod_id { get; set; }
    // public int? record_index { get; set; }
    public string? RequestId { get; set; }
    public bool Success { get; set; }
    public int? NextRecordIndex { get; set; }
    public int? Remaining { get; set; }

    public int? Delivered { get; set; }
    public int? Minutes { get; set; }
}