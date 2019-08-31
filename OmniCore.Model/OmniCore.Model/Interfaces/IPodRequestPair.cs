using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodRequestPair : IPodRequest
    {
        uint RadioAddress { get; }
    }
}
