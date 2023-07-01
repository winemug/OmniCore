using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

public class AmqpService : BackgroundService, IAmqpService
{
    private readonly AsyncProducerConsumerQueue<AmqpMessage> _publishQueue;
    private readonly AsyncProducerConsumerQueue<Task?> _confirmQueue;
    private readonly AsyncManualResetEvent _clientConfigured;

    private readonly ILogger<AmqpService> _logger;
    private IAppConfiguration _appConfiguration;
    private IApiClient _apiClient;
    private IPlatformInfo _platformInfo;

    public event EventHandler<bool> ReadyStateChanged;

    private bool _serviceReady;
    public bool ServiceReady
    {
        get
        {
            return _serviceReady;
        }
        private set
        {
            _serviceReady = value;
            ReadyStateChanged?.Invoke(this, value);
        }
    }

    public AmqpService(
        ILogger<AmqpService> logger,
        IAppConfiguration appConfiguration,
        IApiClient apiClient,
        IPlatformInfo platformInfo)
    {
        _serviceReady = false;
        _clientConfigured = new AsyncManualResetEvent();
        _logger = logger;
        _appConfiguration = appConfiguration;
        _apiClient = apiClient;
        _platformInfo = platformInfo;
        
        _publishQueue = new AsyncProducerConsumerQueue<AmqpMessage>();
        _confirmQueue = new AsyncProducerConsumerQueue<Task?>();
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
                await Task.Delay(5000, stoppingToken);
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
            //TODO:
            var message = new AmqpMessage
            {
                Body = ea.Body.ToArray(),
                DeliveryTag = ea.DeliveryTag
            };

            subChannel.BasicAck(ea.DeliveryTag, false);
            await Task.Yield();
            //try
            //{

            //    bool success = await ProcessMessage(message);
            //    if (success)
            //        subChannel.BasicAck(ea.DeliveryTag, false);
            //    else
            //        subChannel.BasicNack(ea.DeliveryTag, false, true);
            //}
            //catch (Exception e)
            //{
            //    subChannel.BasicNack(ea.DeliveryTag, false, true);
            //    Trace.WriteLine($"Message processing failed: {e}");
            //}
            //await Task.Yield();
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
                    //if (message.OnPublishConfirmed != null)
                    //{
                    //    await _confirmQueue.EnqueueAsync(message.OnPublishConfirmed);
                    //}
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
}