using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseInfoExtendedPart : ResponseInfoPart
{
    public override RequestStatusType StatusType => RequestStatusType.Extended;
    
    public ResponseInfoExtendedPart(Bytes data)
    {
    }
}