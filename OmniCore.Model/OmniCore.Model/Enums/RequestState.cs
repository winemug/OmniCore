 using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Enums
{
    public enum RequestState
    {
        Queued,
        Running,
        Canceled,
        Failed,
        Succeeded
    }
}
