using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities.Extensions;
using OmniCore.Radios.RileyLink.Enumerations;
using OmniCore.Radios.RileyLink.Protocol;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadio : IErosRadio
    {
        private readonly IContainer Container;
        private readonly ILogger Logger;
        private readonly IRepositoryService RepositoryService;
        private readonly IConfigurationService ConfigurationService;

        private IDisposable HealthCheckSubscription;
        private CancellationTokenSource HealthCheckCancellationTokenSource;

        public RileyLinkRadio(
            IContainer container,
            ILogger logger,
            IRepositoryService repositoryService,
            IConfigurationService configurationService)
        {
            ConfigurationService = configurationService;
            RepositoryService = repositoryService;
            Container = container;
            Logger = logger;
        }

        public void Initialize(RadioEntity entity, IErosRadioProvider provider, IBlePeripheral peripheral)
        {
            Entity = entity;
            Provider = provider;
            Peripheral = peripheral;

            //TODO: auto disposables

            Peripheral.WhenConnectionStateChanged()
                .Subscribe(async connectionState =>
                {
                    switch (connectionState)
                    {
                        case PeripheralConnectionState.Connected:
                            Logger.Debug($"RLR: {Address} Connected");
                            await RecordRadioEvent(RadioEvent.Connect);
                            break;
                        case PeripheralConnectionState.Disconnected:
                            Logger.Debug($"RLR: {Address} Disconnected");
                            await RecordRadioEvent(RadioEvent.Disconnect);
                            break;
                    }
                });

            Peripheral.WhenDiscoveryStateChanged()
                .Subscribe(async discoveryState =>
                {
                    switch (discoveryState)
                    {
                        case PeripheralDiscoveryState.Discovered:
                            await RecordRadioEvent(RadioEvent.Online);
                            break;
                    }
                });

            Peripheral.WhenRssiReceived()
                .Subscribe(async rssi => { await RecordRadioEvent(RadioEvent.RssiReceived, rssi: rssi); });

            Peripheral.WhenNameUpdated()
                .Subscribe(async name => { await UpdateRadioName(name); });

        }

        public IBlePeripheral Peripheral { get; private set; }
        public IErosRadioProvider Provider { get; private set; }
        public RadioEntity Entity { get; private set; }

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
            ResumeHealthChecks(TimeSpan.FromSeconds(5));
        }

        private async Task PauseHealthChecks()
        {
            lock (this)
            {
                HealthCheckSubscription?.Dispose();
                HealthCheckSubscription = null;

                if (HealthCheckCancellationTokenSource != null)
                {
                    HealthCheckCancellationTokenSource.Cancel();
                    HealthCheckCancellationTokenSource.Dispose();
                    HealthCheckCancellationTokenSource = null;
                }
            }
        }

        private void ResumeHealthChecks(bool wasInGoodHealth)
        {
            ResumeHealthChecks(wasInGoodHealth
                ? Entity.Options.RadioHealthCheckIntervalGood
                : Entity.Options.RadioHealthCheckIntervalBad);
        }

        private void ResumeHealthChecks(TimeSpan interval)
        {
            lock (this)
            {
                HealthCheckCancellationTokenSource?.Dispose();
                HealthCheckCancellationTokenSource = new CancellationTokenSource();

                HealthCheckSubscription?.Dispose();
                HealthCheckSubscription = Scheduler.Default.Schedule(
                    interval,
                    async () =>
                    {
                        var nextInterval = Entity.Options.RadioHealthCheckIntervalGood;
                        try
                        {
                            Logger.Debug($"RLR: {Address} Starting healthcheck");
                            nextInterval = await PerformHealthChecks(HealthCheckCancellationTokenSource.Token);
                            Logger.Debug($"RLR: {Address} Healthcheck finished");
                        }
                        catch (OperationCanceledException)
                        {
                            Logger.Debug($"RLR: {Address} Healthcheck canceled");
                        }
                        catch (Exception e)
                        {
                            Logger.Warning($"RLR: {Address} Healthcheck failed", e);
                            nextInterval = Entity.Options.RadioHealthCheckIntervalGood;
                        }

                        ResumeHealthChecks(nextInterval);
                    });
            }
        }

        private async Task<TimeSpan> PerformHealthChecks(CancellationToken cancellationToken)
        {
            var jitter = TimeSpan.FromSeconds(new Random().NextDouble() * 5).Add(TimeSpan.FromSeconds(5));
            var peripheralOptions = await ConfigurationService.GetBlePeripheralOptions(CancellationToken.None);
            var discoveryAge = DateTimeOffset.UtcNow - Peripheral.DiscoveryState.Date;
            var discoveryWindow = peripheralOptions.PeripheralDiscoveryCooldown +
                                   peripheralOptions.PeripheralDiscoveryTimeout;
            if (Peripheral.DiscoveryState.State == PeripheralDiscoveryState.NotFound &&
                discoveryAge < discoveryWindow)
            {
                Logger.Debug("RLR: {Address} Healthcheck postponed due discovery cooldown");
                return discoveryWindow - discoveryAge + jitter;
            }

            if (Peripheral.DiscoveryState.State == PeripheralDiscoveryState.Searching)
            {
                Logger.Debug("RLR: {Address} Healthcheck postponed due existing discovery");
                return peripheralOptions.PeripheralDiscoveryTimeout - discoveryAge + jitter;
            }

            if (Peripheral.DiscoveryState.State != PeripheralDiscoveryState.Discovered)
            {
                Logger.Debug("RLR: {Address} Healthcheck running discovery");
                await Peripheral.Discover(cancellationToken);
                Logger.Debug("RLR: {Address} Healthcheck found peripheral");
            }

            var connectionStateAge = DateTimeOffset.UtcNow - Peripheral.ConnectionState.Date;
            if (Peripheral.ConnectionState.State == PeripheralConnectionState.Connecting)
            {
                Logger.Debug("RLR: {Address} Healthcheck postponed b/c peripheral is connecting");
                return peripheralOptions.PeripheralConnectTimeout +  peripheralOptions.CharacteristicsDiscoveryTimeout
                                                                  + jitter - connectionStateAge;
            }

            if (Peripheral.ConnectionState.State == PeripheralConnectionState.Disconnecting)
            {
                Logger.Debug("RLR: {Address} Healthcheck postponed b/c peripheral is disconnecting");
                return peripheralOptions.PeripheralConnectTimeout + jitter - connectionStateAge;
            }

            var rssiRead = false;
            if (Peripheral.ConnectionState.State == PeripheralConnectionState.Connected)
            {
                Logger.Debug($"RLR: {Address} Requesting RSSI on already connected peripheral");
                var rssi = await Peripheral.ReadRssi(cancellationToken);
                Logger.Debug($"RLR: {Address} Healthcheck RSSI received: {rssi}");
                rssiRead = true;
            }
            else if (Peripheral.Rssi.HasValue)
            {
                Logger.Debug($"RLR: {{Address}} Last reported RSSI {Peripheral.Rssi.Value.Rssi} at {Peripheral.Rssi.Value.Date.ToLocalTime()}");
                if (DateTimeOffset.UtcNow - Peripheral.Rssi.Value.Date > TimeSpan.FromMinutes(1))
                {
                    Logger.Debug($"RLR: {{Address}} RSSI is older than 1 minute, will request upon connection");
                }
                else
                {
                    rssiRead = true;
                }
            }

            using var rlConnection = await GetRileyLinkHandler(Entity.Options, cancellationToken);

            if (rssiRead)
            {
                Logger.Debug($"RLR: {Address} Requesting RSSI");
                var rssi = await Peripheral.ReadRssi(cancellationToken);
                Logger.Debug($"RLR: {Address} Healthcheck RSSI received: {rssi}");
            }

            Logger.Debug("RLR: {Address} Healthcheck RL write test");
            for(int i=0; i<4; i++)
                await rlConnection.Noop().ToTask(cancellationToken);
            
            Logger.Debug("RLR: {Address} Healthcheck RL state request");
            var state = await rlConnection.GetState().ToTask(cancellationToken);
            if (!state.StateOk)
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse);

            Logger.Debug("RLR: {Address} Healthcheck RL state OK");

            return Entity.Options.RadioHealthCheckIntervalGood;
        }

        public async Task Identify(CancellationToken cancellationToken)
        {
            await PauseHealthChecks();

            try
            {
                Logger.Debug($"RLR: {Address} Identifying device");
                var options = Entity.Options;
                //options.KeepConnected = false;
                //options.AutoConnect = false;

                using var rlConnection = await GetRileyLinkHandler(options, cancellationToken);

                await rlConnection.Led(RileyLinkLed.Blue, RileyLinkLedMode.On);
                await rlConnection.Led(RileyLinkLed.Blue, RileyLinkLedMode.Off);
                ResumeHealthChecks(true);
            }
            catch (Exception e)
            {
                ResumeHealthChecks(false);
                throw;
            }
        }

        public async Task<byte[]> GetResponse(IPodRequest request, CancellationToken cancellationToken,
            RadioOptions options = null)
        {
            await PauseHealthChecks();
            try
            {
                if (options == null)
                    options = Entity.Options;

                using var rlConnection = await GetRileyLinkHandler(options, cancellationToken);

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
                ResumeHealthChecks(true);

                //return response;
            }
            catch (Exception e)
            {
                ResumeHealthChecks(false);
                throw;
            }
            return null;
        }


        public void Dispose()
        {
        }

        private async Task<RileyLinkConnectionHandler> GetRileyLinkHandler(
            RadioOptions radioOptions,
            CancellationToken cancellationToken)
        {
            IBlePeripheralConnection peripheralConnection = null;

            var peripheralOptions = await ConfigurationService.GetBlePeripheralOptions(CancellationToken.None);
            
            using var connectionTimeoutOverall = new CancellationTokenSource(peripheralOptions.PeripheralConnectTimeout
                + peripheralOptions.CharacteristicsDiscoveryTimeout + peripheralOptions.PeripheralDiscoveryTimeout);
            
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                connectionTimeoutOverall.Token);
            try
            {
                Logger.Debug($"RLR: {Address} Opening connection");
                peripheralConnection = await Peripheral.GetConnection(peripheralOptions,
                    linkedCancellation.Token);

                return new RileyLinkConnectionHandler(Logger, Peripheral, ConfigurationService,
                    peripheralConnection);
            }
            catch (Exception e)
            {
                peripheralConnection?.Dispose();
                Logger.Debug($"RLR: {Address} Error while connecting:\n {e.AsDebugFriendly()}");
                throw new OmniCoreRadioException(FailureType.RadioGeneralError, inner: e);
            }
        }

        private async Task RecordRadioEvent(RadioEvent eventType,
            string text = null, byte[] data = null, int? rssi = null)
        {
             using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
             try
             {
                 using var context = await RepositoryService.GetContextReadWrite(cts.Token);
        
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
                         }, cts.Token);
                 await context.Save(cts.Token);
             }
             catch (OperationCanceledException)
             {
                 Logger.Warning("RLR: Timed out writing radio event to database");
             }
             catch (Exception e)
             {
                 Logger.Warning("RLR: Error writing radio event to database", e);
             }
        
        }

        private async Task UpdateRadioName(string name)
        {
            if (Entity.DeviceName == name)
                return;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                using var context = await RepositoryService.GetContextReadWrite(cts.Token);
                context.WithExisting(Entity);
                Entity.DeviceName = name;
                await context.Save(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Logger.Warning("RLR: Timed out while updating radio name in database");
            }
            catch (Exception e)
            {
                Logger.Warning("RLR: Error writing to database", e);
            }
        }
    }
}