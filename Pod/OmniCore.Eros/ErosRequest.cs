using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OmniCore.Repository.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using SQLite;
using OmniCore.Repository.Entities;

namespace OmniCore.Eros
{
    public class ErosRequest : IDisposable
    {
        public TaskCompletionSource<bool> TaskCompletionSource { get; private set; }
        public CancellationTokenSource CancellationTokenSource { get; private set; }
        public IBackgroundTask BackgroundTask  { get; }
        public PodRequest Request { get; }
        public bool IsActive
        {
            get
            {
                if (TaskCompletionSource == null)
                    return false;

                return !TaskCompletionSource.Task.IsCompleted;
            }
        }
        public bool IsWaitingForScheduledExecution
        {
            get
            {
                return IsActive && BackgroundTask.IsScheduled;
            }
        }

        public ErosRequest(IBackgroundTaskFactory backgroundTaskFactory, PodRequest request)
        {
            BackgroundTask = backgroundTaskFactory.CreateBackgroundTask(async () => await ExecuteRequest());
            Request = request;
        }

        public void Dispose()
        {
            CancellationTokenSource?.Dispose();
        }

        public void Run()
        {
            TaskCompletionSource = new TaskCompletionSource<bool>();

            CancellationTokenSource?.Dispose();
            CancellationTokenSource = new CancellationTokenSource();

            if (Request.StartEarliest.HasValue)
            {
                BackgroundTask.RunScheduled(Request.StartEarliest.Value, true);
            }
            else
            {
                BackgroundTask.Run(true);
            }
        }

        public bool TryCancelScheduledWait()
        {
            if (BackgroundTask.IsScheduled && BackgroundTask.CancelScheduledWait())
            {
                TaskCompletionSource = null;
                return true;
            }
            return false;
        }

        private async Task ExecuteRequest()
        {
            try
            {

            }
            catch(OperationCanceledException oce)
            {

            }
            catch(AggregateException ae)
            {

            }
            catch(Exception e)
            {

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
