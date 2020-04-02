using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Nito.AsyncEx;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;
using OmniCore.Model.Utilities.Extensions;
using OmniCore.Radios.RileyLink.Enumerations;
using OmniCore.Radios.RileyLink.Protocol;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadio : IErosRadio
    {
        public IBlePeripheral Peripheral { get; set; }
        public RadioEntity Entity { get; set; }

        public RadioType Type => RadioType.RileyLink;
        public string Address => Peripheral.PeripheralUuid.AsMacAddress();
        public string Description => Entity.UserDescription;
        public IObservable<string> Name => Peripheral.WhenNameUpdated();
        public IObservable<PeripheralDiscoveryState> DiscoveryState => Peripheral.WhenDiscoveryStateChanged();
        public IObservable<PeripheralConnectionState> ConnectionState => Peripheral.WhenConnectionStateChanged();
        public IObservable<int> Rssi => Peripheral.WhenRssiReceived();
        public RadioOptions Options => Entity.Options;


        private IDisposable ConnectedSubscription = null;
        private IDisposable ConnectionFailedSubscription = null;
        private IDisposable DisconnectedSubscription = null;
        private IDisposable PeripheralStateSubscription = null;
        private IDisposable PeripheralRssiSubscription = null;
        private IDisposable PeripheralLocateSubscription = null;


        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreLoggingFunctions Logging;
        private readonly ICoreRepositoryService RepositoryService;

        public RileyLinkRadio(
            ICoreContainer<IServerResolvable> container,
            ICoreLoggingFunctions logging,
            ICoreRepositoryService repositoryService)
        {
            RepositoryService = repositoryService;
            Container = container;
            Logging = logging;
        }

        public void StartMonitoring()
        {
            Logging.Debug($"RLR: {Address} Start monitoring");

            PeripheralRssiSubscription?.Dispose();

            if (Entity.Options.RssiUpdateInterval.HasValue)
            {
                PeripheralRssiSubscription = Observable.Interval(Entity.Options.RssiUpdateInterval.Value)
                    .Subscribe( async _ =>
                    {
                        try
                        {
                            Logging.Debug($"RLR: {Address} Rssi request");
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                            var rssi = await Peripheral.ReadRssi(cts.Token);
                            Logging.Debug($"RLR: {Address} Rssi received: {rssi}");
                            await RecordRadioEvent(RadioEvent.RssiReceived, CancellationToken.None, null,
                                null, rssi);
                        }
                        catch (OperationCanceledException)
                        {
                            Logging.Debug($"RLR: {Address} Rssi timed out!");
                        }
                    });
            }

            ConnectedSubscription?.Dispose();
            ConnectedSubscription = Peripheral.WhenConnectionStateChanged()
                .FirstAsync(s => s == PeripheralConnectionState.Connected)
                .Subscribe( async (_) =>
                {
                    Logging.Debug($"RLR: {Address} Connected");
                    await RecordRadioEvent(RadioEvent.Connect, CancellationToken.None);
                });

            DisconnectedSubscription?.Dispose();
            DisconnectedSubscription = Peripheral.WhenConnectionStateChanged()
                .FirstAsync(s => s == PeripheralConnectionState.Disconnected)
                .Subscribe( async (_) =>
                {
                    Logging.Debug($"RLR: {Address} Disconnected");
                    await RecordRadioEvent(RadioEvent.Disconnect, CancellationToken.None);
                });

            PeripheralLocateSubscription?.Dispose();
            PeripheralLocateSubscription = null;
            
            PeripheralStateSubscription?.Dispose();
            PeripheralStateSubscription = Peripheral.WhenDiscoveryStateChanged()
                .Subscribe(async (state) =>
            {
                switch (state)
                {
                    case PeripheralDiscoveryState.Unknown:
                        Logging.Debug($"RLR: {Address} Peripheral state unknown");
                        var cts = new CancellationTokenSource(Entity.Options.RadioDiscoveryTimeout);
                        await Peripheral.Discover(cts.Token);
                        break;
                    case PeripheralDiscoveryState.NotFound:
                        Logging.Debug($"RLR: {Address} Peripheral not found in last scan");
                        await RecordRadioEvent(RadioEvent.Offline, CancellationToken.None);
                        PeripheralLocateSubscription = Observable.Interval(Entity.Options.RadioDiscoveryCooldown)
                            .Subscribe(async _ =>
                            {
                                var cts = new CancellationTokenSource(Entity.Options.RadioDiscoveryTimeout);
                                await Peripheral.Discover(cts.Token);
                            });
                        break;
                    case PeripheralDiscoveryState.Searching:
                        Logging.Debug($"RLR: {Address} Looking for peripheral");
                        break;
                    case PeripheralDiscoveryState.Discovered:
                        if (PeripheralLocateSubscription != null)
                        {
                            PeripheralLocateSubscription.Dispose();
                            PeripheralLocateSubscription = null;
                        }
                        Logging.Debug($"RLR: {Address} Peripheral is online");
                        await RecordRadioEvent(RadioEvent.Online, CancellationToken.None);
                        
                        //if (Entity.Options.KeepConnected)
                        //{
                        //    Logging.Debug($"RLR: {Address} Connecting to peripheral due to KeepConnected setting");
                        //    try
                        //    {
                        //    }
                        //    catch (OperationCanceledException e)
                        //    {
                        //    }
                        //}
                        break;
                }
            });
        }

        public async Task Identify(CancellationToken cancellationToken)
        {
            Logging.Debug($"RLR: {Address} Identifying device");
            using var rlConnection = await RileyLinkConnectionHandler.Connect(Peripheral,
                Entity.Options, cancellationToken);

            for (int i = 0; i < 3; i++)
            {
                await rlConnection.Led(RileyLinkLed.Blue, RileyLinkLedMode.On);
                await rlConnection.Led(RileyLinkLed.Blue, RileyLinkLedMode.Off);
                await rlConnection.Led(RileyLinkLed.Green, RileyLinkLedMode.On);
                await rlConnection.Led(RileyLinkLed.Green, RileyLinkLedMode.Off);
            }
        }



        public async Task<byte[]> GetResponse(IErosPodRequest request, CancellationToken cancellationToken,
            RadioOptions options = null)
        {
            using var rlConnection = await RileyLinkConnectionHandler.Connect(Peripheral, options, cancellationToken);

            if (options == null)
                options = Entity.Options;

            await rlConnection.Configure(options, cancellationToken);

            //Logging.Debug($"RLR: {Address} Starting conversation with pod id: {request.ErosPod.Entity.Id}");
            //var conversation = RileyLinkErosConversation.ForPod(request.ErosPod);
            //conversation.Initialize(request);

            //while (!conversation.IsFinished)
            //{
            //    var sendPacketData = conversation.GetPacketToSend();

            //    //TODO

            //    var rssi = GetRssi(result.Data[0]);
            //    conversation.ParseIncomingPacket(result.Data[1..]);
            //}

            //var response = conversation.ResponseData.ToArray();
            //Logging.Debug($"RLR: {Address} Conversation ended.");
            //return response;
            return null;
        }


        public void Dispose()
        {
            PeripheralRssiSubscription?.Dispose();
            PeripheralRssiSubscription = null;

            PeripheralStateSubscription?.Dispose();
            PeripheralStateSubscription = null;

            ConnectedSubscription?.Dispose();
            ConnectedSubscription = null;

            ConnectionFailedSubscription?.Dispose();
            ConnectionFailedSubscription = null;

            DisconnectedSubscription?.Dispose();
            DisconnectedSubscription = null;
        }

        private async Task RecordRadioEvent(RadioEvent eventType, CancellationToken cancellationToken,
            string text = null, byte[] data = null, int? rssi = null)
        {
            Logging.Debug($"RLR: {Address} Recording radio event {eventType}");

            using var context = await RepositoryService.GetWriterContext(cancellationToken);
            
            await context
                .WithExisting(Entity)
                .RadioEvents.AddAsync(
                    new RadioEventEntity
                    {
                        Radio = Entity,
                        EventType = eventType,
                        Text = text,
                        Data = data,
                        Rssi = rssi
                    }, cancellationToken);
            await context.Save(cancellationToken);
        }
    }
}
