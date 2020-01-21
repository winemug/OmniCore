using OmniCore.Model.Enumerations;
using System;
using System.Text;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreException : Exception
    {
        public FailureType FailureType { get;  }

        protected OmniCoreException(FailureType failureType, string message = null, Exception inner = null) : base(message, inner)
        {
            FailureType = failureType;
        }

        public override string ToString()
        {
            var errorString = new StringBuilder()
                .AppendLine($"Exception: {this.GetType().Name}")
                .AppendLine($"FailureType: {FailureType}")
                .AppendLine($"Error StatusMessage: {Message ?? "<none>"}")
                .AppendLine($"Stack trace: {this.StackTrace}");

            if (InnerException == null)
            {
                errorString.AppendLine($"Inner Exception: <None>");
            }
            else if (InnerException is AggregateException aggregateException)
            {
                //TODO:
            }
            else
            {
                //TODO:
            }
            return errorString.ToString();
        }
    }
}
