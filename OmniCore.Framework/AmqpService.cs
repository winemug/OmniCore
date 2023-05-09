using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Entities;
using Polly;
using Polly.Timeout;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OmniCore.Services;

public class AmqpService : IAmqpService
{
    public string Dsn { get; set; }
    public string Exchange { get; set; }
    public string Queue { get; set; }
    public string UserId { get; set; }
    
    private Task _amqpTask;
    private CancellationTokenSource _cts;
    private readonly SortedList<DateTimeOffset, string> _processedMessages;
    private readonly AsyncProducerConsumerQueue<AmqpMessage> _publishQueue;

    private ConcurrentBag<Func<AmqpMessage, Task<bool>>> _messageProcessors;
    public AmqpService()
    {
        _publishQueue = new AsyncProducerConsumerQueue<AmqpMessage>();
        _processedMessages = new SortedList<DateTimeOffset, string>();
        _messageProcessors = new ConcurrentBag<Func<AmqpMessage, Task<bool>>>();
    }

    public void SetEndpoint(AmqpEndpoint endpoint)
    {
        Dsn = endpoint.Dsn;
        Exchange = endpoint.Exchange;
        Queue = endpoint.Queue;
        UserId = endpoint.UserId;
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
        catch (TaskCanceledException) { }
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
        while(true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await ConnectAndPublishLoop(cancellationToken);
            }
            catch(TaskCanceledException) { throw; }
            catch(Exception e)
            {
                Trace.WriteLine($"Error while connectandpublish: {e.Message}");
                await Task.Delay(5000);
            }
        }
    }

    private async Task ConnectAndPublishLoop(CancellationToken cancellationToken)
    {
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(Dsn),
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = false,
        };
        Debug.WriteLine("connecting");
        using var connection = Policy<IConnection>
            .Handle<Exception>()
            .WaitAndRetryForever(
            sleepDurationProvider: retries =>
            {
                return TimeSpan.FromSeconds(Math.Min(retries * 3, 60));
            },
            onRetry: (ex, ts) =>
            {
                Trace.WriteLine($"Error {ex}, waiting {ts} to reconnect");
            }).Execute((_) => { return connectionFactory.CreateConnection(); }, cancellationToken);
        Debug.WriteLine("connected");

        using var pubChannel = connection.CreateModel();
        pubChannel.ConfirmSelect();
        cancellationToken.ThrowIfCancellationRequested();

        using var subChannel = connection.CreateModel();
        var consumer = new AsyncEventingBasicConsumer(subChannel);
        consumer.Received += async (sender, ea) =>
        {
            var message = new AmqpMessage
            {
                Body = ea.Body.ToArray(),
                Id = ea.BasicProperties.MessageId
            };
            try
            {
                bool success = await ProcessMessage(message);
                if (success)
                    subChannel.BasicAck(ea.DeliveryTag, false);
                else
                    subChannel.BasicNack(ea.DeliveryTag, false, true);
            }
            catch (Exception e)
            {
                subChannel.BasicNack(ea.DeliveryTag, false, true);
                Trace.WriteLine($"Message processing failed: {e}");
            }
            await Task.Yield();
        };
        subChannel.BasicConsume(Queue, false, consumer);
        cancellationToken.ThrowIfCancellationRequested();


        while (true)
        {
            var result = false;
            try
            {
                result = await Policy.TimeoutAsync(30)
                    .ExecuteAsync((t) => _publishQueue.OutputAvailableAsync(t), cancellationToken);
            }
            catch (TimeoutRejectedException ex) { }

            if (!connection.IsOpen)
                break;

            if (result)
            {
                var message = await _publishQueue.DequeueAsync(cancellationToken);
                // cancellation ignored below this point
                try
                {
                    var properties = pubChannel.CreateBasicProperties();
                    var sequenceNo = pubChannel.NextPublishSeqNo;
                    Debug.WriteLine($"publishing seq {sequenceNo} {message.Text}");
                    pubChannel.BasicPublish(Exchange, "", false,
                        properties, Encoding.UTF8.GetBytes(message.Text));
                    Debug.WriteLine($"published");
                    pubChannel.WaitForConfirmsOrDie();
                    Debug.WriteLine($"confirmed");
                    if (message.OnPublishConfirmed != null)
                    {
                        try
                        {
                            await message.OnPublishConfirmed();
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine($"Error in user function handling publish confirmation: {e.Message}");
                        }
                    }
                }
                catch
                {
                    await _publishQueue.EnqueueAsync(message);
                    throw;
                }
            }
        }
    }

    private async Task<bool> ProcessMessage(AmqpMessage message)
    {
        if (!string.IsNullOrEmpty(message.Id) && _processedMessages.ContainsValue(message.Id))
        {
            Debug.WriteLine($"Message with id {message.Id} already processed");
            return true;
        }

        Debug.WriteLine($"Incoming amqp message: {message.Text}");
        var processed = false;
        foreach (var pf in _messageProcessors)
        {
            try
            {
                processed = await pf(message);
                if (processed)
                {
                    _processedMessages.Add(DateTimeOffset.Now, message.Id);
                    break;
                }
            }
            catch (Exception e)
            {
                Trace.Write($"Error while processing {e}");
            }
        }

        var keysToRemove = _processedMessages.Where(p =>
                p.Key < DateTimeOffset.Now - TimeSpan.FromMinutes(15))
            .Select(p => p.Key)
            .ToArray();
        foreach (var key in keysToRemove) _processedMessages.Remove(key);

        return processed;
    }
    
    public void RegisterMessageProcessor(Func<AmqpMessage, Task<bool>> function)
    {
        _messageProcessors.Add(function);
    }
}