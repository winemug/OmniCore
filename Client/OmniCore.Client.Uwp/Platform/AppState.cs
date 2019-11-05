using OmniCore.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Uwp.Platform
{
    public class AppState : IAppState
    {
        public string GetString(string key, string defaultValue)
        {
            return null;
        }

        public bool TryGet(string key, out object value)
        {
            value = null;
            return false;
        }

        public bool TryRemove(string key)
        {
            return false;
        }

        public bool TrySet(string key, object value)
        {
            return false;
        }
    }
}
