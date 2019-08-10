using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPodResult
    {
        ResultType ResultType { get; }
        Exception Exception { get; }
    }
}
