using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Common.Amqp
{
    public record AmqpMessageReceivedMessage
    {
        public AmqpMessage Message { get; init; }
    }
}
