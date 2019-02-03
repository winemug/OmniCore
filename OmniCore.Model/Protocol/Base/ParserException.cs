using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Protocol.Base
{
    public class ParserException : Exception
    {
        public ParserException(string errMessage) : base(errMessage)
        {
        }
    }
}
