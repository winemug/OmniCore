using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;
using IUser = OmniCore.Model.Interfaces.Services.Facade.IUser;

namespace OmniCore.Services
{
    public class CorePodService : CoreServiceBase, ICorePodService
    {
        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly IErosRadioProvider[] ErosRadioProviders;
        private readonly IBlePeripheralAdapter BlePeripheralAdapter;
        private readonly ICoreApplicationFunctions ApplicationFunctions;
        private readonly IErosPodProvider ErosPodProvider;
        private readonly IDashPodProvider DashPodProvider;
        private readonly ICoreNotificationFunctions NotificationFunctions;
        
        private IDisposable AdapterEnabledSubscription;
        private IDisposable AdapterDisabledSubscription;
        private ICoreNotification AdapterStatusNotification;
        
        public CorePodService(
            ICoreContainer<IServerResolvable> container,
            IErosRadioProvider[] erosRadioProviders,
            ICoreApplicationFunctions applicationFunctions,
            IBlePeripheralAdapter blePeripheralAdapter,
            IErosPodProvider erosPodProvider,
            //IDashPodProvider dashPodProvider,
            ICoreNotificationFunctions notificationFunctions
            )
        {
            Container = container;
            ErosRadioProviders = erosRadioProviders;
            ApplicationFunctions = applicationFunctions;
            BlePeripheralAdapter = blePeripheralAdapter;
            ErosPodProvider = erosPodProvider;
            //DashPodProvider = dashPodProvider;
            NotificationFunctions = notificationFunctions;
        }
        
        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            await BlePeripheralAdapter.TryEnsureAdapterEnabled(cancellationToken);

            AdapterEnabledSubscription = BlePeripheralAdapter.WhenAdapterEnabled().Subscribe(_ => 
            {
                if (AdapterStatusNotification != null)
                {
                    AdapterStatusNotification.Dispose();
                    AdapterStatusNotification = NotificationFunctions.CreateNotification(
                        NotificationCategory.RadioInformation, null, "Bluetooth is enabled.",
                        TimeSpan.FromSeconds(30), true);
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
                        "Bluetooth is turned off, please turn it on manually."
                        , null, true);
                }
            });
            
            foreach (var pod in await ActivePods(cancellationToken))
            {
                await pod.StartMonitoring();
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public override async Task OnBeforeStopRequest()
        {
            foreach (var pod in await ActivePods(CancellationToken.None))
            {
                var ar = pod.ActiveRequest;
                if (ar != null)
                {
                    if (ar.CanCancel)
                        ar.RequestCancellation();
                }
            }
        }
        protected override async Task OnStop(CancellationToken cancellationToken)
        {
            AdapterEnabledSubscription.Dispose();
            AdapterEnabledSubscription = null;
            
            AdapterDisabledSubscription.Dispose();
            AdapterDisabledSubscription = null;
            
            foreach (var pod in await ActivePods(cancellationToken))
            {
                pod.Dispose();
            }
        }

        protected override async Task OnPause(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task OnResume(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IList<IPod>> ActivePods(CancellationToken cancellationToken)
        {
            var erosPods= await ErosPodProvider.ActivePods(cancellationToken);
            // var dashPods = DashPodProvider.ActivePods(cancellationToken);
            var list = new List<IPod>();
            list.AddRange(erosPods);
            return list;
        }

        public Task<IList<IPod>> ArchivedPods(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
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
                            var radio = await RadioFromPeripheral(peripheral,
                                cts.Token);
                            if (radio != null)
                                observer.OnNext(radio);
                        },
                        observer.OnCompleted, cts.Token);

                return Disposable.Create(() =>
                {
                    cts.Cancel();
                    cts.Dispose();
                });
            });
        }

        private Task<IErosRadio> RadioFromPeripheral(IBlePeripheral peripheral, CancellationToken cancellationToken)
        {
            foreach(var radioProvider in ErosRadioProviders)
                if (peripheral.PrimaryServiceUuid == radioProvider.ServiceUuid)
                {
                    return radioProvider.GetRadio(peripheral, cancellationToken);
                }
            return null;
        }

        public async Task<IErosPod> NewErosPod(IUser user, IMedication medication, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
        
        public async Task<IDashPod> NewDashPod(IUser user, IMedication medication, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}