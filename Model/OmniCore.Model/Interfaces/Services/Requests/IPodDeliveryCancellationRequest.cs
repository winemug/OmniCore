using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Requests
{
    public interface IPodDeliveryCancellationRequest : IPodRequest
    {
        bool StopBolusDelivery { get; }
        bool StopExtendedBolusDelivery { get; }
        bool StopBasalDelivery { get; }

        IPodDeliveryCancellationRequest WithCancelImmediateBolus();
        IPodDeliveryCancellationRequest WithCancelExtendedBolus();
        IPodDeliveryCancellationRequest WithStopBasalDelivery();
        IPodDeliveryCancellationRequest CancelAll();
    }
}