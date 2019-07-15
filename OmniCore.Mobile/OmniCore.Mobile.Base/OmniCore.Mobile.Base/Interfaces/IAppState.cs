namespace OmniCore.Mobile.Base.Interfaces
{
    public interface IAppState
    {
        bool TryGet(string key, out object value);
        bool TrySet(string key, object value);
        bool TryRemove(string key);
        string GetString(string key, string defaultValue);
    }
}