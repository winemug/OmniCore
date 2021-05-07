using System;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IDeliverySchedule
    {
        Task<(DateTimeOffset StartTime, decimal HourlyRate, TimeSpan Duration)> GetSchedule(TimeSpan repeatingWindow);
    }
}