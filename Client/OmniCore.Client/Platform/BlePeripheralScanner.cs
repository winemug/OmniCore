using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using Plugin.BluetoothLE;

namespace OmniCore.Client.Platform
{
    public class BlePeripheralScanner
    {
        private static TimeSpan ScanFrequencyWindow = TimeSpan.FromSeconds(30);
        private static int WindowedScanCountLimit = 5;

        private readonly List<Guid> ServiceIdFilter;
        private readonly ICoreLoggingFunctions Logging;
        private readonly ISubject<IScanResult> ScanResultSubject;
        private readonly ICoreApplicationFunctions ApplicationFunctions;
        private readonly SortedDictionary<DateTimeOffset, TimeSpan> ScanRecords;
        
        private int ScanSubscriberCount = 0;
        private IDisposable ScanSubscription;
        private IDisposable BluetoothLock;
        
        private bool OnPause = false;

        private ISubject<bool> ScanStateSubject;
        public IObservable<bool> WhenScanStateChanged => ScanStateSubject.AsObservable();
        
        public BlePeripheralScanner(
            List<Guid> serviceIdFilter,
            ICoreLoggingFunctions logging,
            ICoreApplicationFunctions applicationFunctions)
        {
            ServiceIdFilter = serviceIdFilter;
            Logging = logging;
            ApplicationFunctions = applicationFunctions;
            ScanResultSubject = new ReplaySubject<IScanResult>(TimeSpan.FromSeconds(10));
            ScanStateSubject = new BehaviorSubject<bool>(false);
            ScanRecords = new SortedDictionary<DateTimeOffset, TimeSpan>();
        }
        
        public IObservable<IScanResult> Scan()
        {
            return Observable.Create<IScanResult>(observer =>
            {
                AddScanSubscription();
                var subscription= ScanResultSubject.Subscribe(result => { observer.OnNext(result); });
                
                return Disposable.Create(() =>
                {
                    subscription.Dispose();
                    RemoveScanSubscription();
                });
            });
        }

        public void Pause()
        {
            lock (this)
            {
                if (OnPause || ScanSubscriberCount == 0)
                    return;

                ScanSubscription.Dispose();
                ScanSubscription = null;
                BluetoothLock.Dispose();
                BluetoothLock = null;
                
                ScanStateSubject.OnNext(false);

                OnPause = true;
                Logging.Debug($"BLES: Scan paused");
                Logging.Debug($"BLES: Total listening: {ScanSubscriberCount} Paused: {OnPause}");
            }
        }

        public void Resume()
        {
            lock (this)
            {
                if (!OnPause)
                    return;
                
                Logging.Debug($"BLES: Resuming scan");
                Logging.Debug($"BLES: Total listening: {ScanSubscriberCount} Paused: {OnPause}");
                StartScan();

                OnPause = false;
            }
        }
        private void AddScanSubscription()
        {
            lock (this)
            {
                var count = Interlocked.Increment(ref ScanSubscriberCount);

                Logging.Debug($"BLES: Incoming scan subscription");
                Logging.Debug($"BLES: Total listening: {count} Paused: {OnPause}");

                if (count == 1 && !OnPause)
                    StartScan();
            }
        }
        
        private void RemoveScanSubscription()
        {
            lock (this)
            {
                var count = Interlocked.Decrement(ref ScanSubscriberCount);
                Logging.Debug($"BLES: Scan subscriber removed");

                if (count == 0)
                {
                    if (!OnPause)
                    {
                        ScanSubscription.Dispose();
                        ScanSubscription = null;
                        BluetoothLock?.Dispose();
                        BluetoothLock = null;
                        ScanStateSubject.OnNext(false);
                        Logging.Debug($"BLES: Scan stopped");
                    }
                    OnPause = false;
                }
                Logging.Debug($"BLES: Total listening: {count} Paused: {OnPause}");
            }
        }

        private void StartScan()
        {
            ScanStateSubject.OnNext(true);
            
            BluetoothLock = ApplicationFunctions.BluetoothKeepAwake();
            Logging.Debug($"BLES: Scan start requested");

            ScanSubscription = SafeScanner()
                .Subscribe(result =>
                {
                    ScanResultSubject.OnNext(result);
                });
        }

        private IObservable<IScanResult> SafeScanner()
        {
            return Observable.Timer(GetNewScanWaitPenalty())
                .FirstAsync()
                .Select<long, IObservable<IScanResult>>(x =>
                    Observable.Create<IScanResult>(observer =>
                    {
                        var scanStart = DateTimeOffset.UtcNow;
                        ScanRecords[scanStart] = TimeSpan.Zero;

                        Logging.Debug($"BLES: Scan started");
                        var actualScan = CrossBleAdapter.Current
                            .Scan(new ScanConfig
                            {
                                ScanType = BleScanType.LowLatency,
                                AndroidUseScanBatching = false,
                                ServiceUuids = ServiceIdFilter
                            })
                            .Subscribe(result => { ScanResultSubject.OnNext(result); });

                        return Disposable.Create(() =>
                        {
                            ScanRecords[scanStart] = DateTimeOffset.UtcNow - scanStart;
                            actualScan.Dispose();
                        });
                    })).Switch();
        }

        private TimeSpan GetNewScanWaitPenalty()
        {
            var penalty = TimeSpan.Zero;
            var windowStart = DateTimeOffset.UtcNow - ScanFrequencyWindow;
            var count = ScanRecords.Count(sr => sr.Key >= windowStart);
            if (count >= WindowedScanCountLimit)
            {
                var penaltyScan = ScanRecords.TakeLast(WindowedScanCountLimit).First();
                var penaltyWindowEnd = penaltyScan.Key + penaltyScan.Value;
                var penaltyEnd = penaltyWindowEnd + ScanFrequencyWindow;

                penalty = penaltyEnd - DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
                Logging.Debug($"BLES: Postponing scan for {penalty.TotalSeconds:F0} seconds");

            }
            return penalty;
        }

  }
}