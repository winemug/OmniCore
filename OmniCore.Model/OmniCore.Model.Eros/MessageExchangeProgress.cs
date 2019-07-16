using OmniCore.Mobile.Base;
using OmniCore.Model.Enums;
using OmniCore.Model.Eros.Data;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Eros
{
    public class MessageExchangeProgress : IMessageExchangeProgress
    {
        public bool CanBeCanceled { get; set; }
        public bool Waiting { get; set; }
        public bool Running { get; set; }
        public int Progress { get; set; }
        public bool Finished { get; set; }

        public IMessageExchangeStatistics Statistics { get; set; }
        public IMessageExchangeResult Result { get; set; }
        public IConversation Conversation { get; set; }
        public string ActionText { get; set; }

        public CancellationToken Token { get => Conversation.Token; }

        public MessageExchangeProgress(IConversation conversation, RequestType type, string parameters = null)
        {
            Conversation = conversation;
            Result = new ErosMessageExchangeResult(this) {
                Source = Conversation.RequestSource,
                Type = type,
                Parameters = parameters};
        }

        public void SetException(Exception exception)
        {
            Result.Success = false;
            var oe = exception as OmniCoreException;
            Result.Failure = oe?.FailureType ?? FailureType.Unknown;
            Result.Exception = exception;
            Conversation.Exception = exception;
        }
    }
}
