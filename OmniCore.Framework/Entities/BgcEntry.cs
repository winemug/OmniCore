using System;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Amqp;

namespace OmniCore.Services.Entities;

public enum BgcReadingType
{
    CGM = 0,
    Manual = 1
}

public enum BgcDirection
{
    DownFast = -3,
    Down = -2,
    DownSlow = -1,
    Flat = 0,
    UpSlow = 1,
    Up = 2,
    UpFast = 3
}

public enum BgcUnit
{
    mgDl = 0,
    mmolL = 1
}

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

    public byte[] AsMessageBody()
    {
        var msg = $"BGC::{ProfileId:N}::{Date.ToUnixTimeMilliseconds()}::{Value}::{Type}::{Direction}::{Rssi}";
        return Encoding.UTF8.GetBytes(msg);
    }

    public Task SetSyncedTask { get; set; }

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