using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface ILease<T> : IDisposable
    {
        T Instance { get; }
    }
}
