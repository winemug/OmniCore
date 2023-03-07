using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces.Amqp;

public interface ISyncableEntry
{
    Task SetSyncedTask { get; set; }
    byte[] AsMessageBody();
}