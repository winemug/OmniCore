using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model
{
    public class MessageExchangeResult : IMessageExchangeResult
    {
        public bool Success { get; private set; }

        public MessageExchangeResult(bool success)
        {
            Success = success;
        }
    }
}
