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
        private readonly ICoreConfigurationService ConfigurationService;

        private IDisposable HealthCheckSubscription;
        private CancellationTokenSource HealthCheckCancellationTokenSource;

        public RileyLinkRadio(
            ICoreContainer<IServerResolvable> container,
            ICoreLoggingFunctions logging,
            ICoreRepositoryService repositoryService,
            ICoreConfigurationService configurationService)
        {
            ConfigurationService = configurationService;
            RepositoryService = repositoryService;
            Container = container;
            Logging = logging;
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
                            Logging.Debug($"RLR: {Address} Connected");
                            await RecordRadioEvent(RadioEvent.Connect);
                            break;
                        case PeripheralConnectionState.Disconnected:
                            Logging.Debug($"RLR: {Address} Disconnected");
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

            ResumeHealthChecks(true);
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
                            Logging.Debug($"RLR: {Address} Starting healthcheck canceled");
                            nextInterval = await PerformHealthChecks(HealthCheckCancellationTokenSource.Token);
                            Logging.Debug($"RLR: {Address} Healthcheck finished");
                        }
                        catch (OperationCanceledException)
                        {
                            Logging.Debug($"RLR: {Address} Healthcheck canceled");
                        }
                        catch (Exception e)
                        {
                            Logging.Warning($"RLR: {Address} Healthcheck failed", e);
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
                Logging.Debug("RLR: {Address} Healthcheck postponed due discovery cooldown");
                return discoveryWindow - discoveryAge + jitter;
            }

            if (Peripheral.DiscoveryState.State == PeripheralDiscoveryState.Searching)
            {
                Logging.Debug("RLR: {Address} Healthcheck postponed due existing discovery");
                return peripheralOptions.PeripheralDiscoveryTimeout - discoveryAge + jitter;
            }

            if (Peripheral.DiscoveryState.State != PeripheralDiscoveryState.Discovered)
            {
                Logging.Debug("RLR: {Address} Healthcheck running discovery");
                await Peripheral.Discover(cancellationToken);
            }

            var connectionStateAge = DateTimeOffset.UtcNow - Peripheral.ConnectionState.Date;
            if (Peripheral.ConnectionState.State == PeripheralConnectionState.Connecting)
            {
                Logging.Debug("RLR: {Address} Healthcheck postponed b/c peripheral is connecting");
                return peripheralOptions.PeripheralConnectTimeout +  peripheralOptions.CharacteristicsDiscoveryTimeout
                                                                  + jitter - connectionStateAge;
            }

            if (Peripheral.ConnectionState.State == PeripheralConnectionState.Disconnecting)
            {
                Logging.Debug("RLR: {Address} Healthcheck postponed b/c peripheral is disconnecting");
                return peripheralOptions.PeripheralConnectTimeout + jitter - connectionStateAge;
            }

            var rssi = await Peripheral.ReadRssi(cancellationToken);
            Logging.Debug($"RLR: {Address} Healthcheck RSSI: {rssi}");

            using var rlConnection = await GetRileyLinkHandler(Entity.Options, cancellationToken);
            Logging.Debug("RLR: {Address} Healthcheck RL write test");
            await rlConnection.Noop().ToTask(cancellationToken);
            
            Logging.Debug("RLR: {Address} Healthcheck RL state request");
            var state = await rlConnection.GetState().ToTask(cancellationToken);
            if (!state.StateOk)
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse);
            Logging.Debug("RLR: {Address} Healthcheck RL state OK");

            return Entity.Options.RadioHealthCheckIntervalGood;
        }

        public async Task Identify(CancellationToken cancellationToken)
        {
            await PauseHealthChecks();

            try
            {
                Logging.Debug($"RLR: {Address} Identifying device");
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

        public async Task<byte[]> GetResponse(IErosPodRequest request, CancellationToken cancellationToken,
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
                Logging.Debug($"RLR: {Address} Opening connection");
                peripheralConnection = await Peripheral.GetConnection(peripheralOptions,
                    linkedCancellation.Token);

                return new RileyLinkConnectionHandler(Logging, Peripheral, ConfigurationService,
                    peripheralConnection);
            }
            catch (Exception e)
            {
                peripheralConnection?.Dispose();
                Logging.Debug($"RLR: {Address} Error while connecting:\n {e.AsDebugFriendly()}");
                throw new OmniCoreRadioException(FailureType.RadioGeneralError, inner: e);
            }
        }

        private async Task RecordRadioEvent(RadioEvent eventType,
            string text = null, byte[] data = null, int? rssi = null)
        {
            //TODO: don't write to db just yet

            //     using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            //     try
            //     {
            //         using var context = await RepositoryService.GetContextReadWrite(cts.Token);
            //
            //         await context
            //             .WithExisting(Entity)
            //             .RadioEvents.AddAsync(
            //                 new RadioEventEntity
            //                 {
            //                     Radio = Entity,
            //                     EventType = eventType,
            //                     Text = text,
            //                     Data = data,
            //                     Rssi = rssi
            //                 }, cts.Token);
            //         await context.Save(cts.Token);
            //     }
            //     catch (OperationCanceledException)
            //     {
            //         Logging.Warning("RLR: Timed out writing radio event to database");
            //     }
            //     catch (Exception e)
            //     {
            //         Logging.Warning("RLR: Error writing radio event to database", e);
            //     }
            //
            // }
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
                Logging.Warning("RLR: Timed out while updating radio name in database");
            }
            catch (Exception e)
            {
                Logging.Warning("RLR: Error writing to database", e);
            }
        }
    }
}