using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces;

public interface ISyncableEntry
{
    Task SetSyncedTask { get; set; }
    byte[] AsMessageBody();
}