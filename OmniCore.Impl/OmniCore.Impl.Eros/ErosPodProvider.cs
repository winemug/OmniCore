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
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Radio.RileyLink;

namespace OmniCore.Impl.Eros
{
    public class ErosPodProvider : IPodProvider<ErosPod>
    {
        private IRadioProvider[] _radioProviders;
        private IPodRepository<ErosPod> _podRepository;
        private IRadioAdapter _radioAdapter;
        private IPodRequestRepository<ErosRequest> _requestRepository;

        private Dictionary<Guid, ErosRequestProcessor> _requestProcessors;

        public ErosPodProvider(IRadioAdapter radioAdapter,
            IRadioProvider[] radioProviders, 
            IPodRepository<ErosPod> podRepository,
            IPodRequestRepository<ErosRequest> requestRepository)
        {
            _radioProviders = radioProviders;
            _radioAdapter = radioAdapter;
            _podRepository = podRepository;
            _requestRepository = requestRepository;

            _requestProcessors = new Dictionary<Guid, ErosRequestProcessor>();
        }

        public async Task<ErosPod> GetActivePod()
        {
            var pods = await _podRepository.GetActivePods();
            return pods.OrderByDescending(p => p.Created).FirstOrDefault();
        }

        public async Task<IEnumerable<ErosPod>> GetActivePods()
        {
            return (await _podRepository.GetActivePods())
                .OrderBy(p => p.Created);
        }

        public async Task Archive(ErosPod pod)
        {
            pod.Archived = true;
            await _podRepository.CreateOrUpdate(pod);
        }

        public async Task<ErosPod> New(IEnumerable<IRadio> radios)
        {
            var pod = new ErosPod
            {
                Id = Guid.NewGuid(),
                ProviderSpecificRadioIds = radios.Select(r => r.ProviderSpecificId).ToArray(),
                RadioAddress = GenerateRadioAddress()
            };
            await _podRepository.CreateOrUpdate(pod);
            return pod;
        }

        public async Task<ErosPod> Register(uint lot, uint serial, uint radioAddress, IEnumerable<IRadio> radios)
        {
            var pod = new ErosPod
            {
                Id = Guid.NewGuid(),
                Lot = lot,
                Serial = serial,
                RadioAddress = radioAddress,
                ProviderSpecificRadioIds = radios.Select(r => r.ProviderSpecificId).ToArray(),
            };
            await _podRepository.CreateOrUpdate(pod);
            return pod;
        }

        public IObservable<IRadio> ListAllRadios()
        {
            return Observable.Create<IRadio>((IObserver<IRadio> observer) =>
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

        public async Task QueueRequest(ErosRequest request)
        {
            try
            {
                lock(_requestProcessors)
                {
                    if (!_requestProcessors.ContainsKey(request.Pod.Id))
                    {
                        _requestProcessors.Add(request.Pod.Id, new ErosRequestProcessor(request.Pod.Id));
                    }
                }
                await _requestRepository.CreateOrUpdate(request);
                await TriggerProcessing(request.Pod.Id);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<ErosResult> ExecuteRequest(ErosRequest request)
        {
            await QueueRequest(request);
            return await GetResult(request, 0);
        }

        public async Task<ErosResult> GetResult(ErosRequest request, int timeout)
        {
            try
            {
                return new ErosResult(ResultType.OK);
            }
            catch (OperationCanceledException oce)
            {
                return new ErosResult(ResultType.Canceled, oce);
            }
            catch (Exception e)
            {
                return new ErosResult(ResultType.Error, e);
            }
        }

        public async Task<ErosResult> CancelRequest(ErosRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task CancelRequests(ErosPod pod)
        {
            throw new NotImplementedException();
        }

        public async Task CancelAllRequests()
        {
            throw new NotImplementedException();
        }

        public async Task TriggerProcessing(Guid podId)
        {
        }

        private void ProcessPodQueue(Guid podId)
        {
            try
            {

            }
            catch(Exception e)
            {
                throw;
            }
        }

        public Task<ErosPod> Register(ErosPod pod, IEnumerable<IRadio> radios)
        {
            throw new NotImplementedException();
        }

        public Task QueueRequest(IPodRequest<ErosPod> request)
        {
            throw new NotImplementedException();
        }

        public Task<IPodResult<ErosPod>> ExecuteRequest(IPodRequest<ErosPod> request)
        {
            throw new NotImplementedException();
        }

        public Task<IPodResult<ErosPod>> GetResult(IPodRequest<ErosPod> request, int timeout)
        {
            throw new NotImplementedException();
        }

        public Task<IPodResult<ErosPod>> CancelRequest(IPodRequest<ErosPod> request)
        {
            throw new NotImplementedException();
        }
    }
}
