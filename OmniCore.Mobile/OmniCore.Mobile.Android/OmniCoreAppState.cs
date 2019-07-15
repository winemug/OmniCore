using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using OmniCore.Mobile.Base.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Mobile.Android
{
    public class OmniCoreAppState : IAppState
    {
        public string GetString(string key, string defaultValue)
        {
            if (Application.Current.Properties.ContainsKey(key))
                return Application.Current.Properties[key] as string ?? defaultValue;
            return defaultValue;
        }

        public bool TryGet(string key, out object value)
        {
            lock (Application.Current.Properties)
            {
                if (Application.Current.Properties.ContainsKey(key))
                {
                    value = Application.Current.Properties[key];
                    return true;
                }

                value = null;
                return false;
            }
        }

        public bool TrySet(string key, object value)
        {
            lock (Application.Current.Properties)
            {
                if (!Application.Current.Properties.ContainsKey(key))
                {
                    Application.Current.Properties.Add(new KeyValuePair<string, object>(key, value));
                    return true;
                }
                return false;
            }
        }

        public bool TryRemove(string key)
        {
            lock (Application.Current.Properties)
            {
                if (Application.Current.Properties.ContainsKey(key))
                {
                    return Application.Current.Properties.Remove(key);
                }
                return false;
            }
        }
    }
}
