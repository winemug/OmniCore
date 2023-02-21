using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Services.Entities;
using OmniCore.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xamarin.Forms;

namespace OmniCore.Services
{
    public class SyncClient
    {
        private IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _subChannel;
        private IModel _pubChannel;
        private string _exchange;
        private string _userId;
        private AsyncEventingBasicConsumer _consumer;
        
        private AsyncProducerConsumerQueue<ISyncableEntry> _publishQueue = new();
        private ConcurrentDictionary<ulong, ISyncableEntry> _awaitingConfirmation = new();
        private AsyncProducerConsumerQueue<ISyncableEntry> _publishConfirmedQueue = new();

        public async Task StartAsync(EndpointResponse epr)
        {
            _connectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(epr.Dsn)
            };
            _exchange = epr.Exchange;
            _userId = epr.UserId;
            _connection = _connectionFactory.CreateConnection();
            // _subChannel = _connection.CreateModel();
            // _subChannel.BasicQos(0, 30, true);
            // _consumer = new AsyncEventingBasicConsumer(_subChannel);
            // _consumer.Received += async (sender, ea) =>
            // {
            //     var ch = (IModel)sender;
            //
            //     Debug.WriteLine($"message received");
            //     ch.BasicAck(ea.DeliveryTag, false);
            //     await Task.Yield();
            // };
            // _subChannel.BasicConsume(epr.Queue, false, _consumer);
            
            _pubChannel = _connection.CreateModel();
            _pubChannel.ConfirmSelect();
            _pubChannel.BasicAcks += (sender, args) =>
            {
                if (args.Multiple)
                {
                    var confirmed = _awaitingConfirmation.Where
                        (k => k.Key <= args.DeliveryTag);
                    foreach (var kvp in confirmed)
                    {
                        _awaitingConfirmation.TryRemove(kvp.Key, out ISyncableEntry entry);
                        _publishConfirmedQueue.Enqueue(entry);
                        Debug.WriteLine($"{entry} marked as updated");
                    }
                }
                else
                {
                    _awaitingConfirmation.TryRemove(args.DeliveryTag, out ISyncableEntry entry);
                    _publishConfirmedQueue.Enqueue(entry);
                    Debug.WriteLine($"{entry} marked as updated");
                }
            };

            _pubChannel.BasicNacks += (sender, args) =>
            {
                if (args.Multiple)
                {
                    var unconfirmed = _awaitingConfirmation.Where
                        (k => k.Key <= args.DeliveryTag);
                    foreach (var kvp in unconfirmed)
                    {
                        _awaitingConfirmation.TryRemove(kvp.Key, out ISyncableEntry entry);
                        _publishQueue.Enqueue(entry);
                        Debug.WriteLine($"{entry} not synced, requeued");
                    }
                }
                else
                {
                    _awaitingConfirmation.TryRemove(args.DeliveryTag, out ISyncableEntry entry);
                    _publishQueue.Enqueue(entry);
                    Debug.WriteLine($"{entry} not synced, requeued");
                }
            };
            
            var _syncTask = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var tPub = _publishQueue.OutputAvailableAsync();
                        var tPubConf = _publishConfirmedQueue.OutputAvailableAsync();
                        var tTimeout = Task.Delay(10000);
                        var tRet = await Task.WhenAny(tPub, tPubConf, tTimeout);
                        if (tRet == tPub)
                        {
                            var entry = await _publishQueue.DequeueAsync();
                            // Debug.WriteLine("got message from queue");
                            var sequenceNumber = _pubChannel.NextPublishSeqNo;
                            IBasicProperties props = _pubChannel.CreateBasicProperties();
                            props.UserId = _userId;
                            // Debug.WriteLine($"publishing message");
                            _pubChannel.BasicPublish(_exchange, "*", false, props,
                                entry.AsMessageBody());
                            _awaitingConfirmation.TryAdd(sequenceNumber, entry);
                            Debug.WriteLine($"published message");
                        }
                        else if (tRet == tPubConf)
                        {
                            var entry = await _publishConfirmedQueue.DequeueAsync();
                            // Debug.WriteLine("got confirmed message from queue");
                            await entry.SetSyncedTask;
                            Debug.WriteLine($"entry updated as synced");
                        }
                        else
                        {
                            try
                            {
                                if (!_awaitingConfirmation.IsEmpty)
                                {
                                    Debug.WriteLine($"requesting pub confirmations");
                                    _pubChannel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(15));
                                    Debug.WriteLine($"pubs confirmed");
                                    foreach (var e in _awaitingConfirmation.Values)
                                        await _publishConfirmedQueue.EnqueueAsync(e);
                                    _awaitingConfirmation.Clear();
                                }
                            }
                            catch (Exception exception)
                            {
                                Debug.WriteLine($"pub confirm failed {exception}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"unknown exception {e}");
                    }
                }
            });
        }
        private async void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            Debug.WriteLine("consumer on received");
        }

        public async Task EnqueueAsync(BgcEntry reading)
        {
            await _publishQueue.EnqueueAsync(reading);
        }
    }
}