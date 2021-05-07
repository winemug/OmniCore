using System;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Requests
{
    public interface IPodScheduledDeliveryRequest : IPodRequest
    {
        IPodScheduledDeliveryRequest WithDeliverySchedule(IDeliverySchedule schedule);
        IPodScheduledDeliveryRequest WithTimeOffset(DateTimeOffset timeOffset);
        IPodScheduledDeliveryRequest WithTemporaryRate(decimal hourlyRateUnits, TimeSpan duration);
        IPodScheduledDeliveryRequest WithScheduledRate();
    }
}