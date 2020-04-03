using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkCommand<T> : IRileyLinkCommand
        where T : IRileyLinkResponse, new()
    {
        public RileyLinkCommandType CommandType { get; set; }
        public byte[] Parameters { get; set; }
        private T Response;

        private TaskCompletionSource<IRileyLinkCommand> SendCompletedSource;
        private ISubject<T> ResponseSubject;
        public bool HasResponse { get; private set; }

        public Task SubmitNoResponse(RileyLinkConnectionHandler connectionHandler)
        {
            HasResponse = false;
            SendCompletedSource = new TaskCompletionSource<IRileyLinkCommand>();
            connectionHandler.CommandQueue.Enqueue(this);
            return SendCompletedSource.Task;
        }

        public IObservable<T> Submit(RileyLinkConnectionHandler connectionHandler)
        {
            HasResponse = true;
            Response = new T();
            ResponseSubject = new AsyncSubject<T>();
            SendCompletedSource = new TaskCompletionSource<IRileyLinkCommand>();
            connectionHandler.CommandQueue.Enqueue(this);
            connectionHandler.TriggerQueue();
            return ResponseSubject.AsObservable();
        }

        public void SetTransmissionResult(Exception e)
        {
            if (e == null)
                SendCompletedSource.TrySetResult(this);
            else
                SendCompletedSource.TrySetException(e);
        }

        public void ParseResponse(byte[] data)
        {
            Response.Parse(data);
            ResponseSubject.OnNext(Response);
            ResponseSubject.OnCompleted();
        }

        //public Execute(IBlePeripheralConnection connectionHandler)
        //{
        //    var ret = new T();

        //    byte[] result = null;
        //    while (true)
        //    {
        //        ResponseEvent.Reset();
        //        if (Responses.IsEmpty)
        //        {
        //            using var responseTimeout = new CancellationTokenSource(Entity.Options.RadioResponseTimeout.Add(expectedProcessingDuration));
        //            using var linkedCancellation =
        //                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
                    
        //            await ResponseEvent.WaitAsync(linkedCancellation.Token);
        //        }
        //        else
        //        {
        //            if (Responses.TryDequeue(out result))
        //                break;
        //        }
        //    }

        //    if (result == null || result.Length == 0)
        //        throw new OmniCoreRadioException(FailureType.RadioInvalidResponse, "Zero length response received");

        //    var responseType = (RileyLinkResponseType)result[0];
        //    var response = new byte[result.Length - 1];
        //    Buffer.BlockCopy(result, 1, response, 0, response.Length);
        //    return (responseType, response);

        //}
    }
}
