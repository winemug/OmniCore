using System;
using System.Text;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreException : Exception
    {
        protected OmniCoreException(FailureType failureType, string message = null, Exception inner = null) : base(
            message, inner)
        {
            FailureType = failureType;
        }

        public FailureType FailureType { get; }

        public override string ToString()
        {
            var errorString = new StringBuilder()
                .AppendLine($"Exception: {GetType().Name}")
                .AppendLine($"FailureType: {FailureType}")
                .AppendLine($"Error StatusMessage: {Message ?? "<none>"}")
                .AppendLine($"Stack trace: {StackTrace}");

            if (InnerException == null)
            {
                errorString.AppendLine("Inner Exception: <None>");
            }
            else if (InnerException is AggregateException aggregateException)
            {
                //TODO:
            }

            return errorString.ToString();
        }
    }
}