namespace OmniCore.Framework;

public class RaddResponse
{
    //public string? pod_id { get; set; }
    // public int? record_index { get; set; }
    public string? request_id { get; set; }
    public bool success { get; set; }
    public int? next_record_index { get; set; }
    public int? remaining { get; set; }

    public int? delivered { get; set; }
    public int? minutes { get; set; }
}