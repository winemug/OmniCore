using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace OmniCore.Model.Utilities.Extensions
{
    public static class CancellationTokenExtensions
    {
        public static CancellationTokenSource ToSourceWithTimeout(this CancellationToken cancellationToken,
            TimeSpan timeout)
        {
            var timeoutSource = new CancellationTokenSource(timeout);
            return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        }
    }
}
