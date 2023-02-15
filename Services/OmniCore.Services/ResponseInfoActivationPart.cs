using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseInfoActivationPart : ResponseInfoPart
{
    public override RequestStatusType StatusType => RequestStatusType.Activation;
    
    public ResponseInfoActivationPart(Bytes data)
    {
    }
}