namespace OmniCore.Common.Pod;

public class PodFaultInfoModel
{
    public int FaultEventCode { get; init; }
    public int FaultEventMinute { get; init; }
    public bool FaultPulseInformationInvalid { get; init; }
    public bool FaultOcclusion { get; init; }
    public bool FaultInsulinStateTable { get; init; }
    public int FaultOcclusionType { get; init; }
    public bool FaultDuringImmediateBolus { get; init; }
    public PodProgress ProgressBeforeFault { get; init; }
}