using System;
using RabbitMQ.Client;

namespace OmniCore.Services;

public class AmqpConnection
{
    private IConnection Connection;
    private IConnectionFactory ConnectionFactory;

    public void Start(string dsn)
    {
        ConnectionFactory = new ConnectionFactory
        {
            Uri = new Uri(dsn)
        };
    }
}