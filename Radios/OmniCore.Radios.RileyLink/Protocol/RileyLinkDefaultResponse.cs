using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkDefaultResponse : IRileyLinkResponse
    {
        public RileyLinkResult Result { get; set; }
        private ISubject<RileyLinkDefaultResponse> ResponseSubject = new Subject<RileyLinkDefaultResponse>();
        
        public void Parse(byte[] responseData)
        {
            try
            {
                Result = (RileyLinkResult) responseData[0];
                switch (Result)
                {
                    case RileyLinkResult.Timeout:
                        throw new OmniCoreRadioException(FailureType.RadioResponseTimeout);
                        break;
                    case RileyLinkResult.Interrupted:
                        throw new OmniCoreRadioException(FailureType.RadioResponseInterrupted);
                        break;
                    case RileyLinkResult.NoData:
                        throw new OmniCoreRadioException(FailureType.RadioResponseNoData);
                        break;
                    case RileyLinkResult.ParameterError:
                        throw new OmniCoreRadioException(FailureType.RadioResponseParameterError);
                        break;
                    case RileyLinkResult.UnknownCommand:
                        throw new OmniCoreRadioException(FailureType.RadioResponseUnknownCommand);
                        break;
                    case RileyLinkResult.Ok:
                        ParseInternal(responseData[1..]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                ResponseSubject.OnNext(this);
                ResponseSubject.OnCompleted();
            }
            catch (Exception e)
            {
                ResponseSubject.OnError(e);
                ResponseSubject.OnCompleted();
                throw;
            }
        }
        
        public bool SkipParse { get; set; }
        public IObservable<IRileyLinkResponse> Observable
        {
            get => ResponseSubject.AsObservable();
        }

        protected virtual void ParseInternal(byte[] responseData)
        {
        }
    }
}