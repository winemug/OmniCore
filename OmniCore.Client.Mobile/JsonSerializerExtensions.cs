using System.Text.Json;

namespace OmniCore.Client.Mobile;

public static class JsonSerializerExtensions
{
    public static T? TryDeserialize<T>(this string? json) where T : class
    {
        try
        {
            if (json != null)
                return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception)
        {
            // ignored
        }

        return null;
    }

    public static string? TrySerialize<T>(this T? obj) where T : class
    {
        try
        {
            if (obj != null)
                return JsonSerializer.Serialize(obj);
        }
        catch (Exception)
        {
            // ignored
        }

        return null;
    }
}