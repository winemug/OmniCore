using System;

namespace OmniCore.Model.Interfaces
{
    public interface ILease<T> : IDisposable
    {
        T Instance { get; }
    }
}