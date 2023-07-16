using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
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

public class AmqpService : BackgroundService, IAmqpService
{
    private readonly AsyncQueue<AmqpMessage> _publishRequestQueue;
    private readonly AsyncQueue<AmqpMessage> _publishConfirmNotifyQueue;
    private readonly AsyncManualResetEvent _clientConfigured;
    private readonly SynchronizedCollection<Func<AmqpMessage, Task<bool>>> _messageHandlers;

    private readonly ILogger<AmqpService> _logger;
    private IAppConfiguration _appConfiguration;
    private IApiClient _apiClient;
    private IPlatformInfo _platformInfo;

    public AmqpService(
        ILogger<AmqpService> logger,
        IAppConfiguration appConfiguration,
        IApiClient apiClient,
        IPlatformInfo platformInfo)
    {
        _clientConfigured = new AsyncManualResetEvent();
        _logger = logger;
        _appConfiguration = appConfiguration;
        _apiClient = apiClient;
        _platformInfo = platformInfo;
        
        _publishRequestQueue = new AsyncQueue<AmqpMessage>();
        _publishConfirmNotifyQueue = new AsyncQueue<AmqpMessage>();
        _messageHandlers = new SynchronizedCollection<Func<AmqpMessage, Task<bool>>>();
    }

    public void RegisterMessageHandler(Func<AmqpMessage, Task<bool>> handler)
    {
        _messageHandlers.Add(handler);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var notificationTask = PublishNotificationsAsync(stoppingToken);
        _appConfiguration.ConfigurationChanged += OnConfigurationChanged;
        while (true)
        {
            try
            {
                await _clientConfigured.WaitAsync(stoppingToken);
                var endpoint = await GetEndpointAsync(stoppingToken);
                if (endpoint == null)
                    throw new ApplicationException("Null received for endpoint definition");
                await ConnectToEndpointAsync(endpoint, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in main loop");
                await Task.Delay(15000, stoppingToken);
            }
        }

        try
        {
            await notificationTask;
        }
        catch (OperationCanceledException)
        {
        }
        
        _appConfiguration.ConfigurationChanged -= OnConfigurationChanged;
    }

    private void OnConfigurationChanged(object? sender, EventArgs e)
    {
        if (_appConfiguration.ClientAuthorization == null)
        {
            _clientConfigured.Reset();
            return;
        }
        _clientConfigured.Set();
    }

    public void PublishMessage(AmqpMessage message)
    {
        if (message.DeferToLatest == null)
            message.DeferToLatest = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(15); 
        _publishRequestQueue.Enqueue(message);
    }

    private async Task<AmqpEndpointDefinition?> GetEndpointAsync(CancellationToken cancellationToken)
    {
        if (_appConfiguration.ClientAuthorization == null)
            return null;

        if (_appConfiguration.Endpoint != null)
            return _appConfiguration.Endpoint;
        
        AmqpEndpointDefinition? endpointDefinition = null;
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
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "while retrieving endpoint");
            throw;
        }

        if (endpointDefinition != null)
            _appConfiguration.Endpoint = endpointDefinition;
        
        return endpointDefinition;
    }

    private async Task ConnectToEndpointAsync(
        AmqpEndpointDefinition endpointDefinition,
        CancellationToken cancellationToken)
    {
        while(true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await ConnectAndPublishAsync(endpointDefinition, cancellationToken);
            }
            catch(TaskCanceledException)
            {
                throw;
            }
            catch(Exception e)
            {
                Trace.WriteLine($"Error while connectandpublish: {e.Message}");
                await Task.Delay(5000);
            }
        }
    }

    private async Task ConnectAndPublishAsync(AmqpEndpointDefinition endpointDefinition,
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

        var consumer = new AmqpConsumer(_messageHandlers, endpointDefinition.Queue);
        subChannel.BasicConsume(endpointDefinition.Queue, false, consumer);
        cancellationToken.ThrowIfCancellationRequested();

        while (true)
        {
            var message = await _publishRequestQueue.DequeueAsync(cancellationToken);
            try
            {
                var properties = pubChannel.CreateBasicProperties();
                properties.UserId = message.UserId;
                var sequenceNo = pubChannel.NextPublishSeqNo;
                pubChannel.BasicPublish(message.Exchange, message.Route, false,
                    properties, Encoding.UTF8.GetBytes(message.Text));
                pubChannel.WaitForConfirmsOrDie();
                message.PublishSequence = sequenceNo;
                Debug.WriteLine($"published {message.Text}");
            }
            catch
            {
                _publishRequestQueue.Enqueue(message);
                throw;
            }
            _publishConfirmNotifyQueue.Enqueue(message);
        }
    }
    private async Task PublishNotificationsAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var message = await _publishConfirmNotifyQueue.DequeueAsync(cancellationToken);
            if (message.WhenPublished != null)
            {
                try
                {
                    await message.WhenPublished();
                }
                catch (Exception e)
                {
                    _logger.LogError(e,"Error in notification function for published message");
                }
            }
        }
    }
}

public class AmqpConsumer : AsyncDefaultBasicConsumer
{
    private readonly SynchronizedCollection<Func<AmqpMessage, Task<bool>>> _handlers;
    private readonly string _queue;

    public AmqpConsumer(SynchronizedCollection<Func<AmqpMessage, Task<bool>>> handlers, string queue):base()
    {
        _handlers = handlers;
        _queue = queue;
    }
    public override async Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
    {
        var message = new AmqpMessage
        {
            Tag = deliveryTag,
            Route = routingKey,
            UserId = properties.UserId,
            Body = body.ToArray(),
            Queue = _queue,
        };

        var ack = false;
        foreach (var messageHandler in _handlers.ToList())
        {
            try
            {
                ack = await messageHandler(message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while handling message: {e}");
            }
            if (ack)
                break;
        }
            
        if (ack)
            Model.BasicAck(message.Tag, false);
        else
            Model.BasicNack(message.Tag, false, true);
    }
}