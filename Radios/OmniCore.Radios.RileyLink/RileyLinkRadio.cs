using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Model.Constants;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities.Extensions;
using OmniCore.Radios.RileyLink.Enumerations;
using OmniCore.Radios.RileyLink.Protocol;
using AsyncLock = Nito.AsyncEx.AsyncLock;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadio : IErosRadio, ICompositeDisposableProvider
    {
        public CompositeDisposable CompositeDisposable { get; } = new CompositeDisposable();
        public RadioEntity Entity { get; private set; }
        public RadioType Type => RadioType.RileyLink;
        public string Address => Entity.DeviceUuid.AsMacAddress();
        public IObservable<string> Name => Peripheral.WhenNameUpdated();
        public IObservable<PeripheralDiscoveryState> DiscoveryState => Peripheral.WhenDiscoveryStateChanged();
        public IObservable<PeripheralConnectionState> ConnectionState => Peripheral.WhenConnectionStateChanged();
        public IObservable<int> Rssi => Peripheral.WhenRssiReceived();
        public RadioOptions DefaultOptions => Entity.Options;
        
        private readonly IContainer Container;
        private readonly ILogger Logger;
        private readonly IRepositoryService RepositoryService;
        private readonly IConfigurationService ConfigurationService;
        private readonly IBlePeripheralAdapter PeripheralAdapter;
        private readonly RileyLinkConnection RileyLinkConnection;
        private readonly AsyncLock RileyLinkConnectionLock;

        private IBlePeripheral Peripheral;
        
        public RileyLinkRadio(
            IContainer container,
            ILogger logger,
            IRepositoryService repositoryService,
            IConfigurationService configurationService,
            IBlePeripheralAdapter peripheralAdapter,
            RileyLinkConnection rileyLinkConnection)
        {
            Container = container;
            Logger = logger;
            RepositoryService = repositoryService;
            ConfigurationService = configurationService;
            PeripheralAdapter = peripheralAdapter;
            RileyLinkConnection = rileyLinkConnection;
            RileyLinkConnectionLock = new AsyncLock();
        }

        public async Task Initialize(IBlePeripheral peripheral, CancellationToken cancellationToken)
        {
            Peripheral = peripheral;

            using (var context = await RepositoryService.GetContextReadOnly(cancellationToken))
            {
                Entity = await context.Radios.FirstOrDefaultAsync(
                    r => !r.IsDeleted &&
                         r.DeviceUuid == Peripheral.PeripheralUuid &&
                         r.ServiceUuid == Uuids.RileyLinkServiceUuid, cancellationToken);
            }

            if (Entity == null)
            {
                using var context = await RepositoryService.GetContextReadWrite(cancellationToken);
                Entity = new RadioEntity
                {
                    DeviceName = Peripheral.Name,
                    DeviceUuid = Peripheral.PeripheralUuid,
                    ServiceUuid = Peripheral.PrimaryServiceUuid
                };
                await context.Radios.AddAsync(Entity, cancellationToken);
                await context.Save(cancellationToken);
            }
            else
            {
                Peripheral.Name = Entity.DeviceName;
            }

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
                }).DisposeWith(this);

            Peripheral.WhenDiscoveryStateChanged()
                .Subscribe(async discoveryState =>
                {
                    switch (discoveryState)
                    {
                        case PeripheralDiscoveryState.Discovered:
                            await RecordRadioEvent(RadioEvent.Online);
                            break;
                    }
                }).DisposeWith(this);

            Peripheral.WhenRssiReceived()
                .Subscribe(async rssi => { await RecordRadioEvent(RadioEvent.RssiReceived, rssi: rssi); })
                .DisposeWith(this);

            Peripheral.WhenNameUpdated()
                .Subscribe(async name => { await UpdateEntityName(name); })
                .DisposeWith(this);
        }
        public async Task Initialize(Guid uuid, CancellationToken cancellationToken)
        {
            var peripheral = await PeripheralAdapter.GetPeripheral(uuid, Uuids.RileyLinkServiceUuid);
            await Initialize(peripheral, cancellationToken);
        }

        public async Task SetDefaultOptions(RadioOptions options, CancellationToken cancellationToken)
        {
            using var context = await RepositoryService.GetContextReadWrite(cancellationToken);
            context.WithExisting(Entity);
            Entity.Options = options;
            await context.Save(cancellationToken);
        }

        public async Task SetName(string newName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task PerformHealthCheck(CancellationToken cancellationToken)
        {
            // var jitter = TimeSpan.FromSeconds(new Random().NextDouble() * 5).Add(TimeSpan.FromSeconds(5));
            // var peripheralOptions = await ConfigurationService.GetBlePeripheralOptions(cancellationToken);
            // var discoveryAge = DateTimeOffset.UtcNow - Peripheral.DiscoveryState.Date;
            // var discoveryWindow = peripheralOptions.PeripheralDiscoveryCooldown +
            //                        peripheralOptions.PeripheralDiscoveryTimeout;
            //
            // if (Peripheral.DiscoveryState.State == PeripheralDiscoveryState.NotFound &&
            //     discoveryAge < discoveryWindow)
            // {
            //     Logger.Debug("RLR: {Address} Healthcheck postponed due discovery cooldown");
            //     return discoveryWindow - discoveryAge + jitter;
            // }
            //
            // if (Peripheral.DiscoveryState.State == PeripheralDiscoveryState.Searching)
            // {
            //     Logger.Debug("RLR: {Address} Healthcheck postponed due existing discovery");
            //     return peripheralOptions.PeripheralDiscoveryTimeout - discoveryAge + jitter;
            // }
            //
            // if (Peripheral.DiscoveryState.State != PeripheralDiscoveryState.Discovered)
            // {
            //     Logger.Debug("RLR: {Address} Healthcheck running discovery");
            //     await Peripheral.Discover(cancellationToken);
            //     Logger.Debug("RLR: {Address} Healthcheck found peripheral");
            // }
            //
            // var connectionStateAge = DateTimeOffset.UtcNow - Peripheral.ConnectionState.Date;
            // if (Peripheral.ConnectionState.State == PeripheralConnectionState.Connecting)
            // {
            //     Logger.Debug("RLR: {Address} Healthcheck postponed b/c peripheral is connecting");
            //     return peripheralOptions.PeripheralConnectTimeout +  peripheralOptions.CharacteristicsDiscoveryTimeout
            //                                                       + jitter - connectionStateAge;
            // }
            //
            // if (Peripheral.ConnectionState.State == PeripheralConnectionState.Disconnecting)
            // {
            //     Logger.Debug("RLR: {Address} Healthcheck postponed b/c peripheral is disconnecting");
            //     return peripheralOptions.PeripheralConnectTimeout + jitter - connectionStateAge;
            // }
            //
            // var rssiRead = false;
            // if (Peripheral.ConnectionState.State == PeripheralConnectionState.Connected)
            // {
            //     Logger.Debug($"RLR: {Address} Requesting RSSI on already connected peripheral");
            //     var rssi = await Peripheral.ReadRssi(cancellationToken);
            //     Logger.Debug($"RLR: {Address} Healthcheck RSSI received: {rssi}");
            //     rssiRead = true;
            // }
            // else if (Peripheral.Rssi.HasValue)
            // {
            //     Logger.Debug($"RLR: {{Address}} Last reported RSSI {Peripheral.Rssi.Value.Rssi} at {Peripheral.Rssi.Value.Date.ToLocalTime()}");
            //     if (DateTimeOffset.UtcNow - Peripheral.Rssi.Value.Date > TimeSpan.FromMinutes(1))
            //     {
            //         Logger.Debug($"RLR: {{Address}} RSSI is older than 1 minute, will request upon connection");
            //     }
            //     else
            //     {
            //         rssiRead = true;
            //     }
            // }
            //
            // using var rlConnection = await GetRileyLinkHandler(Entity.Options, cancellationToken);
            //
            // if (rssiRead)
            // {
            //     Logger.Debug($"RLR: {Address} Requesting RSSI");
            //     var rssi = await Peripheral.ReadRssi(cancellationToken);
            //     Logger.Debug($"RLR: {Address} Healthcheck RSSI received: {rssi}");
            // }
            //
            // Logger.Debug($"RLR: {Address} Healthcheck RL write test");
            // for (int i = 0; i < 8; i++)
            //     await rlConnection.Noop(cancellationToken);
            //
            // Logger.Debug($"RLR: {Address} Healthcheck RL state request");
            // var state = await rlConnection.GetState(cancellationToken);
            // if (!state.StateOk)
            //     throw new OmniCoreRadioException(FailureType.RadioErrorResponse);
            //
            // Logger.Debug($"RLR: {Address} Healthcheck RL state OK");
            //
            // Logger.Debug($"RLR: Signaling end of health-check");
            // await rlConnection.Led(RileyLinkLed.Blue, RileyLinkLedMode.On, cancellationToken);
            // await rlConnection.Led(RileyLinkLed.Blue, RileyLinkLedMode.Off, cancellationToken);
            // await rlConnection.Led(RileyLinkLed.Blue, RileyLinkLedMode.On, cancellationToken);
            // await rlConnection.Led(RileyLinkLed.Blue, RileyLinkLedMode.Off, cancellationToken);
            // Logger.Debug($"RLR: Santa is going home");
            //
            // return Entity.Options.RadioHealthCheckIntervalGood;
        }

        public Task<IRadioConnection> GetConnection(CancellationToken cancellationToken)
        {
            return GetConnection(DefaultOptions, cancellationToken);
        }

        public async Task<IRadioConnection> GetConnection(RadioOptions options, CancellationToken cancellationToken)
        {
            var connectionLock = await RileyLinkConnectionLock.LockAsync(cancellationToken);
            try
            {
                await RileyLinkConnection.Initialize(Peripheral, cancellationToken);
                await RileyLinkConnection.Configure(options, cancellationToken);
                connectionLock.DisposeWith(RileyLinkConnection);
                return RileyLinkConnection;
            }
            catch (Exception)
            {
                connectionLock.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }

    //     private async Task<RileyLinkConnectionHandler> GetRileyLinkHandler(
    //         RadioOptions radioOptions,
    //         CancellationToken cancellationToken)
    //     {
    //         
    //         var handler = await Container.Get<RileyLinkConnectionHandler>();
    //         await handler.Configure(radioOptions, cancellationToken);
    //         return handler;
    //     }
    //
    //     IBlePeripheralConnection peripheralConnection = null;
    //
    //         using var connectionTimeoutOverall = new CancellationTokenSource(peripheralOptions.PeripheralConnectTimeout
    //             + peripheralOptions.CharacteristicsDiscoveryTimeout + peripheralOptions.PeripheralDiscoveryTimeout);
    //         
    //         using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
    //             connectionTimeoutOverall.Token);
    //         try
    //         {
    //             Logger.Debug($"RLR: {Address} Opening connection");
    //             peripheralConnection = await Peripheral.GetConnection(peripheralOptions,
    //                 linkedCancellation.Token);
    //
    //             return new RileyLinkConnectionHandler(Logger, Peripheral, ConfigurationService,
    //                 peripheralConnection);
    //         }
    //         catch (Exception e)
    //         {
    //             peripheralConnection?.Dispose();
    //             Logger.Debug($"RLR: {Address} Error while connecting:\n {e.AsDebugFriendly()}");
    //             throw new OmniCoreRadioException(FailureType.RadioGeneralError, inner: e);
    //         }
    // }

        private async Task RecordRadioEvent(RadioEvent eventType,
            string text = null, byte[] data = null, int? rssi = null)
        {
             using var cts = new CancellationTokenSource(Defaults.DatabaseSingleWriteTimeout);
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
                 Logger.Warning("RLR: Error writing to the database", e);
             }
        }

        private async Task UpdateEntityName(string name)
        {
            if (Entity.DeviceName == name)
                return;

            using var cts = new CancellationTokenSource(Defaults.DatabaseSingleWriteTimeout);
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
                Logger.Warning("RLR: Error writing to the database", e);
            }
        }
    }
}