using System;
using System.Collections.Generic;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IDisposableHandler
    {
        IList<IDisposable> Disposables { get; }
        void DisposeDisposables();
    }
}