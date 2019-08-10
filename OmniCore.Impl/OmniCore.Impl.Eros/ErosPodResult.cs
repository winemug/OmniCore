using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;

namespace OmniCore.Impl.Eros
{
    public class ErosPodResult : IPodResult
    {
        public ErosPodResult(ResultType resultType, Exception exception = null)
        {
            ResultType = resultType;
            Exception = exception;
        }

        public ResultType ResultType { get; }
        public Exception Exception { get; }
    }
}
