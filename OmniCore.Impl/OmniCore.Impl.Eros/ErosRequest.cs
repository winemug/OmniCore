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

namespace OmniCore.Impl.Eros
{
    public class ErosRequest : IPodRequest, IDisposable
    {
        private readonly CancellationTokenSource _cancellationSource;
        private bool _executing = false;
        private readonly TaskCompletionSource<IPodResult> _resultSource;

        [JsonConverter(typeof(StringEnumConverter))]
        public RequestType RequestType { get; set; }

        [PrimaryKey]
        public Guid Id { get; set; }
        public Guid PodId { get; set; }
        public DateTimeOffset Created { get; set; }
        [Ignore]
        public IPodRequestParameters Parameters { get; set; }

        public string ParametersJson
        {
            get
            {
                return Parameters.ToJson();
            }
        }

        public ErosRequest()
        {
            _cancellationSource = new CancellationTokenSource();
            _resultSource = new TaskCompletionSource<IPodResult>();
        }

        public async Task<IPodResult> Execute(IPod pod, IRadio radio)
        {
            Parameters = ErosRequestParameters.FromJson(RequestType, ParametersJson);
            var execute = true;
            if (_cancellationSource.IsCancellationRequested)
                return new ErosPodResult(ResultType.Canceled);

            if (_executing)
                execute = false;
            else
                _executing = true;
            return await GetResult(pod, radio, execute);
        }

        public async Task<IPodResult> Cancel()
        {
            if (!_executing)
            {
                _cancellationSource.Cancel();
                return new ErosPodResult(ResultType.Canceled);
            }

            if (!_cancellationSource.IsCancellationRequested)
                _cancellationSource.Cancel();
            return await GetResult(null, null, false);
        }

        private async Task<IPodResult> GetResult(IPod pod, IRadio radio, bool execute)
        {
            try
            {
                if (execute)
                {
                    var result = await OnExecute(pod, radio, _cancellationSource.Token);
                    _resultSource.TrySetResult(result);
                    return result;
                }
                else
                {
                    return await _resultSource.Task;
                }
            }
            catch (TaskCanceledException tce)
            {
                _resultSource.TrySetCanceled(tce.CancellationToken);
                return new ErosPodResult(ResultType.Canceled);
            }
            catch (Exception e)
            {
                _resultSource.TrySetException(e);
                return new ErosPodResult(ResultType.Error, e);
            }
        }

        private async Task<IPodResult> OnExecute(IPod pod, IRadio radio, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _cancellationSource?.Dispose();
        }
    }
}
