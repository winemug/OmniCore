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
    private readonly AsyncQueue<AmqpMessage> _pendingPublish;
    private readonly AsyncQueue<AmqpMessage> _pendingNotifyPublished;

    private readonly AsyncManualResetEvent _publishRequested;
    private readonly AsyncManualResetEvent _clientConfigured;

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
        _publishRequested = new AsyncManualResetEvent();
        _clientConfigured = new AsyncManualResetEvent();
        _logger = logger;
        _appConfiguration = appConfiguration;
        _apiClient = apiClient;
        _platformInfo = platformInfo;
        
        _pendingPublish = new AsyncQueue<AmqpMessage>();
        _pendingNotifyPublished = new AsyncQueue<AmqpMessage>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _appConfiguration.ConfigurationChanged += OnConfigurationChanged;
        while (true)
        {
            try
            {
                await _clientConfigured.WaitAsync(stoppingToken);
                var endpoint = await GetEndpoint(stoppingToken);
                if (endpoint == null)
                    throw new ApplicationException("Null received for endpoint definition");
                await ConnectToEndpoint(endpoint, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in main loop");
                await Task.Delay(30000, stoppingToken);
            }
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
        _pendingPublish.Enqueue(message);
    }

    private async Task<AmqpEndpointDefinition?> GetEndpoint(CancellationToken cancellationToken)
    {
        if (_appConfiguration.ClientAuthorization == null)
            return null;
        
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
        catch (Exception e)
        {
            _logger.LogError(e, "while retrieving endpoint");
            throw;
        }
        return endpointDefinition;
    }

    private async Task ConnectToEndpoint(
        AmqpEndpointDefinition endpointDefinition,
        CancellationToken cancellationToken)
    {
        while(true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await ConnectAndPublishLoop(endpointDefinition, cancellationToken);
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

        var consumer = new AmqpConsumer();
        subChannel.BasicConsume(endpointDefinition.Queue, false, consumer);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_pendingPublish.IsEmpty)
            _publishRequested.Set();
        
        while(true)
        {
            while (true)
            {
                await _publishRequested.WaitAsync(cancellationToken);
                _publishRequested.Reset();
                await Task.Delay(3000, cancellationToken);
                if (!_publishRequested.IsSet || _pendingPublish.Count > 10)
                    break;
            }

            var published = new List<AmqpMessage>(_pendingPublish.Count);
            while (!_pendingPublish.IsEmpty)
            {
                var message = await _pendingPublish.DequeueAsync(cancellationToken);
                try
                {
                    var properties = pubChannel.CreateBasicProperties();
                    properties.UserId = message.UserId;
                    var sequenceNo = pubChannel.NextPublishSeqNo;
                    Debug.WriteLine($"publishing seq {sequenceNo} {message.Text}");
                    pubChannel.BasicPublish(message.Exchange, message.Route, false,
                        properties, Encoding.UTF8.GetBytes(message.Text));
                    message.Sequence = sequenceNo;
                    published.Add(message);
                }
                catch
                {
                    _pendingPublish.Enqueue(message);
                    throw;
                }
            }

            try
            {
                pubChannel.WaitForConfirmsOrDie();
            }
            catch (Exception)
            {
                foreach(var msg in published)
                    _pendingPublish.Enqueue(msg);
                throw;
            }
            
            foreach(var msg in published)
                _pendingNotifyPublished.Enqueue(msg);
        }
    }
}
public class AmqpConsumer : AsyncDefaultBasicConsumer
{
    public override async Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
    {
        var message = new AmqpMessage
        {
            Tag = deliveryTag,
            Route = routingKey,
            UserId = properties.UserId,
            Body = body.ToArray()
        };

        //TODO: process
        message.Acknowledge = true;
        //
        if (message.Acknowledge)
            Model.BasicAck(message.Tag, false);
        else
            Model.BasicNack(message.Tag, false, true);
    }
}