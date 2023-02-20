using System;
using RabbitMQ.Client;

namespace OmniCore.Services
{
    public class AmqpConnection
    {
        private IConnectionFactory ConnectionFactory;
        private IConnection Connection;

        public void Start(string dsn)
        {
            ConnectionFactory = new ConnectionFactory()            
            {
                Uri = new Uri(dsn)
            };
        }
    }
}