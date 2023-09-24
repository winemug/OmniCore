using OmniCore.Common.Entities;
using OmniCore.Common.Radio;

namespace OmniCore.Common.Pod;

public class BleExchangeResult
{
    public BleCommunicationResult CommunicationResult { get; set; }
    public DateTimeOffset? BleWriteCompleted { get; set; }
    public DateTimeOffset? BleReadIndicated { get; set; }
    public RileyLinkResponse? ResponseCode { get; set; }
    public Bytes? ResponseData { get; set; }
    public Exception? Exception { get; set; }
}