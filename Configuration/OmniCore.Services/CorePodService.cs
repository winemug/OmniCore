﻿using System;
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
    public class CorePodService : CoreServiceBase, ICorePodService
    {
        private readonly ICoreApplicationFunctions ApplicationFunctions;
        private readonly IBlePeripheralAdapter BlePeripheralAdapter;
        private readonly ICoreContainer<IServerResolvable> Container;
        //private readonly IDashPodProvider DashPodProvider;
        private readonly IErosPodProvider ErosPodProvider;
        private readonly IErosRadioProvider[] ErosRadioProviders;
        private readonly ICoreLoggingFunctions Logging;
        private readonly ICoreNotificationFunctions NotificationFunctions;
        private IDisposable AdapterDisabledSubscription;


        private IDisposable AdapterEnabledSubscription;
        private ICoreNotification AdapterStatusNotification;

        public CorePodService(
            ICoreContainer<IServerResolvable> container,
            IErosRadioProvider[] erosRadioProviders,
            ICoreApplicationFunctions applicationFunctions,
            IBlePeripheralAdapter blePeripheralAdapter,
            IErosPodProvider erosPodProvider,
            //IDashPodProvider dashPodProvider,
            ICoreNotificationFunctions notificationFunctions,
            ICoreLoggingFunctions logging
        )
        {
            Logging = logging;
            Container = container;
            ErosRadioProviders = erosRadioProviders;
            ApplicationFunctions = applicationFunctions;
            BlePeripheralAdapter = blePeripheralAdapter;
            ErosPodProvider = erosPodProvider;
            //DashPodProvider = dashPodProvider;
            NotificationFunctions = notificationFunctions;
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
            Logging.Debug("Starting pod service");
            await BlePeripheralAdapter.TryEnsureAdapterEnabled(cancellationToken);

            AdapterEnabledSubscription = BlePeripheralAdapter.WhenAdapterEnabled().Subscribe(_ =>
            {
                if (AdapterStatusNotification != null)
                {
                    AdapterStatusNotification.Dispose();
                    AdapterStatusNotification = NotificationFunctions.CreateNotification(
                        NotificationCategory.RadioInformation, null, "Bluetooth is enabled.",
                        TimeSpan.FromSeconds(30));
                }
            });

            AdapterDisabledSubscription = BlePeripheralAdapter.WhenAdapterDisabled().Subscribe(async _ =>
            {
                AdapterStatusNotification?.Dispose();
                AdapterStatusNotification = NotificationFunctions.CreateNotification(
                    NotificationCategory.RadioImportant, "Bluetooth disabled", "Trying to enable bluetooth"
                    , null, false);

                using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                if (!await BlePeripheralAdapter.TryEnableAdapter(timeoutSource.Token))
                {
                    AdapterStatusNotification?.Dispose();
                    AdapterStatusNotification = NotificationFunctions.CreateNotification(
                        NotificationCategory.RadioImmediateAction, "Couldn't enable bluetooth",
                        "Bluetooth is turned off, please turn it on manually.");
                }
            });

            var activePods = await ErosPodProvider.ActivePods(cancellationToken);
            foreach (var erosPod in activePods)
            {
                ErosPodProvider.StartMonitoring(erosPod);
            }

            Logging.Debug("Pod service started");
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