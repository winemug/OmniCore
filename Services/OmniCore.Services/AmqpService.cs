using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OmniCore.Services;

public class AmqpService : IAmqpService
{
    private Task _amqpTask;
    private CancellationTokenSource _cts;
    private readonly SortedList<DateTimeOffset, string> _processedMessages;
    private readonly AsyncProducerConsumerQueue<AmqpMessage> _publishQueue;

    public AmqpService()
    {
        _publishQueue = new AsyncProducerConsumerQueue<AmqpMessage>();
        _processedMessages = new SortedList<DateTimeOffset, string>();
    }

    public async Task Start()
    {
        _cts = new CancellationTokenSource();
        _amqpTask = Task.Run(async () => await AmqpTask(_cts.Token));
    }

    public async Task Stop()
    {
        try
        {
            _cts?.Cancel();
            _amqpTask?.GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error while cancelling core task: {e}");
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    public async Task PublishMessage(AmqpMessage message)
    {
        try
        {
            await _publishQueue.EnqueueAsync(message);
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Error enqueuing message {e}");
            throw;
        }
    }

    private async Task AmqpTask(CancellationToken cancellationToken)
    {
        var cf = new ConnectionFactory
        {
            Uri = new Uri("amqp://user0:user0@192.168.1.40/oc"),
            DispatchConsumersAsync = true
        };

        IConnection connection = null;
        try
        {
            while (connection == null)
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
                var message = new AmqpMessage
                {
                    Body = ea.Body.ToArray(),
                    Id = ea.BasicProperties.MessageId
                };
                await ProcessMessage(message);
                subChannel.BasicAck(ea.DeliveryTag, false);
                await Task.Yield();
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error while processing: {e}");
            }
        };
        subChannel.BasicConsume("quser0", false, consumer);

        var pendingConfirmations = new ConcurrentDictionary<ulong, AmqpMessage>();
        var pubChannel = connection.CreateModel();
        pubChannel.ConfirmSelect();

        // pubChannel.BasicAcks += (sender, args) =>
        // {
        //     if (args.Multiple)
        //     {
        //         var confirmed = pendingConfirmations.Where
        //             (k => k.Key <= args.DeliveryTag);
        //         foreach (var kvp in confirmed)
        //         {
        //             pendingConfirmations.TryRemove(kvp.Key, out AmqpMessage message);
        //         }
        //     }
        //     else
        //     {
        //         pendingConfirmations.TryRemove(args.DeliveryTag, out AmqpMessage message);
        //     }
        // };
        //
        // pubChannel.BasicNacks += async (sender, args) =>
        // {
        //     if (args.Multiple)
        //     {
        //         var unconfirmed = pendingConfirmations.Where
        //             (k => k.Key <= args.DeliveryTag);
        //         foreach (var kvp in unconfirmed)
        //         {
        //             pendingConfirmations.TryRemove(kvp.Key, out AmqpMessage message);
        //             await PublishMessage(message);
        //         }
        //     }
        //     else
        //     {
        //         pendingConfirmations.TryRemove(args.DeliveryTag, out AmqpMessage message);
        //         await PublishMessage(message);
        //     }
        // };

        while (true)
            try
            {
                var processQueue = false;
                if (pendingConfirmations.IsEmpty)
                    processQueue = await _publishQueue.OutputAvailableAsync(cancellationToken);
                else
                    using (var queueTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
                    {
                        try
                        {
                            processQueue = await _publishQueue.OutputAvailableAsync(queueTimeout.Token);
                        }
                        catch (TaskCanceledException)
                        {
                        }
                    }

                if (processQueue)
                {
                    var message = await _publishQueue.DequeueAsync(cancellationToken);
                    var sequenceNo = pubChannel.NextPublishSeqNo;

                    try
                    {
                        var properties = pubChannel.CreateBasicProperties();
                        properties.UserId = "user0";
                        pendingConfirmations.TryAdd(sequenceNo, message);
                        pubChannel.BasicPublish("eclient", "*", false,
                            properties, message.Body);
                        Debug.WriteLine($"published message: {message.Text}");
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine($"Error while publishing: {e}");
                        pendingConfirmations.TryRemove(sequenceNo, out message);
                        await _publishQueue.EnqueueAsync(message, cancellationToken);
                    }
                }
                else
                {
                    if (!pendingConfirmations.IsEmpty)
                    {
                        try
                        {
                            Debug.WriteLine("Starting confirmations");
                            pubChannel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(15));
                            Debug.WriteLine("Confirmation succeeded");
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine($"Error while confirming: {e}");
                            foreach (var pendingMessage in pendingConfirmations.Values)
                                _publishQueue.Enqueue(pendingMessage);
                        }

                        pendingConfirmations.Clear();
                    }
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }

        subChannel.Close();
        pubChannel.Close();
        connection.Close();
        connection.Dispose();
    }

    private async Task ProcessMessage(AmqpMessage message)
    {
        var alreadyProcessed = false;
        if (!string.IsNullOrEmpty(message.Id))
        {
            if (!_processedMessages.ContainsValue(message.Id))
            {
                _processedMessages.Add(DateTimeOffset.Now, message.Id);
            }
            else
            {
                alreadyProcessed = true;
                Debug.WriteLine($"Message with id {message.Id} already processed");
            }
        }

        if (!alreadyProcessed) Debug.WriteLine($"Incoming amqp message: {message.Text}");

        var keysToRemove = _processedMessages.Where(p =>
                p.Key < DateTimeOffset.Now - TimeSpan.FromMinutes(15))
            .Select(p => p.Key)
            .ToArray();
        foreach (var key in keysToRemove) _processedMessages.Remove(key);
    }
}