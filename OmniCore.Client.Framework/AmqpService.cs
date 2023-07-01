using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Nito.AsyncEx;
using OmniCore.Common.Amqp;
using OmniCore.Common.Api;
using OmniCore.Common.Core;
using OmniCore.Common.Platform;
using OmniCore.Shared.Api;
using Polly;
using Polly.Timeout;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OmniCore.Framework;

public class AmqpService : IAmqpService
{
    private Task _amqpTask;
    private CancellationTokenSource _cts;
    
    private readonly AsyncProducerConsumerQueue<AmqpMessage> _publishQueue;
    private readonly AsyncProducerConsumerQueue<Task?> _confirmQueue;

    private ConcurrentBag<Func<AmqpMessage, Task<bool>>> _messageProcessors;

    private IAppConfiguration _appConfiguration;
    private IApiClient _apiClient;
    private IPlatformInfo _platformInfo;

    public AmqpService(IAppConfiguration appConfiguration,
        IApiClient apiClient,
        IPlatformInfo platformInfo)
    {
        _appConfiguration = appConfiguration;
        _apiClient = apiClient;
        _platformInfo = platformInfo;
        
        _publishQueue = new AsyncProducerConsumerQueue<AmqpMessage>();
        _confirmQueue = new AsyncProducerConsumerQueue<Task?>();
        _messageProcessors = new ConcurrentBag<Func<AmqpMessage, Task<bool>>>();
    }

    public async Task Start()
    {
        _cts = new CancellationTokenSource();
        _amqpTask = Task.Run(async () => await StartJoin(_cts.Token));
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

    private async Task StartJoin(CancellationToken cancellationToken)
    {
        while (_appConfiguration.ClientAuthorization == null)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }

        AmqpEndpointDefinition? endpointDefinition = null;
        while (true)
        {
            try
            {
                var result = await _apiClient.PostRequestAsync<ClientJoinRequest, ClientJoinResponse>(
                    Routes.ClientJoinRequestRoute, new ClientJoinRequest
                    {
                        Id = _appConfiguration.ClientAuthorization.ClientId,
                        Token = _appConfiguration.ClientAuthorization.Token,
                        Version = _platformInfo.GetVersion(),
                    }, cancellationToken);
                if (result is { Success: true })
                {
                    endpointDefinition = new AmqpEndpointDefinition
                    {
                        Dsn = result.Dsn,
                        Exchange = result.Exchange,
                        Queue = result.Queue,
                        UserId = result.Username
                    };
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        }
        
        var confirmationsTask = ConfirmationsTask(cancellationToken);
        while(true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await ConnectAndPublishLoop(endpointDefinition, cancellationToken);
            }
            catch(TaskCanceledException)
            {
                await confirmationsTask;
                throw;
            }
            catch(Exception e)
            {
                Trace.WriteLine($"Error while connectandpublish: {e.Message}");
                await Task.Delay(5000);
            }
        }
    }

    private async Task ConfirmationsTask(CancellationToken cancellationToken)
    {
        while(await _confirmQueue.OutputAvailableAsync(cancellationToken))
        {
            var confirmTask = await _confirmQueue.DequeueAsync(cancellationToken);
            try
            {
                await confirmTask;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error in user function handling publish confirmation: {e.Message}");
            }
            await Task.Yield();
        }
    }

    private async Task ConnectAndPublishLoop(AmqpEndpointDefinition endpointDefinition,
        CancellationToken cancellationToken)
    {
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(endpointDefinition.Dsn),
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
        subChannel.BasicConsume(endpointDefinition.Queue, false, consumer);
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
                    properties.UserId = endpointDefinition.UserId;
                    var sequenceNo = pubChannel.NextPublishSeqNo;
                    Debug.WriteLine($"publishing seq {sequenceNo} {message.Text}");
                    pubChannel.BasicPublish(endpointDefinition.Exchange, message.Route, false,
                        properties, Encoding.UTF8.GetBytes(message.Text));
                    pubChannel.WaitForConfirmsOrDie();
                    if (message.OnPublishConfirmed != null)
                    {
                        await _confirmQueue.EnqueueAsync(message.OnPublishConfirmed);
                    }
                }
                catch
                {
                    await _publishQueue.EnqueueAsync(message);
                    throw;
                }
            }
            await Task.Yield();
        }
    }

    private async Task<bool> ProcessMessage(AmqpMessage message)
    {
        Debug.WriteLine($"Incoming amqp message: {message.Text}");
        var processed = false;
        foreach (var pf in _messageProcessors)
        {
            try
            {
                processed = await pf(message);
            }
            catch (Exception e)
            {
                Trace.Write($"Error while processing {e}");
            }
        }
        return processed;
    }
    
    public void RegisterMessageProcessor(Func<AmqpMessage, Task<bool>> function)
    {
        _messageProcessors.Add(function);
    }
}