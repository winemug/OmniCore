using System.Text.Json;

namespace OmniCore.Shared.Extensions;

public static class JsonSerializerWrapper
{
    public static T? TryDeserialize<T>(string? json) where T : class
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
    
    public static string? TrySerialize<T>(T? obj) where T : class
    {
        try
        {
            if (obj != null)
                return JsonSerializer.Serialize<T>(obj);
        }
        catch (Exception)
        {
            // ignored
        }

        return null;
    }
}