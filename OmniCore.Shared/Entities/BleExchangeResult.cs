using OmniCore.Shared.Enums;

namespace OmniCore.Shared.Entities;

public class BleExchangeResult
{
    public BleCommunicationResult CommunicationResult { get; set; }
    public DateTimeOffset? BleWriteCompleted { get; set; }
    public DateTimeOffset? BleReadIndicated { get; set; }
    public RileyLinkResponse? ResponseCode { get; set; }
    public Bytes? ResponseData { get; set; }
    public Exception? Exception { get; set; }
}