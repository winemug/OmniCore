using System;
using System.Diagnostics;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xamarin.Forms;

namespace OmniCore.Services.Data
{
    public static class AmqpTestClient
    {
        private static IConnectionFactory connectionFactory;
        private static IConnection connection;
        private static IModel model;
        private static EventingBasicConsumer consumer;

        public static void InitializeClient()
        {
            connectionFactory = new ConnectionFactory()
            {
                UserName = "guest",
                Password = "guest",
                HostName = "192.168.1.40"
            };
            connection = connectionFactory.CreateConnection();
            model = connection.CreateModel();
            consumer = new EventingBasicConsumer(model);
            consumer.Received += ConsumerOnReceived;
            consumer.Registered += ConsumerOnRegistered;
            consumer.Shutdown += ConsumerOnShutdown;
            consumer.Unregistered += ConsumerOnUnregistered;
            consumer.ConsumerCancelled += ConsumerOnConsumerCancelled;
            model.BasicConsume("oc-ping", true, consumer);

        }

        private static void ConsumerOnConsumerCancelled(object sender, ConsumerEventArgs e)
        {
            Debug.WriteLine("consumer on cancelled");
        }

        private static void ConsumerOnUnregistered(object sender, ConsumerEventArgs e)
        {
            Debug.WriteLine("consumer on unreg");
        }

        private static void ConsumerOnShutdown(object sender, ShutdownEventArgs e)
        {
            Debug.WriteLine("consumer on shutdown");
        }

        private static void ConsumerOnRegistered(object sender, ConsumerEventArgs e)
        {
            Debug.WriteLine("consumer on registered");
        }

        private static void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            Debug.WriteLine("consumer on received");
            model.BasicPublish("", "oc-pong", false, null,
                System.Text.Encoding.UTF8.GetBytes("PONG"));
        }

    }
}