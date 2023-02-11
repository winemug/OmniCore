using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace OmniCore.Services
{
    public class CoreService : ICoreService
    {
        private AsyncProducerConsumerQueue<AmqpMessage> _publishQueue = new();
        private SortedList<DateTimeOffset,string> _processedMessages = new();
        private RadioService _radioService;
        
        public CoreService(RadioService radioService)
        {
            _radioService = radioService;
        }

        private Task _coreTask;
        private CancellationTokenSource _coreCancellation;
        
        public void Start()
        {
            // _coreCancellation = new CancellationTokenSource();
            // _coreTask = CoreTask(_coreCancellation.Token);
            _radioService.Start();
        }

        public void Stop()
        {
            _radioService.Stop();
            try
            {
                // _coreCancellation?.Cancel();
                // _coreTask?.GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error while cancelling core task: {e}");
            }
            finally
            {
                // _coreCancellation?.Dispose();
            }
        }

        private async Task CoreTask(CancellationToken cancellationToken)
        {
            var cf = new ConnectionFactory()            
            {
                Uri = new Uri("amqp://testere:redere@dev.balya.net/ocv"),
                DispatchConsumersAsync = true,
            };
            
            IConnection connection = null;
            try
            {
                while (connection == null)
                {
                    try
                    {
                        connection = cf.CreateConnection();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine($"Connection failed {e}");
                        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return;
            }                    

            var subChannel = connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(subChannel);
            consumer.Received += async (sender, ea) =>
            {
                try
                {
                    var message = new AmqpMessage {Body = ea.Body.ToArray(),
                        Id = ea.BasicProperties.MessageId};
                    await ProcessMessage(message);
                    subChannel.BasicAck(ea.DeliveryTag, false);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    Trace.WriteLine($"Error while processing: {e}");
                }
            };
            subChannel.BasicConsume("ocq1", false, consumer);

            var pendingConfirmations = new ConcurrentDictionary<ulong, AmqpMessage>();
            var pubChannel = connection.CreateModel();
            pubChannel.ConfirmSelect();

            pubChannel.BasicAcks += (sender, args) =>
            {
                if (args.Multiple)
                {
                    var confirmed = pendingConfirmations.Where
                        (k => k.Key <= args.DeliveryTag);
                    foreach (var kvp in confirmed)
                    {
                        pendingConfirmations.TryRemove(kvp.Key, out AmqpMessage message);
                    }
                }
                else
                {
                    pendingConfirmations.TryRemove(args.DeliveryTag, out AmqpMessage message);
                }
            };

            pubChannel.BasicNacks += async (sender, args) =>
            {
                if (args.Multiple)
                {
                    var unconfirmed = pendingConfirmations.Where
                        (k => k.Key <= args.DeliveryTag);
                    foreach (var kvp in unconfirmed)
                    {
                        pendingConfirmations.TryRemove(kvp.Key, out AmqpMessage message);
                        await PublishMessage(message);
                    }
                }
                else
                {
                    pendingConfirmations.TryRemove(args.DeliveryTag, out AmqpMessage message);
                    await PublishMessage(message);
                }
            };
            
            while (true)
            {
                var queueTask = _publishQueue.OutputAvailableAsync(cancellationToken);
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);

                try
                {
                    var retTask = await Task.WhenAny(queueTask, timeoutTask);
                    if (retTask == queueTask)
                    {
                        var message = await _publishQueue.DequeueAsync();
                        var sequenceNo = pubChannel.NextPublishSeqNo;
                    
                        try
                        {
                            IBasicProperties properties = pubChannel.CreateBasicProperties();
                            // props.UserId = ""
                            pendingConfirmations.TryAdd(sequenceNo, message);
                            pubChannel.BasicPublish("", "ocq2", false,
                                properties, message.Body);
                            // Debug.WriteLine($"published message: {message.Text}");
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine($"Error while publishing: {e}");
                            pendingConfirmations.TryRemove(sequenceNo, out message);
                            _publishQueue.Enqueue(message);
                        }
                    }
                    else
                    {
                        await timeoutTask;
                        try
                        {
                            if (!pendingConfirmations.IsEmpty)
                            {
                                Debug.WriteLine($"Starting confirmations");
                                pubChannel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(15));
                                Debug.WriteLine($"Confirmation succeeded");
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine($"Error while confirming: {e}");
                            foreach (var pendingMessage in pendingConfirmations.Values)
                            {
                                _publishQueue.Enqueue(pendingMessage);
                            }
                        }
                        pendingConfirmations.Clear();
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            subChannel.Close();
            pubChannel.Close();
            connection.Close();
            connection.Dispose();
        }

        
        public async Task PublishMessage(AmqpMessage message)
        {
            // await _publishQueue.EnqueueAsync(message);
        }

        private async Task ProcessMessage(AmqpMessage message)
        {
            if (!_processedMessages.ContainsValue(message.Id))
            {
                Trace.WriteLine($"Nice message this: {message.Text}");
                await PublishMessage(new() { Text = $"goodboi {message.Text}" });
                _processedMessages.Add(DateTimeOffset.Now, message.Id);
            }
            else
            {
                Trace.WriteLine($"Message with id {message.Id} already processed, skipping.");
            }

            var keysToRemove = _processedMessages.Where(p =>
                p.Key < DateTimeOffset.Now - TimeSpan.FromMinutes(15))
                .Select(p => p.Key)
                .ToArray();
            foreach (var key in keysToRemove)
            {
                _processedMessages.Remove(key);
            }
        }
    }
}