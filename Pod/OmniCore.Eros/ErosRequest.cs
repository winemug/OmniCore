using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OmniCore.Repository.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using SQLite;
using OmniCore.Repository.Entities;
using OmniCore.Repository;

namespace OmniCore.Eros
{
    public class ErosRequest : IDisposable
    {
        private TaskCompletionSource<bool> TaskCompletionSource;
        private CancellationTokenSource CancellationTokenSource;
        private IBackgroundTask BackgroundTask;
        private IBackgroundTaskFactory BackgroundTaskFactory;
        public PodRequest Request { get; private set; }
        private SemaphoreSlim RequestActionSemaphore;
        private IRadioProvider[] RadioProviders;
        private Pod Pod;
        private Radio[] Radios;

        public async Task<bool> IsActive()
        {
            return await ExecuteSynchronously( async () =>
            {
                if (TaskCompletionSource == null)
                    return false;

                return !TaskCompletionSource.Task.IsCompleted;
            });
        }
        public async Task<bool> IsWaitingForScheduledExecution()
        {
            return await IsActive() && BackgroundTask.IsScheduled;
        }

        public ErosRequest(IBackgroundTaskFactory backgroundTaskFactory, IRadioProvider[] radioProviders, PodRequest request)
        {
            RequestActionSemaphore = new SemaphoreSlim(1,1);
            BackgroundTaskFactory = backgroundTaskFactory;
            RadioProviders = radioProviders;
            Request = request;
        }

        public void Dispose()
        {
            //TODO
            CancellationTokenSource?.Dispose();
        }

        public async Task<bool> Run()
        {
            return await ExecuteSynchronously(
                async () =>
                {
                    TaskCompletionSource = new TaskCompletionSource<bool>();
                    CancellationTokenSource?.Dispose();
                    CancellationTokenSource = new CancellationTokenSource();
                    BackgroundTask?.Dispose();
                    BackgroundTask = BackgroundTaskFactory.CreateBackgroundTask(async () => await ExecuteBackgroundTask(CancellationTokenSource.Token));

                    using (var pr = RepositoryProvider.Instance.PodRepository)
                    {
                        Pod = await pr.Read(Request.PodId);
                    }
                    using (var rr = RepositoryProvider.Instance.RadioRepository)
                    {
                        Radios = new Radio[Pod.RadioIds.Length];
                        for(int i=0; i<Radios.Length; i++)
                            Radios[i] = await rr.Read(Pod.RadioIds[i]);
                    }

                    if (Request.StartEarliest.HasValue
                        && await BackgroundTask.RunScheduled(Request.StartEarliest.Value, true))
                    {
                        await UpdateStatus(RequestState.Scheduled);
                        return true;
                    }
                    else if (await BackgroundTask.Run(true))
                    {
                        return true;
                    }
                    TaskCompletionSource.TrySetResult(false);
                    return false;
                });
        }

        public async Task<bool> TryCancelScheduledWait()
        {
            return await ExecuteSynchronously(async () => 
                {
                    if (BackgroundTask.IsScheduled && await BackgroundTask.CancelScheduledWait())
                    {
                        await UpdateStatus(RequestState.Canceled);
                        TaskCompletionSource.TrySetResult(true);
                        return true;
                    }
                    return false;
                });
        }

        private async Task ExecuteBackgroundTask(CancellationToken cancellationToken)
        {
            try
            {
                // check request expiry
                ImmediateCancelIfRequested(cancellationToken);
                await ExecuteSynchronously(async () =>
                {
                    var now = DateTimeOffset.UtcNow;
                    if (Request.StartLatest.HasValue && Request.StartLatest.Value < now)
                    {
                        await UpdateStatus(RequestState.Expired);
                        TaskCompletionSource.TrySetResult(false);
                        return;
                    }
                    else
                    {
                        await UpdateStatus(RequestState.Initializing);
                    }
                });

                // get radio device
                ImmediateCancelIfRequested(cancellationToken);


            }
            catch (OperationCanceledException)
            {
                await ExecuteSynchronously(async () =>
                {
                    await UpdateStatus(RequestState.Canceled);
                    TaskCompletionSource.TrySetResult(true);
                });
            }
            catch (AggregateException)
            {
                await ExecuteSynchronously(async () =>
                {
                    await UpdateStatus(RequestState.Failed);
                    TaskCompletionSource.TrySetResult(false);
                });
            }
            catch (Exception)
            {
                await ExecuteSynchronously(async () =>
                {
                    await UpdateStatus(RequestState.Failed);
                    TaskCompletionSource.TrySetResult(false);
                });
            }
        }

        private async Task<IRadioConnection> GetRadioConnection()
        {
            var cts = new CancellationTokenSource();

            var leaseTasks = new List<Task<IRadioConnection>>();
            foreach(var radioProvider in RadioProviders)
            {
                foreach(var re in Radios)
                {
                    leaseTasks.Add(radioProvider.GetConnection(re, Request, cts.Token));
                }
            }

            IRadioConnection lease = null;
            while(lease == null && leaseTasks.Any())
            {
                var leaseTask = await Task.WhenAny(leaseTasks);

                if (leaseTask != null)
                {
                    lease = await leaseTask;
                    leaseTasks.Remove(leaseTask);

                    if (lease != null)
                    {
                        cts.Cancel();
                        leaseTasks.ForEach(async (lt) => (await lt)?.Dispose());
                        return lease;
                    }
                }
            }

            return null;
        }

        private void ImmediateCancelIfRequested(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();
        }

        private async Task<T> ExecuteSynchronously<T>(Func<Task<T>> function)
        {
            await RequestActionSemaphore.WaitAsync();
            try
            {
                return await Task.Run( () => function.Invoke());
            }
            catch
            {
                throw;
            }
            finally
            {
                RequestActionSemaphore.Release();
            }
        }

        private async Task<Task> ExecuteSynchronously(Action action)
        {
            await RequestActionSemaphore.WaitAsync();
            try
            {
                return Task.Run(() => action);
            }
            catch
            {
                throw;
            }
            finally
            {
                RequestActionSemaphore.Release();
            }
        }

        private async Task UpdateStatus(RequestState newState)
        {
            using(var prr = RepositoryProvider.Instance.PodRequestRepository)
            {
                this.Request.RequestStatus = newState;
                await prr.CreateOrUpdate(this.Request);
            }
        }

        //public async Task<IPodResult> Execute(IPod pod, IRadio radio)
        //{
        //    Parameters = ErosRequestParameters.FromJson(RequestType, ParametersJson);
        //    var execute = true;
        //    if (_cancellationSource.IsCancellationRequested)
        //        return new ErosResult(ResultType.Canceled);

        //    if (_executing)
        //        execute = false;
        //    else
        //        _executing = true;
        //    return await GetResult(pod, radio, execute);
        //}

        //public async Task<IPodResult> Cancel()
        //{
        //    if (!_executing)
        //    {
        //        _cancellationSource.Cancel();
        //        return new ErosResult(ResultType.Canceled);
        //    }

        //    if (!_cancellationSource.IsCancellationRequested)
        //        _cancellationSource.Cancel();
        //    return await GetResult(null, null, false);
        //}

        //private async Task<IPodResult> GetResult(IPod pod, IRadio radio, bool execute)
        //{
        //    try
        //    {
        //        if (execute)
        //        {
        //            var result = await OnExecute(pod, radio, _cancellationSource.Token);
        //            _resultSource.TrySetResult(result);
        //            return result;
        //        }
        //        else
        //        {
        //            return await _resultSource.Task;
        //        }
        //    }
        //    catch (TaskCanceledException tce)
        //    {
        //        _resultSource.TrySetCanceled(tce.CancellationToken);
        //        return new ErosResult(ResultType.Canceled);
        //    }
        //    catch (Exception e)
        //    {
        //        _resultSource.TrySetException(e);
        //        return new ErosResult(ResultType.Error, e);
        //    }
        //}

        //private async Task<IPodResult> OnExecute(IPod pod, IRadio radio, CancellationToken token)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Dispose()
        //{
        //    _cancellationSource?.Dispose();
        //}
    }
}
