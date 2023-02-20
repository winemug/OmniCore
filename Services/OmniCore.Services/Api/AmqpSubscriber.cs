using System;
using System.Diagnostics;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OmniCore.Services
{
    public class AmqpSubscriber : IDisposable
    {
        private IModel Channel;
        private AsyncEventingBasicConsumer Consumer;
        
        public void Start(IConnection connection, string queue)
        {
            Channel = connection.CreateModel();
            Channel.BasicQos(0, 30, true);
            Consumer = new AsyncEventingBasicConsumer(Channel);
            Consumer.Received += async (sender, ea) =>
            {
                var ch = (IModel)sender;
            
                Debug.WriteLine($"message received");
                ch.BasicAck(ea.DeliveryTag, false);
                await Task.Yield();
            };
            Channel.BasicConsume(queue, false, Consumer);
        }

        public void Dispose()
        {
            Channel?.Dispose();
            Consumer = null;
            Channel = null;
        }
    }
}
