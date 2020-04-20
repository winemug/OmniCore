using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;

namespace OmniCore.Services
{
    public class PodService : ServiceBase, IPodService
    {
        private readonly ICommonFunctions CommonFunctions;
        private readonly IBlePeripheralAdapter BlePeripheralAdapter;
        private readonly IContainer Container;
        //private readonly IDashPodProvider DashPodProvider;
        private readonly IErosPodProvider ErosPodProvider;
        private readonly IErosRadioProvider[] ErosRadioProviders;
        private readonly ILogger Logger;
        private readonly IServiceFunctions ServiceFunctions;
        private IDisposable AdapterDisabledSubscription;
        private IDisposable AdapterEnabledSubscription;

        public PodService(
            IContainer container,
            IErosRadioProvider[] erosRadioProviders,
            ICommonFunctions commonFunctions,
            IBlePeripheralAdapter blePeripheralAdapter,
            IErosPodProvider erosPodProvider,
            //IDashPodProvider dashPodProvider,
            IServiceFunctions serviceFunctions,
            ILogger logger
        )
        {
            Logger = logger;
            Container = container;
            ErosRadioProviders = erosRadioProviders;
            CommonFunctions = commonFunctions;
            BlePeripheralAdapter = blePeripheralAdapter;
            ErosPodProvider = erosPodProvider;
            //DashPodProvider = dashPodProvider;
            ServiceFunctions = serviceFunctions;
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

            // force get active pods to start them
            await ErosPodProvider.ActivePods(cancellationToken);

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

        public Task<IErosRadio> RadioFromPeripheral(IBlePeripheral peripheral, CancellationToken cancellationToken)
        {
            return ErosRadioProviders.First(rp => rp.ServiceUuid == peripheral.PrimaryServiceUuid)
                .GetRadio(peripheral, cancellationToken);
        }
    }
}