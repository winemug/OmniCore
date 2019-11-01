using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using SQLite;

namespace OmniCore.Eros
{
    public class ErosRequest : IPodRequest<ErosPod>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RequestType RequestType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RequestState RequestStatus { get; set; }

        [PrimaryKey]
        public Guid Id { get; set; }

        public Guid? ResultId { get; set;}

        public ErosPod Pod { get; set; }
        public DateTimeOffset Updated { get; set; }
        public DateTimeOffset Created { get; set; }

        [Ignore]
        public IPodRequestParameters Parameters { get; set; }

        public string ParametersJson
        {
            get
            {
                return Parameters?.ToJson();
            }
        }

        public DateTimeOffset? StartEarliest { get; set; }
        public DateTimeOffset? StartLatest { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; }
        public TaskCompletionSource<ErosResult> ResultSource { get; }
        public Task RequestTask {get; set;}

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
