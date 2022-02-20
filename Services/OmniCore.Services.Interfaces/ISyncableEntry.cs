using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces
{
    public interface ISyncableEntry
    {
        byte[] GetMessageBody();
        long DbRowId { get; }
        string DbTableName { get;  }
    }
}