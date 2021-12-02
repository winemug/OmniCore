using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Services.Data;

namespace OmniCore.Services.Interfaces
{
    public interface IDataStore
    {
        string DatabasePath { get; }
        Task Initialize();
        Task AddBgcReadings(IEnumerable<BgcReading> bgcReadings);
    }
}