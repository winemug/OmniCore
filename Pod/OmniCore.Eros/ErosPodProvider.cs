using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model;
using OmniCore.Repository.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Radios.RileyLink;
using OmniCore.Repository.Entities;
using OmniCore.Repository;

namespace OmniCore.Eros
{
    public class ErosPodProvider : IPodProvider
    {
        private IRadioProvider[] _radioProviders;
        private IRadioAdapter _radioAdapter;
        private IBackgroundTaskFactory _backgroundTaskFactory;
        private Dictionary<long, ErosRequestProcessor> _requestProcessors;

        public ErosPodProvider(IRadioAdapter radioAdapter,
            IRadioProvider[] radioProviders,
            IBackgroundTaskFactory backgroundTaskFactory)
        {
            _radioProviders = radioProviders;
            _radioAdapter = radioAdapter;
            _backgroundTaskFactory  = backgroundTaskFactory;
            _requestProcessors = new Dictionary<long, ErosRequestProcessor>();
        }

        public async Task<Pod> GetActivePod()
        {
            using(var pr = new PodRepository())
            {
                var pods = await pr.GetActivePods();
                return pods.OrderByDescending(p => p.Created).FirstOrDefault();
            }
        }

        public async Task<List<Pod>> GetActivePods()
        {
            using(var pr = new PodRepository())
            {
                return (await pr.GetActivePods())
                    .OrderBy(p => p.Created).ToList();
            }
        }

        public async Task Archive(Pod pod)
        {
            using(var pr = new PodRepository())
            {
                pod.Archived = true;
                await pr.CreateOrUpdate(pod);
            }
        }

        public async Task<Pod> New(UserProfile up, List<Radio> radios)
        {
            using(var pr = new PodRepository())
            {
                var pod = new Pod
                {
                    UserProfileId = up.Id.Value,
                    PodUniqueId = Guid.NewGuid(),
                    RadioIds = radios.Select(r => r.Id.Value).ToArray(),
                    RadioAddress = GenerateRadioAddress()
                };
                return await pr.CreateOrUpdate(pod);
            }
        }

        public async Task<Pod> Register(Pod pod, UserProfile up, List<Radio> radios)
        {
            using(var pr = new PodRepository())
            {
                if (!pod.PodUniqueId.HasValue)
                    pod.PodUniqueId = Guid.NewGuid();
                pod.RadioIds = radios.Select(r => r.Id.Value).ToArray();
                return await pr.CreateOrUpdate(pod);
            }
        }

        public IObservable<Radio> ListAllRadios()
        {
            return Observable.Create<Radio>((IObserver<Radio> observer) =>
            {
                var disposables = new List<IDisposable>();
                foreach (var radioProvider in _radioProviders)
                {
                    disposables.Add(radioProvider.ListRadios()
                        .Subscribe(radio =>
                        {
                            observer.OnNext(radio);
                        }));
                }

                return Disposable.Create(() =>
                {
                    foreach(var disposable in disposables)
                        disposable.Dispose();

                });
            });
        }

        private uint GenerateRadioAddress()
        {
            var random = new Random();
            var buffer = new byte[3];
            random.NextBytes(buffer);
            uint address = 0x34000000;
            address |= (uint)buffer[0] << 16;
            address |= (uint)buffer[1] << 8;
            address |= (uint)buffer[2];
            return address;
        }

        private async Task<ErosRequestProcessor> GetProcessor(long podId)
        {
            if (!_requestProcessors.ContainsKey(podId))
            {
                using(var pr = new PodRepository())
                {
                    var pod = await pr.Read(podId);

                    var erp = new ErosRequestProcessor();
                    await erp.Initialize(pod, _backgroundTaskFactory, _radioProviders);
                    _requestProcessors.Add(podId, erp);
                }
            }
            return _requestProcessors[podId];
        }

        public async Task QueueRequest(PodRequest request)
        {
            try
            {
                var processor = await GetProcessor(request.PodId);
                await processor.QueueRequest(request);
            }
            catch (Exception e)
            {
                throw;
            }

        }

        public async Task<bool> WaitForResult(PodRequest request, int timeout)
        {
            var processor = await GetProcessor(request.PodId);
            return await processor.WaitForResult(request, 0);
        }

        public async Task<bool> CancelRequest(PodRequest request)
        {
            var processor = await GetProcessor(request.PodId);
            return await processor.CancelRequest(request);
        }

        public async Task<List<PodRequest>> GetActiveRequests(Pod pod)
        {
            var processor = await GetProcessor(pod.Id.Value);
            return await processor.GetActiveRequests();
        }

        public async Task<List<PodRequest>> GetActiveRequests()
        {
            var processors = new List<ErosRequestProcessor>();
            var list = new List<PodRequest>();
            lock(_requestProcessors)
            {
                foreach(var processor in _requestProcessors.Values)
                    processors.Add(processor);
            }

            foreach(var processor in processors)
                list.AddRange(await processor.GetActiveRequests());

            return list;
        }
    }
}
