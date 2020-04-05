using System;

namespace OmniCore.Model.Interfaces.Common
{
    public interface ILease<T> : IDisposable
    {
        T Instance { get; }
    }
}