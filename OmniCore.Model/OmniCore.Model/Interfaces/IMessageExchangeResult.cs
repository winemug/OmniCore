using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeResult
    {
        bool Success { get; }

        Exception Error { get; }
    }
}
