using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface ILeaseable<T>
    {
        Task<ILease<T>> Lease(CancellationToken cancellationToken);
        bool OnLease { get; set; }
        void ThrowIfNotOnLease();
    }
}
