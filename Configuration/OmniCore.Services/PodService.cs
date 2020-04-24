using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;

namespace OmniCore.Services
{
    public class PodService : ServiceBase, IPodService
    {
        private readonly IPlatformFunctions PlatformFunctions;
        private readonly IBlePeripheralAdapter BlePeripheralAdapter;
        private readonly IContainer Container;
        //private readonly IDashPodProvider DashPodProvider;
        private readonly IErosPodProvider ErosPodProvider;
        private readonly IErosRadioProvider[] ErosRadioProviders;
        private readonly ILogger Logger;
        private readonly Dictionary<IPod, IDisposable> MonitoredPods;
        private readonly List<IErosRadio> MonitoredErosRadios;

        private IDisposable AdapterDisabledSubscription;
        private IDisposable AdapterEnabledSubscription;

        public PodService(
            IContainer container,
            IErosRadioProvider[] erosRadioProviders,
            IPlatformFunctions platformFunctions,
            IBlePeripheralAdapter blePeripheralAdapter,
            IErosPodProvider erosPodProvider,
            //IDashPodProvider dashPodProvider,
            ILogger logger
        )
        {
            Logger = logger;
            Container = container;
            ErosRadioProviders = erosRadioProviders;
            PlatformFunctions = platformFunctions;
            BlePeripheralAdapter = blePeripheralAdapter;
            ErosPodProvider = erosPodProvider;
            //DashPodProvider = dashPodProvider;
            MonitoredPods = new Dictionary<IPod, IDisposable>();
            MonitoredErosRadios = new List<IErosRadio>();
        }

        // public override async Task OnBeforeStopRequest()
        // {
        //     // foreach (var pod in await ActivePods(CancellationToken.None))
        //     // {
        //     //     var ar = pod.ActiveRequest;
        //     //     if (ar != null) ar.Cancel();
        //     // }
        // }

        public async Task<IEnumerable<IPod>> ActivePods(CancellationToken cancellationToken)
        {
            var erosPods = await ErosPodProvider.ActivePods(cancellationToken);
            // var dashPods = DashPodProvider.ActivePods(cancellationToken);
            var list = new List<IPod>();
            list.AddRange(erosPods);
            return list;
        }

        public Task<IEnumerable<IPod>> ArchivedPods(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IObservable<IErosRadio> ListErosRadios()
        {
            var serviceUuids = new List<Guid>();
            foreach (var radioProvider in ErosRadioProviders)
                serviceUuids.Add(radioProvider.ServiceUuid);

            return Observable.Create<IErosRadio>(observer =>
            {
                var cts = new CancellationTokenSource();

                BlePeripheralAdapter
                    .FindErosRadioPeripherals()
                    .Subscribe(async peripheral =>
                        {
                            var radio = await RadioFromPeripheral(peripheral, cts.Token);
                            if (radio != null) observer.OnNext(radio);
                        },
                        observer.OnCompleted, cts.Token);

                return Disposable.Create(() =>
                {
                    cts.Cancel();
                    cts.Dispose();
                });
            });
        }

        public async Task<IErosPod> NewErosPod(IUser user, IMedication medication, CancellationToken cancellationToken)
        {
            var pod = await ErosPodProvider.NewPod(user, medication, cancellationToken);
            await UpdateMonitoredPodList(cancellationToken);
            return pod;
        }

        public Task<IDashPod> NewDashPod(IUser user, IMedication medication, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            Logger.Debug("Starting pod service");
            await BlePeripheralAdapter.TryEnsureAdapterEnabled(cancellationToken);

            AdapterEnabledSubscription = BlePeripheralAdapter.WhenAdapterEnabled().Subscribe(_ =>
            {
                // if (AdapterStatusNotification != null)
                // {
                //     AdapterStatusNotification.Dispose();
                //     AdapterStatusNotification = ServiceFunctions.CreateNotification(
                //         NotificationCategory.RadioInformation, null, "Bluetooth is enabled.",
                //         TimeSpan.FromSeconds(30));
                // }
                Logger.Information("Bluetooth is enabled.");
            });

            AdapterDisabledSubscription = BlePeripheralAdapter.WhenAdapterDisabled().Subscribe(async _ =>
            {
                // AdapterStatusNotification?.Dispose();
                // AdapterStatusNotification = ServiceFunctions.CreateNotification(
                //     NotificationCategory.RadioImportant, "Bluetooth disabled", "Trying to enable bluetooth"
                //     , null, false);

                Logger.Warning("Bluetooth is disabled, trying to enable it automatically.");

                using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                if (!await BlePeripheralAdapter.TryEnableAdapter(timeoutSource.Token))
                {
                    Logger.Error("Failed to enable bluetooth automatically.");
                    // AdapterStatusNotification?.Dispose();
                    // AdapterStatusNotification = ServiceFunctions.CreateNotification(
                    //     NotificationCategory.RadioImmediateAction, "Couldn't enable bluetooth",
                    //     "Bluetooth is turned off, please turn it on manually.");
                }
            });

            await UpdateMonitoredPodList(cancellationToken);
            await UpdateMonitoredErosRadios(cancellationToken);

            Logger.Debug("Pod service started");
        }

        protected override Task OnStop(CancellationToken cancellationToken)
        {
            AdapterEnabledSubscription.Dispose();
            AdapterEnabledSubscription = null;

            AdapterDisabledSubscription.Dispose();
            AdapterDisabledSubscription = null;
            ErosPodProvider.Dispose();
            
            return Task.CompletedTask;
        }

        protected override Task OnPause(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task OnResume(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private Task<IErosRadio> RadioFromPeripheral(IBlePeripheral peripheral, CancellationToken cancellationToken)
        {
            return ErosRadioProviders.First(rp => rp.ServiceUuid == peripheral.PrimaryServiceUuid)
                .GetRadio(peripheral, cancellationToken);
        }


        private async Task UpdateMonitoredPodList(CancellationToken cancellationToken)
        {
            var activePods = await ErosPodProvider.ActivePods(cancellationToken);

            var inactivePods = MonitoredPods.Keys.Except(activePods).ToList();
            var newlyActivePods = activePods.Except(MonitoredPods.Keys).ToList();

            foreach (var inactivePod in inactivePods)
            {
                MonitoredPods[inactivePod].Dispose();
                MonitoredPods.Remove(inactivePod);
            }

            foreach (var newlyActivePod in newlyActivePods)
            {
                var archiveSub = newlyActivePod.WhenPodArchived().Subscribe(async _ =>
                {
                    await UpdateMonitoredPodList(cancellationToken);
                });

                if (newlyActivePod is IErosPod erosPod)
                {
                    var radioUpdSub = erosPod.WhenRadiosUpdated().Subscribe(async _ =>
                    {
                        await UpdateMonitoredErosRadios(CancellationToken.None);
                    });

                    MonitoredPods.Add(newlyActivePod, new CompositeDisposable { radioUpdSub, archiveSub, newlyActivePod });
                }
                else
                {
                    MonitoredPods.Add(newlyActivePod, new CompositeDisposable { archiveSub, newlyActivePod });
                }
                newlyActivePod.StartMonitoring();
            }
        }

        private async Task UpdateMonitoredErosRadios(CancellationToken cancellationToken)
        {
            var activePods = await ErosPodProvider.ActivePods(cancellationToken);

            var activeRadioEntities = activePods
                .Select(ap => ap.Entity)
                .SelectMany(ape => ape.PodRadios)
                .Select(apr => apr.Radio)
                .Distinct(new KeyEqualityComparer<RadioEntity>(r => r.Id))
                .ToList();

            var activeRadios = new List<IErosRadio>();
            foreach (var activeRadioEntity in activeRadioEntities)
            {
                var peripheral = await BlePeripheralAdapter.
                    GetPeripheral(activeRadioEntity.DeviceUuid, activeRadioEntity.ServiceUuid);
                var radio = await RadioFromPeripheral(peripheral, cancellationToken);
                activeRadios.Add(radio);
            }

            var inactiveRadios = MonitoredErosRadios.Except(activeRadios).ToList();
            var newlyActiveRadios = activeRadios.Except(MonitoredErosRadios).ToList();

            foreach (var inactiveRadio in inactiveRadios)
            {
                MonitoredErosRadios.Remove(inactiveRadio);
                inactiveRadio.Dispose();
            }

            foreach (var newlyActiveRadio in newlyActiveRadios)
            {
                MonitoredErosRadios.Add(newlyActiveRadio);
                newlyActiveRadio.StartMonitoring();
            }
        }
    }
}