using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Common
{
    public interface ILease<T> : IDisposable
    {
        T Instance { get; }
    }
}
