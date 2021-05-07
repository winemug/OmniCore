﻿using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkStandardResponse : IRileyLinkResponse
    {
        protected readonly ISubject<RileyLinkStandardResponse> ResponseSubject = new Subject<RileyLinkStandardResponse>();
        public RileyLinkResult Result { get; set; }

        public virtual void Parse(byte[] responseData)
        {
            try
            {
                Result = (RileyLinkResult) responseData[0];
                switch (Result)
                {
                    case RileyLinkResult.Timeout:
                        throw new OmniCoreRadioException(FailureType.RadioResponseTimeout);
                    case RileyLinkResult.Interrupted:
                        throw new OmniCoreRadioException(FailureType.RadioResponseInterrupted);
                    case RileyLinkResult.NoData:
                        throw new OmniCoreRadioException(FailureType.RadioResponseNoData);
                    case RileyLinkResult.ParameterError:
                        throw new OmniCoreRadioException(FailureType.RadioResponseParameterError);
                    case RileyLinkResult.UnknownCommand:
                        throw new OmniCoreRadioException(FailureType.RadioResponseUnknownCommand);
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

        public IObservable<IRileyLinkResponse> Observable => ResponseSubject.AsObservable();

        protected virtual void ParseInternal(byte[] responseData)
        {
        }
    }
}