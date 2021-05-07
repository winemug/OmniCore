using System;
using System.Reactive.Disposables;

namespace OmniCore.Model.Utilities.Extensions
{
    public interface ICompositeDisposableProvider : IDisposable
    {
        CompositeDisposable CompositeDisposable { get; }
    }
}