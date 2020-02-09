using System;
using System.Collections.Generic;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IDisposablesContainer
    {
        IList<IDisposable> Disposables { get; }
        void DisposeDisposables();
    }
}