namespace OmniCore.Model.Interfaces.Services.Requests
{
    public interface IPodActivationRequest : IPodRequest
    {
        IPodActivationRequest WithRadio(IErosRadio radio);
        IPodActivationRequest WithNewRadioAddress(uint radioAddress);
        IPodActivationRequest WithPairAndPrime();
        IPodActivationRequest WithInjectAndStart(IDeliverySchedule deliverySchedule);
        IPodActivationRequest WithDeactivate();
    }
}