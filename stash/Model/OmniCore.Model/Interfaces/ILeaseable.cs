﻿using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface ILeaseable<T>
    {
        bool OnLease { get; set; }
        Task<ILease<T>> Lease(CancellationToken cancellationToken);
        void ThrowIfNotOnLease();
    }
}