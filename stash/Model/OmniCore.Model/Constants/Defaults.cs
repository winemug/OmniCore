using System;

namespace OmniCore.Model.Constants
{
    public static class Defaults
    {
        public static TimeSpan DatabaseSingleWriteTimeout = TimeSpan.FromSeconds(15);
        public static TimeSpan DatabaseSingleReadTimeout = TimeSpan.FromSeconds(10);
    }
}