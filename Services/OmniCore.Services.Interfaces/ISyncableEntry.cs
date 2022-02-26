using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces
{
    public interface ISyncableEntry
    {
        byte[] AsMessageBody();
        Task SetSyncedTask { get; set; }
    }
}