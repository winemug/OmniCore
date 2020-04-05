using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities.Extensions;
using OmniCore.Radios.RileyLink.Enumerations;
using OmniCore.Radios.RileyLink.Protocol;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadio : IErosRadio
    {
        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreLoggingFunctions Logging;
        private readonly ICoreRepositoryService RepositoryService;


        private IDisposable ConnectedSubscription;
        private IDisposable ConnectionFailedSubscription;
        private IDisposable DisconnectedSubscription;
        private IDisposable PeripheralLocateSubscription;
        private IDisposable PeripheralRssiSubscription;
        private IDisposable PeripheralStateSubscription;

        public RileyLinkRadio(
            ICoreContainer<IServerResolvable> container,
            ICoreLoggingFunctions logging,
            ICoreRepositoryService repositoryService)
        {
            RepositoryService = repositoryService;
            Container = container;
            Logging = logging;
        }

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

        public void StartMonitoring()
        {
            Logging.Debug($"RLR: {Address} Start monitoring");

            PeripheralRssiSubscription?.Dispose();

            if (Entity.Options.RssiUpdateInterval.HasValue)
                PeripheralRssiSubscription = Observable.Interval(Entity.Options.RssiUpdateInterval.Value)
                    .Subscribe(async _ =>
                    {
                        try
                        {
                            Logging.Debug($"RLR: {Address} Rssi request");
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
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

            ConnectedSubscription?.Dispose();
            ConnectedSubscription = Peripheral.WhenConnectionStateChanged()
                .FirstAsync(s => s == PeripheralConnectionState.Connected)
                .Subscribe(async _ =>
                {
                    Logging.Debug($"RLR: {Address} Connected");
                    await RecordRadioEvent(RadioEvent.Connect, CancellationToken.None);
                });

            DisconnectedSubscription?.Dispose();
            DisconnectedSubscription = Peripheral.WhenConnectionStateChanged()
                .FirstAsync(s => s == PeripheralConnectionState.Disconnected)
                .Subscribe(async _ =>
                {
                    Logging.Debug($"RLR: {Address} Disconnected");
                    await RecordRadioEvent(RadioEvent.Disconnect, CancellationToken.None);
                });

            PeripheralLocateSubscription?.Dispose();
            PeripheralLocateSubscription = null;

            PeripheralStateSubscription?.Dispose();
            PeripheralStateSubscription = Peripheral.WhenDiscoveryStateChanged()
                .Subscribe(async state =>
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
            var options = Entity.Options;
            //options.KeepConnected = false;
            //options.AutoConnect = false;

            using var rlConnection = await GetRileyLinkHandler(options, cancellationToken);

            await rlConnection.Led(RileyLinkLed.Blue, RileyLinkLedMode.On);
            await rlConnection.Led(RileyLinkLed.Blue, RileyLinkLedMode.Off);
        }

        public async Task<byte[]> GetResponse(IErosPodRequest request, CancellationToken cancellationToken,
            RadioOptions options = null)
        {
            using var rlConnection = await GetRileyLinkHandler(options, cancellationToken);

            if (options == null)
                options = Entity.Options;

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

        public async Task<RileyLinkConnectionHandler> GetRileyLinkHandler(
            RadioOptions options,
            CancellationToken cancellationToken)
        {
            IBlePeripheralConnection peripheralConnection = null;

            using var connectionTimeoutOverall = new CancellationTokenSource(options.RadioConnectionOverallTimeout);
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                connectionTimeoutOverall.Token);
            try
            {
                Logging.Debug($"RLR: {Address} Opening connection");
                peripheralConnection = await Peripheral.GetConnection(options.AutoConnect, options.KeepConnected,
                    options.RadioDiscoveryTimeout, options.RadioConnectTimeout,
                    options.RadioCharacteristicsDiscoveryTimeout,
                    linkedCancellation.Token);

                return new RileyLinkConnectionHandler(Logging, Peripheral, peripheralConnection, options);
            }
            catch (Exception e)
            {
                peripheralConnection?.Dispose();
                Logging.Debug($"RLR: {Address} Error while connecting:\n {e.AsDebugFriendly()}");
                throw new OmniCoreRadioException(FailureType.RadioGeneralError, inner: e);
            }
        }

        private async Task RecordRadioEvent(RadioEvent eventType, CancellationToken cancellationToken,
            string text = null, byte[] data = null, int? rssi = null)
        {
            Logging.Debug($"RLR: {Address} Recording radio event {eventType}");

            using var context = await RepositoryService.GetContextReadWrite(cancellationToken);

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