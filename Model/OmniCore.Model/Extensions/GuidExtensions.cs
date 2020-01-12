using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Extensions
{
    public static class GuidExtensions
    {
        public static string AsMacAddress(this Guid value)
        {
            var gb = value.ToByteArray();
            return $"{gb[10]:X2}:{gb[11]:X2}:{gb[12]:X2}:{gb[13]:X2}:{gb[14]:X2}:{gb[15]:X2}";
        }
    }
}
