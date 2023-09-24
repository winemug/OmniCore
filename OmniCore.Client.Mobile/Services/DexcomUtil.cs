using OmniCore.Framework.Entities;

namespace OmniCore.Framework.Data;

public static class DexcomUtil
{
    public static TimeSpan GetRefreshInterval(DateTimeOffset lastReadingDate, bool hasGaps)
    {
        TimeSpan refreshInterval;
        var now = DateTimeOffset.UtcNow;
        var dMod = (now - lastReadingDate).TotalSeconds % 300;
        if (now - lastReadingDate < TimeSpan.FromSeconds(300))
        {
            refreshInterval = TimeSpan.FromSeconds(303 - dMod);
        }
        else
        {
            if (dMod <= 85 || dMod >= 295)
                refreshInterval = TimeSpan.FromSeconds(3);
            else if (dMod > 240)
                refreshInterval = TimeSpan.FromSeconds(298 - dMod);
            else
                refreshInterval = TimeSpan.FromSeconds(60);
        }

        if (hasGaps)
        {
            var refreshWhenGaps = TimeSpan.FromSeconds(60);
            return refreshInterval < refreshWhenGaps ? refreshInterval : refreshWhenGaps;
        }

        return refreshInterval;
    }

    public static BgcDirection? GetDirection(string directionString)
    {
        switch (directionString.ToLowerInvariant())
        {
            case "doubledown":
                return BgcDirection.DownFast;
            case "doubleup":
                return BgcDirection.UpFast;
            case "singledown":
                return BgcDirection.Down;
            case "singleup":
                return BgcDirection.Up;
            case "fortyfivedown":
            case "45down":
                return BgcDirection.DownSlow;
            case "fortyfiveup":
            case "45up":
                return BgcDirection.UpSlow;
            case "flat":
                return BgcDirection.Flat;
            default:
                return null;
        }
    }
}