using System;
using System.Collections.Generic;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IDisposablesContainer
    {
        IList<IDisposable> Disposables { get; }
        void DisposeDisposables();
    }
}