using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Common.Amqp
{
    public class AmqpProcessMessageRequest : AsyncRequestMessage<AmqpMessage>
    {
    }
}
