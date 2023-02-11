using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OmniCore.Services;
using OmniCore.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class AmqpTestViewModel : BaseViewModel
    {
        public Command StartCommand { get; }
        public Command StopCommand { get; }

        private IForegroundServiceHelper _foregroundServiceHelper;
        public AmqpTestViewModel()
        {
            StartCommand = new Command(StartClicked);
            StopCommand = new Command(StopClicked);
            _foregroundServiceHelper = App.Container.Resolve<IForegroundServiceHelper>();
        }

        private void StopClicked()
        {
            _foregroundServiceHelper.StopForegroundService();
        }

        private void StartClicked()
        {
            _foregroundServiceHelper.StartForegroundService();
        }
        
        private void StartClicked2()
        {
            var cf = new ConnectionFactory()            
            {
                Uri = new Uri("amqp://testere:redere@dev.balya.net/ocv")
            };

            var connection = cf.CreateConnection();
            var subChannel = connection.CreateModel();
            var pubChannel = connection.CreateModel();
            
            var consumer = new EventingBasicConsumer(subChannel);
            consumer.Received += (sender, ea) =>
            {
                Debug.WriteLine($"message received");
                subChannel.BasicAck(ea.DeliveryTag, false);
                Debug.WriteLine($"receive ackd");
                pubChannel.BasicPublish("", "ocq2", false, null, ea.Body);
                Debug.WriteLine($"published");

            };
            subChannel.BasicConsume("ocq1", false, consumer);
        }
    }
}