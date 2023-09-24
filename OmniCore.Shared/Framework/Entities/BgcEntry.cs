using System.Text;

namespace OmniCore.Framework.Entities;

public class BgcEntry
{
    public Guid ProfileId { get; set; }
    public Guid ClientId { get; set; }
    public DateTimeOffset Date { get; set; }
    public BgcReadingType? Type { get; set; }
    public BgcDirection? Direction { get; set; }
    public int? Rssi { get; set; }
    public double Value { get; set; }
    public bool Deleted { get; set; }

    public Task SetSyncedTask { get; set; }

    public byte[] AsMessageBody()
    {
        var msg = $"BGC::{ProfileId:N}::{Date.ToUnixTimeMilliseconds()}::{Value}::{Type}::{Direction}::{Rssi}";
        return Encoding.UTF8.GetBytes(msg);
    }

    public override bool Equals(object obj)
    {
        var x = (BgcEntry)obj;
        return ProfileId == x.ProfileId && Date == x.Date && Type == x.Type;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}