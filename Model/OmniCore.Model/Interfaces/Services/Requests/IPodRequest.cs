using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IPodRequest : IDisposable
    {
        Task<IPodRequest> Submit(CancellationToken cancellationToken);
        // Task ExecuteRequest();
        // IPodRequest WithoutAutoRescheduling(TimeSpan rescheduleWindow);
        // IPodRequest ScheduleStart(DateTimeOffset earliestStart, bool rescheduleOnRetry);
        // IPodRequest ScheduleRelative(IPodRequest otherRequest, bool onlyIfSuccessful, TimeSpan? );
        // IPodRequest ScheduleExpire(DateTimeOffset expireAfter, bool rescheduleOnRetry);
        // IPodRequest ScheduleAsap();
        // IPodRequest ScheduleAfterRequest(IPodRequest otherRequest);
        // IPodRequest ExpireAfter(DateTimeOffset requestStartEarliest);
        // IPodRequest ExecuteAfter(DateTimeOffset requestStartEarliest);
        // IPodRequest ExecuteFollowing(IPodRequest requestBefore, TimeSpan );
        // IPodRequest ExpireAfter(DateTimeOffset requestExpiry);
        // IPodRequest AutoSchedule();
    }
}